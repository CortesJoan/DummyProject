# Shared VN integration audit — 2026-09-07

## What is shared now

- Actual execution engine: package `com.studio.vn-engine`, version **1.1.0**, assembly `VN.Engine`, class `VN.DialogueEngine`.
- Current source: `J:/Unity Projects/30 Days Of Silence/Packages/com.studio.vn-engine`.
- 30 Days of Silence consumes that embedded package. Memento Match references the **same directory**, not a copied engine, through its Packages/manifest.json.
- Presentation helpers remain the shared sidecar package `com.ehkp.visual-novel-presentation` 0.1.0, assembly `EHKP.VisualNovel.Presentation`.
- Both consumers currently declare Unity **6000.5.9f1**. This audit does not prove support on other Unity versions.

Before this audit, Memento referenced only the presentation helpers and owned a second linear cursor. That did not satisfy sharing the original technical engine.

## Game-specific boundary

MementoNarrativeRunner now maps MementoMatchStoryScene beats to typed VN.SayCommand objects and uses DialogueEngine.RunAllCommands. The engine owns progression and cursor. The adapter retains only scene content and presentation metadata. MementoMatchCampaign remains game content; none of its narrative is moved to the package.

The additive LoadCommands API avoids converting authored text into parser syntax. Isolated sessions opt out of the old static events, so Memento dialogue does not activate 30 Days audio handlers or global observers. The original constructor retains static broadcasts for compatibility.

Memento does not instantiate the VN DayManager, SaveLoadService, StoryState, gallery, minigame lab or legacy settings screens. Save/load controls remain absent in its story presentation; campaign progress remains its own game save.

## Content audit and limitations

The original package includes two small Resources UI-configuration assets: MainUiAtlas.asset and MiniGameAtlas.asset. They reference sprite GUIDs from 30 Days; the actual textures, portraits, voice clips and story script assets are outside this package and are not copied into Memento. Its MinigameLab scene is an editor sample and is not added to Memento's enabled scenes.

These UI configs and legacy DayManager/StoryState policies are existing compatibility baggage. They are not used by Memento, but must not be described as an entirely content-agnostic SDK. Their eventual split must preserve the original consumer's resource paths and GUIDs.

## Remaining physical separation — approval required

The current source is still physically embedded in 30 Days. It is therefore shared, but **not yet an independent workspace outside all games**.

Proposed narrowly scoped migration, not executed:
1. Register `J:/UnityPackages` as a shared package workspace with only the necessary package read/write scopes.
2. Snapshot both current package directories, preserving all .meta GUIDs and hashes.
3. Establish canonical directories at `J:/UnityPackages/Packages/com.studio.vn-engine` and `J:/UnityPackages/Packages/com.ehkp.visual-novel-presentation`.
4. Separate original-game Resources configurations and sample scene from reusable runtime, preserving 30 Days references. Do not migrate game art into the Circle repository.
5. Point both games' manifests at the canonical directories.
6. Move original embedded copies to a recoverable backup outside Packages only after references are validated. Embedded packages otherwise override manifest references.
7. Resolve/import both consumers, run both consumers' dialogue and presentation tests, and compare resolved package paths/GUIDs. No APK/player build is required.
8. Rollback: restore original manifests and embedded directories from snapshots. Do not delete old copies before acceptance.

The migration requires exact external-directory and gateway-registration authorization. A blocked permission check must not be bypassed through another root, shell or tool.

## Verification artifacts

- Package suite: VN.Tests.SharedDialogueEngineTests (typed/parsed execution, cursor, legacy compatibility, private events, replay, invalid inputs, reset).
- Memento suite: MementoSharedVnIntegrationTests (actual package resolution, scene/replay mapping, no global services, no narrative media assets).
- Existing VisualNovelPresentationTests continue covering identity and aspect classes.
- Full Editor and runtime results belong in the root closure checklist. This document does not claim tests or 30 Days consumer sessions were run merely because tests were added.
