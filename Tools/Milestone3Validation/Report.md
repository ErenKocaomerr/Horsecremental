# Milestone 3 implementation report

Implemented on 2026-10-05 in Unity 6000.6.2f1, using the project's installed Cinemachine 6.6.0 and existing art/UI assets. Milestone 4 has not been started.

## 1. Implemented gameplay

The Stable, carrot Farm, race entry, and Main Race now share a saved gameplay world. The player manages upgrades while a lightweight Training Race timer earns Coins automatically. A moving horse icon travels along its progress bar. The manual Main Race retains SPACE Boost, E Jump, five hurdles, finishing positions, reward protection, and progression-based player stats. It uses fixed stronger opponents. Returning from results keeps the same scene and progression model alive.

`PlayerProgression` remains the single source of truth for Coins, upgrade levels, collected/stored carrots, feeding speed, and idle timer progress. It has no UnityEngine imports; `IIdleSettings` keeps its configuration dependency independent of Unity. Unity adapters handle ticking, input, presentation, and cameras.

## 2. Scenes created, merged, and retained

- Created `Assets/Racing/Scenes/Milestone3World.unity`. It reuses the saved race hierarchy and clones the Stable environment into the same world, beside the track. There is a connecting path and small carrot plot.
- The world is the only enabled scene in Build Settings.
- Retained `Milestone1Race.unity` and `Milestone2Stable.unity`, including their GUIDs, as legacy reference/source scenes. Neither source scene was overwritten by the Milestone 3 builder. Their old navigation is superseded by the unified world.
- Retained SampleScene and the URP scene template. No scenes were deleted.
- The world has one physical Camera, one AudioListener, one EventSystem, and three Cinemachine cameras. The cloned Stable's duplicate camera/listener/event system were removed from the world.
- PixelPerfectCamera and CinemachinePixelPerfect were removed only from the new world. Their lens quantization conflicts with smooth blends and subtle orthographic zoom. Existing source scenes keep their original setup.

## 3. Scripts created

Runtime, under `Assets/Racing/Runtime/`:

- `IIdleSettings.cs`: pure C# settings contract for Training/Farm calculations.
- `WorldSettings.cs`: configurable Training, Farm, feeding, blend duration, and recommended power asset.
- `GameplayWorld.cs`: small same-scene gameplay state coordinator and A/D input.
- `WorldCameraNavigation.cs`: camera priorities/blending and modest racing lens feedback.
- `ManagementUI.cs`: Training/Farm presentation, collect/feed controls, power guidance, and area buttons.
- `CurrencyFeedback.cs`: persistent coin counter animation and gain/spend popup.
- `ButtonPulse.cs`: brief button scale response.

Editor, under `Assets/Racing/Editor/`:

- `Milestone3SceneSetup.cs`: saved world assembly, configuration, references, and build entry.
- `Milestone3UIBuilder.cs`: anchored management UI and correct Canvas scaling.

Tests:

- `Assets/Racing/Tests/Editor/Milestone3ModelTests.cs`: idle economy, farm, boost, balance, saved settings references, and world camera/Canvas configuration.

Unity generated the corresponding `.meta` files. Created `Assets/Racing/Config/World.asset` and the new world scene with their own GUIDs.

## 4. Scripts and existing assets modified

Runtime: `HorseParameters.cs`, `HorseStats.cs`, `HorseSimulation.cs`, `PlayerProgression.cs`, `RaceManager.cs`, `RacePrototype.cs`, `RaceCamera.cs`, `GameScenes.cs`, `StableUI.cs`, `RaceUI.cs`, and `RaceResultRow.cs`.

Editor: `Milestone2SceneSetup.cs` and `RaceSceneBaker.cs` place obsolete setup commands under the Legacy submenu.

Tests: `HorseSimulationTests.cs`, `PlayerProgressionTests.cs`, `Milestone2SceneTests.cs`, `ProgressionLoopTests.cs`, `RacePlayModeTests.cs`, and `RaceVisualProbe.cs`. Obsolete scene-loading/reward/boost assumptions were migrated to the new behavior. The visual probe now compares consecutive rendered frames rather than an inherited transform sampled before the first LateUpdate.

Existing configuration assets: `PlayerHorse.asset`, `RivalSteady.asset`, `RivalQuick.asset`, `RivalFast.asset`, and `Progression.asset`. Build Settings now enable the unified world. No packages or assembly definitions were changed. HorseMovement, HorseView, AIHorseController, ProgressionSession, UpgradeRule, RaceRewards, and the original art assets were reused without changes.

## 5. Stable ↔ Race camera movement

Use **D / RACE >** to browse the race entry and **A / < STABLE** to return during management. Cinemachine priority changes select a calm Stable camera or fixed race-entry view. The Brain uses **EaseInOut, 0.9 seconds**. The race's existing follow camera becomes active for an attempt.

START MAIN RACE prepares/reset participants and applies purchased/fed player stats, then allows 0.9 seconds for the camera to settle before the existing 3–2–1–GO countdown. During preparation/racing, area navigation is disabled. At the player's finish, results are immediately available. RETURN TO STABLE blends back; racers remain at their current positions until that blend finishes, then reset outside the active race view. There are no gameplay calls to SceneManager or scene loading.

The physical race origin stays at X=0, preserving HorseMovement/HorseView's existing absolute-distance assumptions. The Stable view is centered at X=-26, and the environment is offset so its horse/stall sit centrally in that view.

## 6. Training income calculation

Training succeeds automatically. Every **15 seconds of management time**, `AdvanceTraining` credits **35 Coins**, advances the race number, preserves any timer remainder, and immediately begins another race. This is **140 Coins per management minute**. It requires no spawned opponents or full background race simulation.

Race-area browsing is still management, so training continues there. It pauses during race preparation, countdown, running, results, and the return blend. The same timer resumes afterward. Training does not call BeginRace or consume Main Race payout tokens. Its smooth rendered-frame progress bar and horse icon communicate activity; timer text refreshes at 10 Hz.

## 7. Main Race difficulty configuration

The existing `PrototypeRace.asset` supplies the 420-unit track, five hurdles at 65/145/230/315/385, and references to the three fixed rival assets. Rival speeds are **14 / 14.5 / 15**, with acceleration **12**. They use the existing automatic jump timing and do not scale with the player or use Boost.

The Main Race remains available regardless of power. Stable guidance uses:

`Power = Speed * 10 + Acceleration * 0.25 + BoostPower`

Recommended power is 150 in World.asset. Below 80% of this recommendation displays VERY HARD; below 100% displays HARD; at/above it displays COMPETITIVE. This is rough guidance, not a gate or a promise of winning.

The deterministic balance test clears the same obstacles for both players/rivals and uses two player Boost taps per second. Fresh player: **33.88s**, slowest rival: **30.58s**, fastest rival: **28.62s**. Ten management minutes of training-funded purchases and feeding yielded speed 14.35 / BoostPower 8 and **23.32s**. The PlayMode loop also demonstrated a fresh fourth-place finish and two later first-place finishes without loading scenes.

## 8. Boost Meter behavior

Each SPACE press adds a small normalized energy amount. Holding SPACE does not repeat taps. Energy continuously drains while racing, with much faster drain at high energy. Energy multiplies BoostPower to determine speed bonus. The existing acceleration model approaches the resulting target speed smoothly. Acceleration and Boost Power upgrades still affect actual racing performance.

Boost Power upgrades leave tap gain and drain unchanged, so they do not make the meter easier to fill. Countdown/management/finished racers reject Boost and Jump requests. Race resets clear Boost, jump, penalty, and obstacle state.

## 9. Boost gain and drain formulas

Let `m` be energy in [0,1], with initial configuration in the HorseStats assets:

```text
On SPACE press: m = min(1, m + 0.06)
Drain per second: d(m) = 0.02 + 0.58 * m^3
During racing tick: m = max(0, m - d(m) * dt)
Speed bonus: m * BoostPower
Target speed: Base Speed + Speed bonus
Actual speed approaches target by Acceleration * dt
```

At 0/25/50/75/100% energy, the drain rates are approximately **2 / 2.91 / 9.25 / 26.47 / 60 percentage points per second**. At two taps/second, average incoming energy is 12 percentage points/second and the approximate steady level is 56%, with a small tap-to-tap oscillation. Maintaining near-full energy requires an impractically fast sustained tapping rate; high upgrades do not reduce that requirement.

The displayed bar interpolates with `1 - exp(-16 * unscaledDeltaTime)`. Its width updates each rendered frame. Accepted taps add a short vertical pulse. Above 75%, a subtle additional pulse appears; color shifts toward orange/red as energy rises.

## 10. Farm and carrot production

The Farm starts automatically. Every **20 seconds of active game time**, one carrot enters storage. Storage caps at **10**; production stops at capacity without banking unlimited catch-up time. The Farm continues during Main Races. COLLECT moves stored output into collected Carrots in the same progression model.

FEED HORSE spends **5 collected Carrots** for **+0.1 permanent speed for the current play session**. This adds to the player's base-speed calculation alongside Speed upgrades and is applied to the next Main Race attempt. It leaves rival stats, jump, Boost tap gain, and Boost drain unchanged. Collect/feed are available in the Stable management view. There are no extra crops, recipes, inventories, or offline gains.

## 11. Game feel and presentation

- 0.9-second Cinemachine area blends; smooth race follow remains in use.
- Brief camera settling before the three-second countdown.
- Countdown text scale response and large result emphasis.
- Small speed-driven orthographic lens change, plus restrained Boost/hit lens kicks.
- Smooth Boost UI, press pulse, high-energy pulse, and color feedback.
- Existing horse running animation, visual interpolation, Boost tint, and obstacle-hit red tint retained.
- Clear obstacle-hit UI message and existing short speed penalty retained.
- Shared button scale pulse; upgrades show before → after stats.
- Global floating/fading +Coins/-Coins feedback and a smoothed coin total.
- Training horse icon and smooth progress fill; smooth farm fill and collect/feed feedback.
- Management panels fade on area navigation and reject clicks when hidden.
- UI uses Scale With Screen Size at 1920x1080 with a 0.5 width/height match. Controls/panels use edge anchors; the difficulty panel clears the navigation at ultrawide ratios.

## 12. How to test the complete loop

1. Choose **Horsecremental → Open Milestone 3 World**, then press Play. The world is already assembled and saved; no setup command is needed for ordinary testing.
2. Observe the Training timer/icon. After 15 seconds, receive +35 Coins. Buy Speed, Acceleration, or Boost Power when affordable.
3. Observe a carrot appearing every 20 seconds. Collect output, then feed with five collected carrots. Check that Speed increases by 0.1.
4. Press D or RACE >. Watch the continuous camera slide toward the neighboring track. Press A or < STABLE to return before entering a race.
5. At the race view, click START MAIN RACE. After the camera settles, watch 3–2–1–GO. Tap SPACE rhythmically, around twice per second. Press E near each yellow approach marker.
6. A fresh horse should finish behind the fixed rivals. Check position, payout, visible coin feedback, and that Training progress did not advance during the attempt/results.
7. Click RETURN TO STABLE. After the blend, confirm Training resumes from its retained progress and Farm output can be collected.
8. Continue upgrading/feeding for several minutes and attempt again. The same rivals remain fixed while your horse becomes competitive. The model and accelerated PlayMode test demonstrated a win after ten management minutes.
9. Repeat races/returns and check that coins pay once, stats carry over, the scene remains Milestone3World, and participant/camera/listener counts stay constant.

For automated checks, run the `Horsecremental.Racing.Tests` EditMode assembly and `Horsecremental.Racing.PlayModeTests` PlayMode assembly in Unity Test Runner. PlayMode advances ten minutes of actual management timers at an accelerated time scale to avoid making a developer wait ten real minutes.

## 13. Prototype balance values

| System | Current value |
|---|---|
| Starting Coins / levels | 0 Coins; all three upgrades at level 1 |
| Player speed / acceleration / BoostPower | 10 / 12 / 5 |
| Training | Guaranteed 35 Coins / 15s |
| Main Race payouts, places 1–4 | 500 / 250 / 150 / 100 Coins |
| Fixed rival speeds | 14 / 14.5 / 15 |
| Track / hurdles | 420 units; five hurdles |
| Speed upgrade | 60 base cost, +0.75 speed |
| Acceleration upgrade | 50 base cost, +3 acceleration |
| Boost Power upgrade | 60 base cost, +1 maximum speed bonus |
| Upgrade price growth | 1.45 per purchased level |
| Boost tap / base drain / cubic drain coefficient | 0.06 / 0.02 / 0.58 |
| Farm | 1 carrot / 20s; storage cap 10 |
| Feeding | 5 carrots → +0.1 speed |
| Camera transition / countdown | 0.9s / 3s |
| Jump / gravity | 7 / 20 |
| Obstacle penalty | Speed multiplier 0.45; duration 1.2s |

## 14. Verification and known limits

The completed validation includes **25 passing EditMode tests** and **3 passing PlayMode tests**, with no skipped tests and no Console errors/warnings in the final checks. Detailed job results are saved in `Validation.json` alongside this report. Screenshots are saved under `Tools/Milestone3Validation/Screenshots/`, including Stable views at 1920x1080, 1280x720, 1024x768, and 2560x1080, plus race entry, racing, initial loss, upgraded Stable, and upgraded win.

PlayMode covers same-scene retries, duplicate-start rejection, reward accounting, Training pause/resume, actual timer-driven upgrades/feeding, fixed AI parameters, unchanged scene identity/object count, keyboard A/D/SPACE/E behavior, camera midpoint blending, obstacle penalties, and visual interpolation at 30/60/120 FPS.

Current limits: art remains placeholder; Training is an intentional simulation with an icon rather than a second rendered race; progression/feeding are runtime-only and reset on a fresh Play session; there is no offline income or save/load; power guidance is approximate; a standalone player build has not been produced. Balance is for a short prototype, not a finalized economy. Extremely fast tapping is still accepted, but ordinary sustained rhythms remain below maximum energy.

## 15. Obsolete systems and safe cleanup candidates

The old scene-loading flow has been removed from `GameScenes`. Its compatibility methods now navigate the loaded world. The original race and Stable scenes, their hidden old navigation controls, and Milestone 1/2 setup helpers are legacy references and are outside the active build. Legacy setup commands can still rebuild their own old scenes/build loop when explicitly selected; use the Milestone 3 world for current development.

After deciding that historical rebuild/reference support is no longer useful, remove the old scene-entry compatibility methods/constants, Milestone 1/2 scenes, and legacy builders/tests together after checking references. Do not delete shared horse sprites, generated sprite assets, the race settings, progression settings, AI/movement code, or UI helpers used by Milestone 3. Existing source scene GUIDs were deliberately preserved.

The Milestone 3 Setup command is a rebuild tool: it reconstructs the world from the retained sources and reinstates prototype balance. Use Open World for normal play and Inspector edits; running Setup after hand-editing the world replaces those scene edits.

## 16. Recommendation for Milestone 4

First add a small versioned save/load layer for the existing progression model, then one explicit Main Race victory unlock and a next fixed challenge. Validate the earn → upgrade → win pacing with real play sessions before expanding content. Equipment, crafting, breeding, additional crops, and broader championship structure can wait. None of this recommendation has been implemented.
