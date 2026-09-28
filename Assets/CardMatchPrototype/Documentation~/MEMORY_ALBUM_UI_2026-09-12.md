# Memory Album UI — 2026-09-12

## Art direction
Warm ivory paper, plum ink, mint secondary actions and coral primary action. Album/card-pair imagery relates directly to the memory loop; six keepsakes link to worlds. Circular portraits and round selection controls; rounded rectangular copy remains readable. Original characters and animal cards untouched. Legacy assets preserved but no longer used for main background/button/panel skin.

## Superseded background
The decorated v2 PNG below is retained for provenance only. The active UI now uses the clean v3 album base with exact in-game sprites added by Unity; see ALBUM_ORIGINAL_SPRITES_2026-09-12.md.

## Generated asset
- Built-in image generation tool (model selector not exposed by this tool; no specific model-version claim).
- Source: C:/Users/evilh/.codex/generated_images/01a04a73-9859-7371-8c47-73012a60beae/exec-896b6146-3583-47b4-981a-19f5738d7be0.png
- Runtime asset: Assets/Art/UI/MementoV2/lobby-memory-album-v2.png
- SHA256: 7a7049620da222e03e9cfdcf9803dcefd9c8679e54da228aa21bdce7fd1207d6

### Prompt
Use case: illustration-story. Asset type: final full-bleed 16:9 landscape background for Memento Match, a charming anime memory-pair puzzle game about collecting memories and teaming up. Create an original 2D hand-painted gouache and colored-pencil illustration with intentional bold silhouettes and restrained detail, absolutely not a realistic fantasy palace. Scene: a cozy open scrapbook resting on a pale mint picnic cloth, seen at a gentle overhead angle. On the RIGHT half a small open cream album holds two matching rounded cards with a simple coral flower emblem, with a few tiny paper keepsakes representing a leaf, a small gear, a crescent, a blossom, a candy and a clock tucked beside it. A thin coral bookmark curves organically; a couple of mint leaves enter from the outer edges. Composition: LEFT 55% is mostly calm warm ivory paper space, very low detail for live UI; right-middle contains the legible scrapbook focal point, all important objects inside the center 80% so phone crops remain pleasant. Palette warm ivory #fff4e6, muted mint #beddd0, restrained coral #e67975, warm plum outlines #463446, tiny butter-yellow highlights. Sunlit, playful, tactile, gentle anime slice-of-life mood. Flat cel-like shadows, deliberate imperfect pencil edges, no metal or bevels, no ornate frames, no dark green castle, no lens flare, no heavy bloom, no photorealism, no lettering, no interface, no logo, no watermark. Spacious high-quality cohesive illustration, not a collage of unrelated objects.

## Skill presentation
Minimum 3.6s, duration expands for subtitle/voice (bounded at 8s). Caption explains actual current rule separately from voiced subtitle. No duplicate voice subtitle box. Semantic emblem animates cards/constellation, rewind clock, shield, blossom or encore orbit. Legacy speed stripes removed from UXML. Card input and reveal coroutines wait for the reading interval without changing global time scale; Yoru's pair remains exposed 2.2s afterwards. Match cancellation clears pending holds.

## Verification
Game/shared-VN regression: 550/550 passed, 0 skipped (3a26bf9ec453421bb69775a7b5a43638), assemblies Assembly-CSharp-Editor, AnimalMemory.Effects.Tests, AnimalMemory.Progression.Tests and VN.Engine.Shared.Tests. Targeted reading/real-board regression also passed 15/15 (7afdee7f801f46eab510d4b835d321ef). The unfiltered suite did not finish cleanly: the separate EasyShop editor currency-generation test logged missing TestGemCurrency after compilation; no claim of a globally green suite.

Editor captures inspected for home landscape, team/story portrait and cut-in portrait/landscape. A cream-on-cream team heading was corrected and rechecked. Capture of cut-in was held for visual inspection only; timing/real effect covered separately by regression tests. Device acceptance remains to be checked on the owner's next build. No build, no progress reset, no ad SDK activation.
