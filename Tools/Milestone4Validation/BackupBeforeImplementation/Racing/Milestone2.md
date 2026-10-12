# Milestone 2 — Core incremental loop

## Implementation

Stable → Race → Coins → Stable → Speed / Acceleration / Boost Power upgrades → repeat.
One repeatable race, four racers including the player, one currency, and runtime-only progression.
The existing horse sprites and an existing environment background are reused. The Stable is intentionally a simple paddock.

## Created

- Runtime: `UpgradeRule`, `PlayerProgression`, `ProgressionSettings`, `ProgressionSession`, `RaceRewards`, `GameScenes`, `StableUI`.
- Editor: `CinemachineSetup`, `Milestone2SceneSetup`.
- Assets: `Config/Progression.asset`, `Scenes/Milestone2Stable.unity` and their meta files.
- Tests: `PlayerProgressionTests`, `Milestone2SceneTests`, `ProgressionLoopTests`.

## Modified

- `HorseMovement`: accepts calculated `HorseParameters` while retaining its original initialization overload and base `HorseStats` reference.
- `RacePrototype`: binds the runtime player parameters, rewards, and automatic countdown when arriving from the Stable.
- `RaceManager`: adds a race-start event; the movement, obstacle, countdown, ranking, and finish algorithms are unchanged.
- `RaceUI` and `RaceUIBuilder`: display place, reward and balance; add Return to Stable and retain debug restart.
- `RaceCamera`: replaces manual transform following with a Cinemachine binding/reset adapter.
- `HorseView`: runs interpolation before CinemachineBrain so the camera follows the current rendered position.
- `RaceSceneBaker`: preserves Cinemachine and progression integration when rebuilding the prototype.
- Assembly definitions: reference the existing Cinemachine package.
- Race, sample, and template scenes: camera conversion and Canvas scaling; build settings start with Stable, then Race.
- Existing PlayMode tests: isolate progression per test and share screenshot capture.
- `README.md`: documents the current Stable entry point and upgrade loop.

## Progression data

`ProgressionSession` holds one pure C# `PlayerProgression` instance across scene changes. It owns Coins and all three levels. The Stable and Race use the same `ProgressionSettings` asset. Stats are derived from the player's base `HorseStats` and purchased levels; the base asset and AI assets are never mutated. Entering Play resets the session even with domain reload disabled. No PlayerPrefs, disk saving, inventory, or additional currencies are implemented.

Purchases check affordability before changing data and raise one event after the complete transaction. UI refreshes from that event. Each race gets a session token; duplicate or abandoned tokens cannot award Coins.

## Economy configuration

Edit `Assets/Racing/Config/Progression.asset` in the Inspector. Levels start at 1.

`next cost = ceil(base cost × growth rate ^ (current level − 1))`

| Upgrade | First cost | Growth | Stat increase per purchase |
| --- | ---: | ---: | ---: |
| Speed | 60 | 1.45 | +0.75 |
| Acceleration | 50 | 1.45 | +3 |
| Boost Power | 60 | 1.45 | +1 |

Base stats remain 10 Speed, 12 Acceleration, and 5 Boost Power. Boost upgrades also scale the original +1-per-tap strength in proportion to the boost limit, while keeping the original decay, jump and obstacle rules. Prices and currency guard against integer overflow; upgrades stop when their price exceeds the integer range.

Rewards come from the finishing-position array: 1st = 100, 2nd = 70, 3rd = 50, 4th = 30 Coins. They are awarded as soon as the player crosses the finish, so returning before the remaining AI finish is supported. Result refreshes do not award again. Debug restart starts a new rewarded race.

## Cameras and UI

Unity Camera components remain as rendering outputs controlled by CinemachineBrain. Cinemachine cameras drive the Stable and race views; CinemachineFollow supplies race tracking and damping. The race retains its PixelPerfectCamera and adds the Cinemachine pixel-perfect extension. All saved scene Canvases use Scale With Screen Size, 1920×1080, with width/height matching at 0.5.

## Manual test

1. Open `Assets/Racing/Scenes/Milestone2Stable.unity` or use **Horsecremental → Open Milestone 2 Stable**, then press Play.
2. Confirm the horse, 0 Coins, base stats, and disabled upgrade buttons.
3. Click **YARIŞI BAŞLAT**. The race loads and starts its three-second countdown.
4. Tap SPACE repeatedly to stack boost; press E near yellow approach markers to jump orange hurdles. Automatic running and AI are unchanged.
5. Finish and check position, reward, and total Coins. Click **AHIRA DÖN**.
6. Repeat until an upgrade is affordable. Buy each upgrade and check the balance, level, new price, stats, and before/after feedback.
7. Start another race and compare speed, acceleration, and boost. Opponents retain their original stats.
8. Repeat the loop, including returning while AI are still racing and using debug restart. Insufficient funds must never reduce Coins.
9. Stop and re-enter Play: progression starts at 0 Coins and level 1 again.

**Horsecremental → Setup Milestone 2 Loop** regenerates the Stable and result UI, converts scene cameras/scalers, and configures build scenes. It preserves existing progression configuration. Existing race track and horse hierarchies are retained. Use it only when you intend to rebuild generated layouts; it replaces the generated Stable scene and race UI.

## Validation

**20/20 EditMode and 3/3 PlayMode tests passed**, including four complete scene loops and all upgrade categories. No unexpected gameplay logs occurred. See `Tools/Milestone2Validation/Validation.md` for executed test results and screenshots.

## Limitations and Milestone 3 recommendation

Progression resets when the game closes or Play stops, as requested. There is a single race, prototype economy/UI/environment, and a debug restart button. UI uses the project's existing legacy uGUI Text components. Very high upgrade levels eventually reach the protected currency/price limit.

For Milestone 3, add a small versioned save/load system for Coins and upgrade levels, then improve feedback and balance based on short playtests. No Milestone 3 work was started.
