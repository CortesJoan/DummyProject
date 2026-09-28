# Album: exact in-game illustrations

The decorative artwork is no longer regenerated. MementoAlbumDecorations asks CardMatchUI.GetCardSetPreview for the same cached Sprite objects as Collection/gameplay. The bear is repeated as a literal matching pair; other stickers use nautilus, crescent_moon, rose, swirl_cake and gear_train. No source card files are modified. Missing art hides its sticker instead of inventing a substitute. All elements ignore picking and keep square aspect ratios in both orientations.

## Blank base (built-in image edit)
Assets/Art/UI/MementoV2/lobby-memory-album-blank-v3.png
SHA256 b11de6b525ecde57e76b4570e9762198451ade6bd0d7cf0fc1ebe5fbdc6987a4
Source preserved: C:/Users/evilh/.codex/generated_images/01a04a73-9859-7371-8c47-73012a60beae/exec-5805aed5-7fc4-41a8-8982-d3eae76d3976.png
Only the previously generated album background was passed as an image-edit input. The game sprites were not sent to image generation. They are added by Unity locally, not baked into this PNG. The old v2 background is preserved but no longer referenced by the UI skin.

## Exact edit prompt
Use case: precise-object-edit. Edit target: the supplied generated scrapbook background. Remove ALL illustrated objects and decorations from this image: all flowers, foliage, leaves, petals, gear, moon, candy, watch, postcard and the two flower cards with their small album/frames. Leave the large open scrapbook pages entirely blank, including the right page. Preserve the existing camera angle, exact large book composition, wide 16:9 framing, warm ivory textured paper, subtle pencil/gouache style, coral fabric bookmark and mint gingham tablecloth. Replace removed items with the matching paper or cloth underneath. No new symbols, no drawings, no icons, no text, no characters. This is a quiet blank base; the game's existing original card sprites will be placed on top by the engine, not generated here. Keep the book clearly recognizable and fully inside the frame.

## Verification
18/18 focused tests passed (751082477f364101b276be35efda20b6): 8 album tests plus 10 skill-reading regressions. Tests cover direct sprite-reference identity, set coverage, input pass-through, missing assets and square in-bounds placement across resolutions. Runtime inspection confirmed ReferenceEquals=True for all seven decorations against CardMatchUI.GetCardSetPreview. Home captures inspected in landscape and portrait; lower stickers moved clear of the portrait training panel. No build or profile changes.
