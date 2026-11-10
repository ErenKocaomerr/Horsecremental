# Main Race environment art

The approved final artwork is applied to the existing Milestone3World scene's Generated Race/Prototype Track/Main Race Environment Art hierarchy.

- RaceTerrain.png: Dirt and Grass materials from the first preview.
- RaceBackdrop.png: tree-line strip from the first preview.
- RaceProps.png: trackside Fence, StartFlag, FinishFlag and Approach marker sprites are used. Earlier rejected hurdle/decor draft cells remain in the source atlas but are not placed.
- RaceHurdle.png: final approved unpainted timber hurdle from MainRace-v6. Scene presentation matches the final preview: 1.2-unit overall height, 75% horizontal scale, muted darker tint. This overall size represents the combination of lane depth and upright height; gameplay clearance remains 0.65.
- VergeDetails.png: sparse tiny grass, pebbles and flower sprites from MainRace-v3.

All imported sprites are persistent assets with stable IDs, Point filtering, no mipmaps and uncompressed source pixels. Source PNGs are retained without raster edits. Terrain and distant tree strips alternate their horizontal mirror direction so adjacent strips share the same source edge colors.

Reapply with **Horsecremental > Main Race Art > Apply to Current World**, with Milestone3World open and Play mode stopped. It replaces only the generated main-race art hierarchy and hides original primitive track renderers. The original track objects and references remain in the scene. Reapply after any legacy setup action that rebuilds the track.

The three pre-existing root-level Attached Horse Style Reference objects are deactivated, without deleting them or their assets, to prevent large static reference horses appearing on the live starting lanes.

The art belongs to the Main Race track, so existing race visibility switches also hide it when returning to Stable or using Training tracks. The 420-unit length, configured obstacle distances, 0.65 obstacle width/clearance, movement, jump, penalties, race camera, economy and training courses are unchanged. Start and finish flags are at the real 0 and 420 coordinates, unlike the compressed approval preview.

Scene backup: Tools/ArtPreviews/MainRace-v6/BeforeIntegration/Milestone3World.unity.
Generation prompts: Tools/ArtPreviews/MainRace-v1/GenerationPrompts.json, MainRace-v3/GenerationPrompts.json, MainRace-v6/GenerationPrompts.json. Source art was made with built-in ImageGen.
