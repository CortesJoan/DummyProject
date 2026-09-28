"""First-party write-only telemetry collector. Local by default; cloud deployment is explicit."""
import hashlib
import json
import os
import re
import sqlite3
import threading
import time
from datetime import datetime, timezone, timedelta
from wsgiref.simple_server import make_server

MAX_BODY = 65536
MAX_BATCH = 32
RETENTION_SECONDS = 14 * 86400
EVENTS = frozenset(("session_start session_end app_pause app_resume level_start level_win "
    "level_loss level_abandon level_checkpoint move_resolved skill_cast skill_effect "
    "campaign_progress menu_page story_beat story_skip story_end tutorial_start tutorial_complete").split())
IDS = ("eventId", "installationId", "sessionId")
STRINGS = ("name", "build", "platform")
NUMBERS = ("schema", "utcSeconds", "sequence", "level", "scene", "beat", "guide", "set",
    "turns", "pairs", "combo", "playerHealth", "rivalHealth", "value", "width", "height")
FIELDS = frozenset(IDS + STRINGS + NUMBERS)

def validate(payload, games, now):
    if not isinstance(payload, dict) or set(payload) != {"schema", "game", "events"}:
        raise ValueError("invalid envelope")
    if type(payload["schema"]) is not int or payload["schema"] != 1 or payload["game"] not in games:
        raise ValueError("invalid game or schema")
    events = payload["events"]
    if not isinstance(events, list) or not 1 <= len(events) <= MAX_BATCH:
        raise ValueError("invalid batch")
    for event in events:
        if not isinstance(event, dict) or set(event) != FIELDS:
            raise ValueError("unexpected event fields")
        if any(not isinstance(event[key], str) or not re.fullmatch(r"[0-9a-f]{32}", event[key]) for key in IDS):
            raise ValueError("invalid identifier")
        if event["name"] not in EVENTS:
            raise ValueError("unregistered event")
        for key, maximum in (("build", 64), ("platform", 32)):
            if not isinstance(event[key], str) or not re.fullmatch(r"[A-Za-z0-9_.+ -]{1," + str(maximum) + "}", event[key]):
                raise ValueError("invalid metadata")
        if any(type(event[key]) is not int for key in NUMBERS):
            raise ValueError("non numeric measurement")
        if event["schema"] != 1 or not now - RETENTION_SECONDS <= event["utcSeconds"] <= now + 300:
            raise ValueError("expired event")
        if not 1 <= event["sequence"] <= 10000000:
            raise ValueError("invalid sequence")
        if any(not -1 <= event[key] <= 1000000 for key in NUMBERS if key not in ("schema", "utcSeconds", "sequence")):
            raise ValueError("measurement out of bounds")
    return payload["game"], events

class SQLiteStore:
    """Local/persistent-volume option. Never use ephemeral Cloud Run disk for production."""
    def __init__(self, path):
        self.db = sqlite3.connect(path, check_same_thread=False)
        self.lock = threading.Lock()
        self.db.execute("CREATE TABLE IF NOT EXISTS events (game TEXT,event_id TEXT,received INTEGER,payload TEXT,PRIMARY KEY(game,event_id))")
        self.db.commit()
    def accept(self, game, events, now):
        with self.lock, self.db:
            self.db.execute("DELETE FROM events WHERE received < ?", (now - RETENTION_SECONDS,))
            for event in events:
                self.db.execute("INSERT OR IGNORE INTO events VALUES (?,?,?,?)",
                    (game, event["eventId"], now, json.dumps(event, separators=(",", ":"))))
        return [event["eventId"] for event in events]

class FirestoreStore:
    """Uses runtime service identity, never an exported JSON private key."""
    def __init__(self):
        from google.cloud import firestore
        self.db = firestore.Client()
    def accept(self, game, events, now):
        from google.api_core.exceptions import AlreadyExists
        accepted = []
        for event in events:
            document_id = hashlib.sha256((game + ":" + event["eventId"]).encode()).hexdigest()
            record = dict(event, game=game, received=now,
                expiresAt=datetime.fromtimestamp(now, timezone.utc) + timedelta(days=14))
            try:
                self.db.collection("gameEvents").document(document_id).create(record, timeout=5)
            except AlreadyExists:
                pass
            accepted.append(event["eventId"])
        return accepted

class Collector:
    def __init__(self, store, games, clock=time.time):
        self.store, self.games, self.clock = store, frozenset(games), clock
        self.limiter = {}
        self.lock = threading.Lock()
    def limited(self, key, now):
        # Best-effort abuse reduction, not authentication or a cloud-spending guarantee.
        with self.lock:
            minute = int(now // 60)
            self.limiter = {k: v for k, v in self.limiter.items() if v[0] == minute}
            if len(self.limiter) >= 4096 and key not in self.limiter:
                return True
            _, count = self.limiter.get(key, (minute, 0))
            self.limiter[key] = (minute, count + 1)
            return count >= 30
    def __call__(self, environ, start_response):
        def respond(status, body):
            encoded = json.dumps(body, separators=(",", ":")).encode()
            start_response(status, [("Content-Type", "application/json"), ("Content-Length", str(len(encoded))),
                ("Cache-Control", "no-store"), ("X-Content-Type-Options", "nosniff")])
            return [encoded]
        if environ.get("REQUEST_METHOD") == "GET" and environ.get("PATH_INFO") == "/health":
            return respond("200 OK", {"ok": True})
        if environ.get("REQUEST_METHOD") != "POST" or environ.get("PATH_INFO") != "/v1/events":
            return respond("404 Not Found", {"error": "not found"})
        try:
            size = int(environ.get("CONTENT_LENGTH", "0"))
            if size < 1 or size > MAX_BODY:
                return respond("413 Payload Too Large", {"error": "batch size"})
            if not environ.get("CONTENT_TYPE", "").startswith("application/json"):
                return respond("415 Unsupported Media Type", {"error": "json required"})
            payload = json.loads(environ["wsgi.input"].read(size))
            now = int(self.clock())
            game, events = validate(payload, self.games, now)
            if len({event["installationId"] for event in events}) != 1:
                raise ValueError("mixed installation")
            if self.limited((game, events[0]["installationId"]), now):
                return respond("429 Too Many Requests", {"error": "retry later"})
            accepted = self.store.accept(game, events, now)
            return respond("200 OK", {"acceptedEventIds": accepted})
        except (ValueError, TypeError, KeyError, UnicodeError):
            return respond("400 Bad Request", {"error": "invalid batch"})
        except Exception:
            # Never echo payloads, credentials, names or cloud exception internals.
            return respond("503 Service Unavailable", {"error": "retry later"})

_application = None
def application(environ, start_response):
    global _application
    if _application is None:
        games = [value.strip() for value in os.environ.get("GAME_IDS", "").split(",") if value.strip()]
        if not games:
            start_response("503 Service Unavailable", [("Content-Type", "application/json")])
            return [b'{"error":"game allowlist not configured"}']
        cloud = os.environ.get("TELEMETRY_STORE") == "firestore"
        if os.environ.get("K_SERVICE") and not cloud:
            start_response("503 Service Unavailable", [("Content-Type", "application/json")])
            return [b'{"error":"persistent cloud store required"}']
        store = FirestoreStore() if cloud else SQLiteStore(os.environ.get("TELEMETRY_SQLITE", "telemetry.sqlite"))
        _application = Collector(store, games)
    return _application

if __name__ == "__main__":
    # Local development only. Deploy the WSGI callable with a production server.
    make_server("127.0.0.1", int(os.environ.get("PORT", "8787")), application).serve_forever()
