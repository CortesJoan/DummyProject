# Minigames integration pilot — 2026-09-28

## Checkpoint
Memento baseline before integration: commit 0b6d49e, pushed to origin/development.
No build was generated. Existing postgame release flag and save format are unchanged.

## Canonical source and installation
The package inventory under J:/UnityPackages/Packages contained only the two VN packages.
The original framework remains at:
J:/Unity Projects/MinigamesSolidCleanCODE/Assets/Minigames/Core

That existing folder now has package.json (com.ehkp.minigames.core 0.1.0).
Memento references it as a local UPM package; no copy of the engine was created.
SerializeReferenceEditor is referenced from the same original project under Assets/SREditor/Package.
The core explicitly references Unity.ugui. Two SREditor editor files use EntityId on Unity 6000.5+, retaining the older InstanceID API on older editors.

## What this pilot actually migrates
MementoMemorySession owns an instance of the ORIGINAL Minigames.Core.Minigame:
- Ready -> Playing starts the board once.
- Win/Lose finish the shared session before campaign/result processing.
- Returning to menu or destroying the owner disposes the session without awarding a result.
- New attempts dispose the old session and cancel old board coroutines.
- Continuing after defeat creates a new session without redealing the board.
- Repeated terminal signals cannot award the same attempt twice.

No second TurnBasedGameLoop runs beside CardMatchUI.
Board rules, RNG, AI, powers, scoring and presentation retain their previous owners.
Generic session pause is NOT wired to board coroutines in this pilot: do not expose a pause control until that boundary is implemented/tested.
A runtime adapter alone is not a completed domain/presentation separation.

## Verification
- Shared Minigames.Core loaded in Unity 6000.5.9f1.
- 112/112 Minigames.Tests.EditMode passed (job 720f7489c9c74a06912dc04c0e3fe697).
- 9/9 adapter tests passed (job 72564df72f9f4a94bbe45b07d642a480).
- 69/69 Memento regression and adapter tests passed (c3446795ac70412b86c891dc7076c8f0): GOD, postgame, save roundtrips, alliance skills, rewarded continue and manager integration.
- Pre-existing postgame tests now explicitly configure their closed-release scenario and restore the previous flag. The development flag remains true.
- Shared source checkpoint pushed: 046b8f2 in CortesJoan/MinigamesSolidCleanCODE master.

## Remaining steps
1. Verify the real GameManager boundary in play: free match, campaign victory, defeat/continue, quit during reveal, quick retry.
2. Extract Memory board/turn rules behind a game-specific interface without changing their semantics.
3. Run a second non-memory example against the same host before generalizing campaign/skill APIs.
4. Make package installation portable and version-pinned before clean-machine release verification.
5. Audit original runner callback reentrancy and addon dependency validation before adopting those runners; this pilot uses Minigame directly and owns callback disposal.

## Recovery
The baseline is a reference checkpoint, not permission to reset user changes.
Remove only the pilot changes if rolling back; retain all unrelated modifications.
The repository still depends on external VN packages and an EasyShop junction; this commit is not a self-contained distribution.
