# Milestone 3 project audit and proposed changes

Date: 2026-10-05. Status: audit complete; implementation awaits the approval required by the user's change-control rule 5. No existing scripts, assets, scenes, or build settings have been edited.

## Verified baseline

- Unity Editor: 6000.6.2f1. Installed packages include Cinemachine 6.6.0, Input System 1.20.0, URP 17.6.0, and Unity Test Framework 1.8.0. No new package is needed for this milestone.
- Enabled build scenes: `Assets/Racing/Scenes/Milestone2Stable.unity`, followed by `Assets/Racing/Scenes/Milestone1Race.unity`. The Editor was idle in the race scene.
- `GameScenes` implements asynchronous scene loading in both directions. `StableUI` and `RaceUI` call it directly.
- `PlayerProgression` is a pure C# runtime model for Coins and three upgrade levels. `ProgressionSession` shares that model across scenes. `RaceRewards` protects payouts with race tokens. Retain these responsibilities and the existing reward protection.
- `RacePrototype.Start` binds the scene, initializes racers using progression stats, and creates the reward adapter. It currently applies player stats once per scene entry; same-scene retries will need to apply updated stats before each race.
- `RaceManager` already resets racers and runs a three-second countdown. It resolves same-tick finishes by interpolated crossing time. Reuse that logic.
- `HorseSimulation` already handles acceleration, jumping, swept obstacle checks, penalties, and finish/reset. `AIHorseController` only chooses jump timing. Opponent parameters are already independent of player progression.
- `HorseMovement` and `HorseView` write absolute X coordinates from race distance. Keep the existing track origin when assembling the world, or explicitly support a world origin in both adapters; moving only a parent would break their alignment.
- The Stable horse is currently a static SpriteRenderer decoration, separate from the racing participant. Training can be communicated using a small animated horse icon/progress indicator rather than adding another racing simulation.
- Each gameplay scene has its own physical Camera, AudioListener, EventSystem, CinemachineBrain, and CinemachineCamera. A direct merge must remove duplicate physical cameras, listeners, and event systems.
- Cinemachine brains in the four saved scenes use Cut with zero duration. `RaceCamera` also forces the virtual camera position on restart. Smooth transitions need nonzero blends and carefully ordered participant reset/camera selection.
- Both existing gameplay canvases already use Scale With Screen Size, 1920x1080, and a 0.5 width/height match. Sample/template scenes contain no CanvasScaler. Existing Stable controls predominantly use centered fixed offsets; the new panels need suitable anchors and layout checks.
- The gameplay cameras are orthographic; the race camera also uses PixelPerfectCamera/CinemachinePixelPerfect. Speed-based lens feedback should use a small orthographic-size change, with pixel-perfect compatibility checked visually, rather than perspective FOV changes that would have no useful effect here.
- Current Boost stores a speed bonus from 0 to BoostPower. Each tap adds 1 and drain is a constant 3 units/second at baseline. Progression increases both the ceiling and tap gain. The boost bar changes width at 10 Hz, causing discrete UI motion.
- Initial player: speed 10, acceleration 12, BoostPower 5. Rivals: speed 9.5, 10.1, and 10.4; acceleration 12. Track length: 420 with five hurdles.
- Upgrade rules: Speed costs 60 and adds 0.75; Acceleration costs 50 and adds 3; Boost Power costs 60 and adds 1. All prices grow by 1.45 per purchased level. Race payouts are 100/70/50/30.
- Baseline EditMode tests for HorseSimulation and PlayerProgression: **18 passed, 0 failed, 0 skipped** (Unity MCP job `a82ff3c95d0c4d8f805e75abaad59b31`). Console query before tests returned no errors or warnings. This does not constitute a complete PlayMode audit.
- Existing scene/loop tests encode Milestone 2 scene loading, exact old rewards, old boost formulas, and immediate boosted wins. Those expectations must be updated alongside the requested behavior rather than preserved as Milestone 3 acceptance criteria.

## Concrete implementation proposed for approval

1. Create `Assets/Racing/Scenes/Milestone3World.unity` using existing track, horse sprites, Stable environment, and UI helpers. Place Stable/Farm beside the existing track origin with a visible connecting area. Use one physical camera, listener, and EventSystem. Make the world the enabled gameplay build scene; retain original scenes and their GUIDs as references.
2. Add a small `GameplayWorld` coordinator with management, preparing, countdown, running, and finished states. Keep race-area browsing within management. A/D and arrow buttons select the Stable or race-entry Cinemachine view. Blend for about 0.9 seconds; a separate follow camera handles active racing. Reset participants while the race camera is inactive, then blend and start the existing countdown after the preparation delay. Results return to management without loading a scene. Avoid exposing participant resets as visible teleportation.
3. Reuse RaceManager, HorseSimulation, AI input, HorseView, RaceRewards, and the existing progression session. Reapply current player parameters before each attempt. Replace scene-loading UI callbacks with world actions. Ensure repeated starts, return transitions, and UI subscription changes cannot duplicate rewards or handlers.
4. Add configurable Training and Farm settings plus small pure C# timer models. Extend the existing PlayerProgression source of truth with carrots, stored farm output, and permanent feeding speed gain. Training credits Coins through that model without consuming Main Race payout tokens. Start with guaranteed training success and 35 Coins every 15 seconds. Pause and preserve training progress during preparation/countdown/running/results; resume on management return.
5. Set fixed Main Race rival speeds to 14/14.5/15 with suitable fixed acceleration. Configure payouts around 500/250/150/100. Preserve upgrade costs initially, then validate the intended 10–20 minute progression loop with simulation and gameplay. No player-dependent rival scaling or entry restriction.
6. Rework Boost as normalized energy `m` in [0,1]. Initial proposed tap gain is 0.06. Drain is `0.02 + 0.58*m*m*m` per second while energy is positive, clamped at zero. Speed bonus is `m * BoostPower`; existing acceleration smooths actual speed. At two taps/second, average input is 0.12/second and the approximate equilibrium is 56%; near maximum, drain approaches 60%/second. Upgrades increase the speed bonus, leaving tap gain and drain independent of upgrade level. All constants live in configuration. Update the displayed bar every rendered frame using smoothing and add a short press pulse/high-meter glow.
7. Farm automatically produces one carrot every 20 seconds into storage capped at 10. Collect transfers stored carrots into the model's carrot balance. Feed spends five collected carrots for +0.1 permanent speed. Production runs while gameplay is active and stops at capacity; no offline accrual, extra crops, or inventory system. Expose all balance values in a settings asset.
8. Add modest shared UI feedback: coin gain/spend popup, smoothed displayed total, upgrade button pulse and before/after stat text, carrot collection/feed response, countdown/result emphasis, smooth management panel visibility, and existing obstacle event feedback. Keep the Stable camera calm; use small racing lens feedback appropriate to the orthographic setup.
9. Audit every saved Canvas and camera, including inactive objects. Preserve existing correct scaler configuration. Verify layout at 1920x1080, 1280x720, 1024x768, and an ultrawide aspect ratio. Check camera blending for interaction with pixel-perfect rounding.
10. Update obsolete Milestone 2 test assumptions and add meaningful model/integration coverage for payout isolation, timer pause/resume, farm cap/collect/feed, boost dynamics, fixed opponent power, updated stats on retries, and repeated race loops in the same scene. Run EditMode and PlayMode checks, inspect screenshots and Console, and provide the requested implementation/balance/testing report. Stop at Milestone 3.

## Expected file scope

New runtime responsibilities: `GameplayWorld`, `WorldCameraNavigation`, Training/Farm configuration and timer models, management UI for training/farm, and lightweight currency/button feedback components. Keep components small; final filenames may be consolidated where that avoids unnecessary abstractions.

New editor/test responsibilities: a Milestone 3 scene setup MenuItem, model tests, and same-scene loop tests. The setup tool should preserve referenced assets and should not invoke old broad rebuild commands that regenerate working scenes.

Existing runtime files likely to change: `PlayerProgression`, `HorseParameters`, `HorseStats`, `HorseSimulation`, `RacePrototype`, `RaceManager`, `RaceCamera`, `StableUI`, `RaceUI`, and potentially `HorseMovement`/`HorseView` only where coordinate/reset or feedback support requires it. `GameScenes` becomes obsolete for the unified gameplay path; retain or remove it only after references are migrated. Reuse `ProgressionSession`, `UpgradeRule`, `RaceRewards`, and AI behavior with minimal changes where possible.

Existing asset/configuration changes: rival HorseStats, Progression payouts, boost configuration fields, and enabled Build Settings scenes. Existing scene files need edits only if a discovered Canvas/camera problem requires correction. Old Milestone 1/2 setup menus and scene-transition tests must be marked legacy or migrated so they cannot accidentally overwrite the unified world setup.

## Approval boundary

The user's instructions say: “Before modifying existing working code or core asset setups, explain the intended changes and obtain the user's approval.” This audit and proposal are the reviewable result preceding that approval. Implementation, scene assembly, and balance asset changes have not started.
