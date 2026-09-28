# Optional rewarded continuation — prepared, not activated

No advertising SDK, network traffic, ad account, banner, store upload or billing has been enabled.
The game remains fully playable without ads. A missing/unready provider hides the offer. Retry and menu are always free.

## Implemented reward
Once per duel attempt after a genuine defeat. Keeps pairs, score, turns, equipped skill/cooldown and rival damage; restores at least ceil(max health/2) and returns control to the player. An exhausted board deals an extra 2x2 round. Does not apply to the scripted introduction or tutorial.
New attempt resets the allowance; repeated defeat after a continue does not offer another.
Game progress is not saved or reset by continuing. A later earned victory follows the normal reward path.

## Later integration
Implement IRewardedContinueProvider with the chosen SDK in a separate adapter.
After GameManager.Awake, call GameManager.Instance.RewardedContinue.Configure(GameManager.Instance, provider).
IsReady must be false until initialization, appropriate privacy/age/consent handling and a loaded ad are ready.
Show is called only after explicit player opt-in.
Report RewardedContinueOutcome.Rewarded ONLY when the SDK confirms the reward was earned AND the fullscreen ad has closed. Closing without earning is Cancelled; no-fill is Unavailable; load/show errors are Failed.
Do not reward from an ad click, generic close event, timeout, or application-resume event.
SDK callbacks may arrive off-thread: MementoRewardedContinue queues completion onto Unity's main thread.
Duplicates and stale callbacks from previous requests/runs are ignored. A 180s watchdog abandons stalled requests without granting a reward.
The adapter must dispose its SDK event handlers and handle audio pause/resume, orientation, backgrounding and teardown. If the network supports server-side verification, add it to the adapter/backend before monetized deployment.
Configure after scene initialization; do not place production unit IDs in test builds.

## Release gates still required before real ads
Choose SDK/network and review its current terms; configure age/privacy consent and Play Data Safety; use official test ads on a physical device; test complete, skip, no-fill, offline, back/home, rotation, late and duplicate callbacks.
Never ship a simulated provider. Unit tests use fakes, but no fake provider is included in runtime.
No claim of SDK/device certification is made while the SDK is absent.
