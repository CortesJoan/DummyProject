# EHKP Game Analytics 0.1
Reusable embedded UPM package; no Memento Match, store SDK, Firebase SDK or VN dependency in its core.
The Unity adapter supports native Unity platforms; WebGL filesystem persistence and CORS need target-specific acceptance testing before claiming support.

## Current state
Opt-in is off by default. No endpoint has been configured, no cloud project created, no real events uploaded.
Without a server the settings screen explicitly says LOCAL diagnostic recording.
Consent is bound to the endpoint/purpose: local consent cannot silently become remote consent after an update.
Turn collection off to erase the local queue and pseudonymous installation ID. It does NOT delete already received remote records.
No player-entered name, account, advertising ID, device hardware ID, dialogue text or raw pointer coordinates.
Build/platform, viewport dimensions, random installation/session/event IDs and numeric gameplay state ARE collected. This is pseudonymous, not anonymous.

## Data and delivery
- Schema 1; stable eventId for retry deduplication.
- 512-event rolling queue, 14-day local retention. Old events can be dropped when offline; not lossless.
- Persisted after each event; 32 events per request. Retried with bounded exponential backoff.
- A successful HTTP response alone does not remove events: server must list acceptedEventIds.
- HTTPS only, no redirect, no URL credentials/query token. No service-account secret belongs in a game.
- Native storage uses a small JSON file in Application.persistentDataPath. These files are not encrypted.
- Killed apps may never send session_end; infer the last known state from checkpoints, not a fictional exact quit reason.
- Consent-based data has sampling bias; never interpret opt-out users as churn.

## Game mapping
MementoTelemetry lives outside this package.
session_start/end, app_pause/resume, tutorial_start/complete, menu_page, story_beat/skip/end,
level_start/win/loss/abandon/checkpoint (30 seconds), move_resolved, skill_cast/effect, campaign_progress.
Payload includes level/scene/beat/guide/set IDs, turns, pairs, combo, health, viewport and numeric event value.
Game rules never depend on analytics delivery. No rewards, ads, unlocks or saves depend on collector availability.

## Collector
Documentation~/receiver contains a WSGI receiver and tests.
Run tests: python -B -m unittest -v test_receiver.
Local development: set GAME_IDS to the exact Application.identifier, then python receiver.py.
SQLite is local/persistent-volume only. Cloud Run ephemeral disk is explicitly refused.
Cloud Run configuration: TELEMETRY_STORE=firestore and GAME_IDS allowlist. Service account needs only its dedicated Firestore database permissions; do not export keys.
Start command is in Procfile; requirements use major-version bounds and must be locked and scanned before production deployment.
Firestore records use hashed game+event ID keys and an expiresAt field. Configure a Firestore TTL policy separately or records will not automatically expire. TTL deletion may be billable.
Collector exposes only POST /v1/events and GET /health; no public data-reading route.
Rate limiting is best-effort per-process and installation IDs are client-supplied. They are NOT authentication. Before public deployment configure edge abuse controls/quotas, costs, and decide on client attestation. Treat all incoming metrics as untrusted.
Restrict origins for a future WebGL client; current collector does not enable CORS.
Default Cloud Run access logs can contain IP/user-agent even though our application does not store them; include hosting behavior in privacy review.

## Google Cloud deployment gate
User suggested Google Cloud. Its free allowance is not a promise of zero cost.
No billing account linked or cloud resources deployed by this work.
Review region, Cloud Run scale-to-zero/max instances, request quotas, Firestore retention/usage, build/registry charges and budget controls first.
Cloud Run spend caps are a limited Preview feature, not instant; Firestore/storage can still incur costs.
Official sources:
- https://cloud.google.com/run/pricing
- https://firebase.google.com/docs/projects/billing/firebase-pricing-plans
- https://docs.cloud.google.com/billing/docs/how-to/budgets-spend-caps
- https://docs.cloud.google.com/run/docs/quickstarts/build-and-deploy/deploy-python-service

When a reviewed endpoint exists, add Resources/ehkp-analytics.json with {"endpoint":"https://YOUR-HOST/v1/events"}.
That change requires fresh consent because the purpose differs from local-only logging.
Before release: privacy policy, in-app disclosure, Play Data Safety, applicable audience/consent requirements and remote deletion policy need owner approval.
