# Farm Expansion v2

Approved replacement artwork from `Tools/ArtPreviews/FarmAssetPack-v2` is integrated into the existing Milestone3World scene.

- Buildings: Stable, FeedShed, Blacksmith.
- Props: Fence, Gate, HayBale, WaterTrough, Oak, Bush, Rocks, FeedBucket, Soil, Sprout, Ready, Carrots (4 columns x 3 rows).
- Background: sparse lawn, tree line and path.

Reapply with **Horsecremental > Farm Expansion > Apply to Current World** while Milestone3World is open and Play mode is stopped. This action now uses v2 exclusively. The original horse sprite remains unchanged; its scene scale is adjusted to match the approved building/horse comparison. Legacy v1 artwork is retained in its original folder for rollback.

Sprites use Point filtering, no mipmaps, uncompressed pixels and full source resolution. Buildings and props are sliced by alpha bounds. Building cells use the approved sheet's uneven gutters. Logical drawing widths are 192 and 160 respectively, matched to the horse's 24 PPU before scene layout sizing.

The four gameplay crop stages use three art states: Soil, Sprout, Sprout, Ready. Crop logic, input, highlighting, harvest drops and race behavior are unchanged. The harvested carrot sprite is also used by the existing pickup pool.

Pre-v2 scene backup: `Tools/ArtPreviews/FarmAssetPack-v2/BeforeIntegration/Milestone3World.unity`. Prompts: `Tools/ArtPreviews/FarmAssetPack-v2/GenerationPrompts.json`. Source artwork was produced with built-in ImageGen.
