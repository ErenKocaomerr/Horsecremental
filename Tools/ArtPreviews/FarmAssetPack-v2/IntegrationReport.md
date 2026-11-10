# Approved v2 integration

Applied the user's approved replacement set to the existing Milestone3World scene on 2026-11-10. New source assets live in Assets/Sprites/Farm Expansion v2. The previous scene was copied into BeforeIntegration and v1 artwork retained at its original paths.

Updated the existing FarmExpansion importer for the v2 three-building and 4x3 twelve-prop sheets. Removed decoration references to the v1-only pine, barrel and horseshoe. Preserved full approved texture resolution, Point filtering, no mipmaps and uncompressed sprites. Adjusted the existing stable horse's transform scale for the approved relative proportions without changing its sprite image.

The scene's generated environment references v2 Background.png, Buildings.png and Props.png exclusively. Crop visuals map to Soil / Sprout / Sprout / Ready for the four model stages, with v2 carrot pickups. Existing gameplay and economy code remains unchanged.

Validation:

- Unity compilation succeeded.
- Existing PlayMode StableMouseInputTests.MouseClicksGrowHarvestAndHoverCollectsWithoutAnotherClick: 1 passed, 0 failed (2.134 seconds). This exercises crop growth, harvest, physical drops, hover collection and UI blocking.
- Play mode initialized in StableManagement with overlay UI visible. No console errors or warnings were present after the runtime capture.
- Explicit stage-art check: grown plot uses Ready; successful harvest resets it to Soil; source is the v2 prop atlas.
- InGame.png is the actual running Game view including overlay UI, captured through Unity ScreenCapture.CaptureScreenshot.

Play mode stopped after validation. Integrated scene saved. HorseScaleComparison.png remains the earlier approval composition; InGame.png is the integrated gameplay screenshot.
