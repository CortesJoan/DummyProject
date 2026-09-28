import io
import json
import sqlite3
import unittest
from receiver import Collector, SQLiteStore, validate

def event():
    return dict(eventId="a"*32, installationId="b"*32, sessionId="c"*32,
        name="level_start", build="test", platform="test", schema=1, utcSeconds=2000000,
        sequence=1, level=0, scene=-1, beat=-1, guide=-1, set=0, turns=0, pairs=0,
        combo=0, playerHealth=4, rivalHealth=4, value=0, width=1080, height=1920)
def envelope(value=None):
    return {"schema":1,"game":"test.game","events":[value or event()]}

class CollectorTests(unittest.TestCase):
    def setUp(self):
        self.store = SQLiteStore(":memory:")
        self.app = Collector(self.store, ["test.game"], lambda: 2000000)
    def tearDown(self): self.store.db.close()
    def send(self, payload, path="/v1/events", method="POST"):
        body=json.dumps(payload).encode()
        status=[]
        response=self.app({"REQUEST_METHOD":method,"PATH_INFO":path,"CONTENT_LENGTH":str(len(body)),
            "CONTENT_TYPE":"application/json","wsgi.input":io.BytesIO(body)},
            lambda code, headers: status.append(code))
        return status[0],json.loads(b"".join(response))
    def test_accepts_and_deduplicates_retry(self):
        for _ in range(2):
            code, body=self.send(envelope())
            self.assertEqual(code,"200 OK")
            self.assertEqual(body["acceptedEventIds"],["a"*32])
        self.assertEqual(self.store.db.execute("SELECT COUNT(*) FROM events").fetchone()[0],1)
    def test_rejects_unregistered_game(self):
        p=envelope();p["game"]="another.game"
        self.assertEqual(self.send(p)[0],"400 Bad Request")
    def test_rejects_unexpected_private_field(self):
        e=event();e["playerName"]="not collected"
        self.assertEqual(self.send(envelope(e))[0],"400 Bad Request")
    def test_rejects_unknown_event(self):
        e=event();e["name"]="anything"
        self.assertEqual(self.send(envelope(e))[0],"400 Bad Request")
    def test_no_public_read_endpoint(self):
        self.assertEqual(self.send({},method="GET")[0],"404 Not Found")
    def test_rejects_out_of_range_numbers_and_bools(self):
        for value in [True,-99999999,"one"]:
            e=event();e["pairs"]=value
            self.assertEqual(self.send(envelope(e))[0],"400 Bad Request")
    def test_rejects_oversized_batch(self):
        p=envelope();p["events"]*=33
        self.assertEqual(self.send(p)[0],"400 Bad Request")
    def test_limits_repeated_installation(self):
        for _ in range(30): self.send(envelope())
        self.assertEqual(self.send(envelope())[0],"429 Too Many Requests")
    def test_expiration(self):
        e=event();e["utcSeconds"]=0
        self.assertEqual(self.send(envelope(e))[0],"400 Bad Request")
    def test_store_failure_does_not_acknowledge(self):
        self.store.db.close()
        self.assertEqual(self.send(envelope())[0],"503 Service Unavailable")
        self.store.db=sqlite3.connect(":memory:")
if __name__=="__main__": unittest.main()
