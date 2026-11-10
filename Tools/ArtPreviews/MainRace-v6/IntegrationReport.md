# Approved Main Race integration

Applied the final approved v6 timber hurdle, v1 terrain/backdrop/fence/flags and v3 sparse verge details to the existing Milestone3World scene. The scene backup is BeforeIntegration/Milestone3World.unity. Source PNGs are under Assets/Sprites/Main Race Expansion.

Added two editor-only setup/import files; no runtime game code changed. Main Race Environment Art is a child of the existing main track and follows its existing visibility switching. Original primitive renderers are disabled while their objects/references are retained. Training tracks and Stable art are preserved.

20 timber hurdle sprites are placed at the 5 configured obstacle distances in all 4 lanes. RaceSettings retains length 420, distances 65/145/230/315/385 and width/clearance 0.65. The approved visual spans lane depth as well as upright height; gameplay collision remains the pure C# obstacle model. Real start/finish artwork is at 0/420. All new sprites are persistent imported assets, Point-filtered, uncompressed and have no mipmaps. Long terrain strips are mirrored alternately and overlap by 0.03 units to cover subpixel cracks.

The 3 pre-existing root Attached Horse Style Reference objects were deactivated, preserving their objects and assets, after the real start view showed large static duplicate horses on the lanes.

Validation:

- Unity compiled the importer/setup successfully.
- Verified 600 generated art objects and 20 hurdle visuals with persistent sprite references.
- PlayMode: RacePlayModeTests.KeyboardNavigationBlendsAndSpaceAddsEnergyOnlyPerPress passed (navigation, input, jumping and missed-hurdle penalty).
- PlayMode: ProgressionLoopTests.PhysicalStableSurvivesTwoMainRacesWithoutPassiveTraining passed (two completed main races, zero obstacle hits with timed jumps, rewards and Stable return).
- Combined test result: 2 passed / 0 failed, 18.088 seconds.
- Runtime capture at player distance 53.13 near the first obstacles: MainRaceRunning, zero hits, no console errors/warnings. InGameRace.png includes the actual overlay UI. Player is unassisted in this capture, so faster AI rivals have moved ahead of the viewport.
- InGameRace-final.png is the subsequent visual check after adding the small strip-edge overlap. Both captures use the real Main Race camera and overlay UI.
- Validation race-result captures were copied into Validation; historical tracked capture files were restored byte-for-byte from HEAD after the test run overwrote them.

The captured composition differs from the approval overview: the real race camera retains its existing lens/follow settings and the start and finish remain 420 units apart. Earlier draft hurdles, large bottom decorations and red/cream hurdle versions are not placed.

Reapply: Horsecremental > Main Race Art > Apply to Current World with Milestone3World open and Play stopped. Reapply after using an older world/track rebuild menu.
