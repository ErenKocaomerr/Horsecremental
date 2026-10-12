# Manual Training Race demo

Open `Assets/Racing/Scenes/Milestone3World.unity` and press Play. This remains the single enabled gameplay scene.

Click **TRAINING** to choose Training I, II or III. Only I starts unlocked. Finishing 1st unlocks the next race; every finish grants Coins and Training Tokens. Use SPACE Boost and E Jump. Return to Stable and spend Training Tokens on Speed, Acceleration or Boost Power. Coins remain for Stable / Farm economy. Passive multiple-horse lap production and its purchases are inactive.

Training races reuse the existing controller, participants and Cinemachine camera with short courses and fixed rivals. Main Race remains a manual entry with its original course and rivals. Day/night and automatic Main Race scheduling are not implemented.

Farm retains automatic growth, click acceleration, physical harvest drops, hover collection, Coin upgrades and Carrot feeding. Blacksmith remains ambient-only. Canvases use Scale With Screen Size at 1920x1080.

Tune rewards and initial token upgrade prices in `Config/TrainingLadder.asset`; tune courses / rivals in the three TrainingRace assets and nine Training rival assets. **Horsecremental > Setup Manual Training Races** reapplies scene/UI setup while preserving tuning. Reapply after changing lengths or obstacles. Existing world / Playground setup actions preserve the migration when the ladder asset is present.

Progress lasts only for the current Play session. K grants debug Coins and legacy Main Race Tokens; it does not grant Training Tokens or unlock Training tiers.

Run EditMode and PlayMode suites in Unity Test Runner. The complete script list, dependency audit, balance, test evidence, manual checks and limitations are in `Tools/TrainingRaceValidation/Report.md`; screenshots are alongside that report.

Horse sprites: existing Horse Pack by loota9, CC BY 4.0; see `Assets/Sprites/Horse Pack/Horse Pack/Read Me.txt`. Historical milestone documents and backend models remain for compatibility and development history.
