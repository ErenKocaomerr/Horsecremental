# Farm Expansion

Approved ImageGen preview artwork is integrated into `Assets/Racing/Scenes/Milestone3World.unity`.

- `Buildings.png`: Stable, FeedShed, Blacksmith; individually named sprites.
- `Props.png`: 16 named environment, crop, and resource sprites.
- `Background.png`: farm terrain backdrop.

Use **Horsecremental > Farm Expansion > Apply to Current World** with Milestone3World open and Play mode stopped to reapply. The action updates only the new art hierarchy, original placeholder renderer visibility, farm positions/art, and carrot pickup sprites. It preserves the existing race, UI, economy, crop model, and original art assets. Rerun after a legacy world setup rebuilds placeholders.

Textures use Point filtering, no mipmaps, and uncompressed sprites. Source files retain their original resolution; building and prop import resolution is reduced to 512 to suit the small horse sprites. Building slicing reads full source dimensions before applying this reduction.

`FarmCropAppearance` subscribes to crop stage events and updates the six plot sprites without frame polling. Existing FarmWorldView retains input, growth, harvesting, highlighting, and feedback behavior.

Generated art is a preview-quality pixel-art approximation, not a hand-authored uniform native pixel grid. Background paths and soil are baked into the image, not a tilemap. Original generation prompts are in `Tools/ArtPreviews/FarmAssetPack-v1/GenerationPrompts.json`.

Pre-integration scene copy: `Tools/ArtPreviews/FarmAssetPack-v1/BeforeIntegration/Milestone3World.unity`. Original scene GUID/meta was retained.
