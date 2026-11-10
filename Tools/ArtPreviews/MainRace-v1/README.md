# Main Race minimalist artwork preview

Generated using built-in ImageGen, referencing the existing brown horse and approved Farm Expansion v2 background. Exact generation prompts are in GenerationPrompts.json.

- RaceTerrain.png: tan dirt and sage grass materials. The requested four-cell layout was generated as two broad material areas; use the left half for earth and right half for grass. This is not a validated seamless tile atlas.
- RaceProps.png: 4 columns x 3 rows: fence, hurdle, hay bale, distance stake; start pennant, checkered finish flag, arrow sign, jump marker; bush, rocks, bench, flowers.
- RaceBackdrop.png: wide tree-line and grass strip. Seamless repetition is not guaranteed.
- HorseReference.png: unchanged existing brown idle sprite rendered with nearest-neighbor filtering.
- MainRacePreview.png: static Unity-rendered composition using the existing four racers, actual lane spacing (1.7), and hurdle clearance (0.65). Start and finish are compressed into one image to show the art together; the real race remains 420 units long. Overlay UI is omitted for art review.
- RenderPreview.cs.txt: temporary preview rendering code for Unity MCP execute_code; does not import assets or edit/save the gameplay scene.

Generated PNGs retain some smooth color variation and transparency fringes; they are not guaranteed native-grid hand-authored sprites. Full source files are preserved. The final preview uses each terrain material as a continuous area to avoid obvious tile seams.

No Main Race assets, scene references, colliders, obstacle distances, balance, race logic, or build settings have been changed. Integration awaits approval, following the existing preview/approval workflow.
