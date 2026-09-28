# Enamel Adventure UI v4

## Direction from user references
Character-first mobile game composition, illustrated navigation, one prominent story action, compact status and training controls. Sapphire/turquoise enamel with restrained champagne edges and light text replaces cream paper panels. No copied logos, copyrighted game characters, currencies, stamina systems or promotional clutter. Existing world backgrounds follow the selected guide; no new generic palace or album.

The square backgrounds reported by the user were UI backing plates, not failed transparency in the card sprites. Removed those plates from MementoAlbumDecorations and stopped instantiating the album layer in the new lobby. Original card textures are unmodified.

## Assets / built-in image tool
Assets/Art/UI/MementoV2/Resources/MementoNavigationV4.png
sha256 ba48583bb23657fe8749c36c96aae9b4c1857b4887052a85d8f4b90cf066759d
Assets/Art/UI/MementoV2/Resources/MementoQuestV4.png
sha256 f95e7c7c7471f93050d8a2c1116215f3bbae536416057fa4d27624d3d466dc69
Original outputs preserved in C:/Users/evilh/.codex/generated_images/01a04a73-9859-7371-8c47-73012a60beae/ (exec-6af21c03-f7e0-40e7-bb67-a03f0828c4cf.png and exec-56371c6d-f3ab-4b00-a907-17575030e028.png).
No model selector is exposed by the built-in tool; no specific version claimed. PNGs have real alpha: GPU readback at 64x64 reported all four corners alpha=0; navigation 2321/4096 transparent samples, quest 1491/4096.

### Navigation prompt
Use case: ui-mockup. Asset type: production sprite sheet for ORIGINAL anime mobile game navigation, transparent background. A single horizontal row of FIVE completely separate equal-sized circular enamel badges, each centered inside its own equal-width cell. Wide 3:1 canvas, five columns, one row; generous transparent gutters, no overlaps, nothing cut off. Exact left to right subjects: small welcoming fantasy house; folded adventure map with a coral flag; two friendly stylized profile silhouettes denoting a team; open card collection binder with two matching star cards; mechanical settings cog. Art direction: hand-painted crisp anime mobile JRPG UI icons, rich midnight sapphire and turquoise enamel, restrained satin champagne-metal outline with two or three broad ornamental curves, warm coral highlights. Strong chunky silhouettes readable at 56 pixels, professional game HUD assets, playful not grimdark. Consistent round medallion silhouette and scale across all five cells. No text, numbers, letters, labels, brand marks, characters from existing games, white backdrop, checkerboard pattern, drop shadows outside the badge, or rectangular tiles. True transparent alpha outside each badge; complete cutout icons.

### Quest prompt
Use case: ui-mockup. Asset type: one circular main quest button frame for an original anime memory-puzzle RPG. 1:1 square canvas, single centered complete round medallion, true transparent alpha outside. Match a refined hand-painted anime mobile JRPG HUD: midnight sapphire/turquoise enamel, champagne satin metal edge and a tiny coral jewel at the top, only a few broad elegant ornamental curves. Wide empty dark-blue inner disc reserved for a 2-line white live text label, NO icon or symbol in the central 65% diameter. Thin outer ring with two small stylized winglike flourishes at sides, dimensional enamel highlights, crisp silhouette readable at 130 pixels. Entire ornament inside 90% canvas with transparent margin. No words, no letters, no characters, no white background, no checkerboard, no drop shadow beyond alpha, no photorealistic metal, no baroque black/gold frame. This is a ready-to-use isolated videogame UI sprite, not a mockup screen.

## Skill staging
MementoSkillCinematic borrows the actual owner's ability pose, including Rei when copying techniques. Eased 0.48-second entrance and afterimage, convergent impact shards, drifting highlights and final 0.25-second fade/exit. Integrated voiced subtitle and explicit rule caption retain the existing 3.6–8-second reading policy and defer board reveals until after the announcement. Old semantic glyph is no longer instantiated. No ability rules/cooldowns changed.

## Verification
Game/shared-VN regression 566/566 passed, zero failures/skips (5a0b26630035427186e6ba5df785a121). Assemblies: Assembly-CSharp-Editor, AnimalMemory.Effects.Tests, AnimalMemory.Progression.Tests and VN.Engine.Shared.Tests. This does not claim unrelated EasyShop editor tests are green. Home and full-pose skill captures checked in landscape/portrait; portrait Team checked for readable contrast. Removed outline from small home labels after visual review (kept only on the headline). No builds, no profile/progression changes. Screenshots held beyond normal duration for layout checks only; reading policy regression tests cover normal timing.
