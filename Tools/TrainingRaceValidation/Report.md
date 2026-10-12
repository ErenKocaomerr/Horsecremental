# Manual Training Races — 6 October 2026

Implemented only the three-tier manual Training ladder. Day/night, automatic Main Race scheduling, recovery and comeback bonuses are outside this change.

## Loop and balance

Training I starts unlocked. Finish 1st to unlock the immediately following tier. Other places still earn both currencies. Best place never regresses on replay. Rewards below are ordered **1st / 2nd / 3rd / 4th**.

| Tier | Track length | Fixed rival Speed | Rival Acceleration | Coins | Training Tokens |
| --- | ---: | --- | ---: | --- | --- |
| Training I | 120 | 9.8 / 10.2 / 10.6 | 10 | 100 / 75 / 50 / 30 | 6 / 4 / 3 / 1 |
| Training II | 150 | 11 / 11.5 / 12 | 12 | 160 / 120 / 80 / 48 | 10 / 7 / 4 / 2 |
| Training III | 180 | 13 / 13.5 / 14 | 14 | 240 / 180 / 120 / 72 | 16 / 10 / 6 / 3 |

Training I has no hurdles, II has one at 55, and III has two at 55 / 120. Main Race retains its 420-unit course and original rivals. The automated Training I win took about 10.43 seconds of racing, plus the existing countdown and transition.

`Assets/Racing/Config/TrainingLadder.asset` configures all tier rewards and horse token prices. `TrainingRace1.asset` through `TrainingRace3.asset` configure length, obstacles and fixed rival references. Nine Training rival HorseStats assets contain Speed / Acceleration and other stats. Opponents never scale with player power.

Initial horse prices: Speed **3**, Acceleration **2**, Boost Power **3** Training Tokens. Cost grows as `ceil(baseCost × 1.35^(level−1))`. Real stat increases remain +0.75 Speed, +3 Acceleration and +1 Boost Power per level. All three stat categories are available in the active manual demo; Coins still pay Farm / Stable production and economy unlocks.

## Central progression and race reuse

`PlayerProgression` owns Training Tokens, their change event, best places and unlocks through the pure C# `PlayerTraining` partial. Horse purchases atomically debit Tokens in manual mode. Standalone legacy models retain their previous Coin purchases for compatibility; the active scene always enables manual mode.

Training and Main Races share a monotonically increasing claim ID. A Training result must match its active claim and tier, can pay once, and cannot be paid through Main Race rewards. Abandoned claims cannot pay later. Training grants neither Metal nor legacy Race Tokens. Main Race rewards are retained; their separate Race Tokens are hidden from the active demo HUD.

The existing RacePrototype, RaceManager, four participants, HorseController, AIHorseController, HorseSimulation, RaceUI and Cinemachine race camera handle both race kinds. Entry selects the course and fixed opponents; return restores Main Race settings. Three prebuilt course visuals switch in place. No new gameplay scene or second racing controller was introduced.

## Passive dependency audit

- `GameplayWorld` stops advancing passive Training in manual mode. `PlayerProgression.AdvanceTraining` independently guards other callers.
- Passive lap reward subscriptions are omitted in manual mode and removed when enabling it after Playground initialization.
- Training Horse Count, Lap Speed and Lap Reward purchases are blocked in the central model. Their physical track, view and purchase panel are disabled in the saved scene. Their tree nodes / connections are hidden.
- Dependency-heavy legacy classes, enum values, fields, settings and existing GUIDs are retained. No referenced script was deleted.
- `CropGrowth` now validates its own time steps without relying on a helper inside `TrainingProduction`. Physical crops, click growth, harvest drops and hover collection still work.

## UI and setup

The contextual selector shows tier name, lock state, best finish, Coin range, Token range and the previous-tier win requirement. Locked buttons are disabled. Results show both rewards and the next unlock. Horse cards show level, current → next stat, token cost and affordability; the HUD displays Training Tokens separately. Canvas Scalers retain Scale With Screen Size, 1920×1080, and anchors.

`Horsecremental > Setup Manual Training Races` is an idempotent focused scene migration that preserves tuned assets. Reapply after changing course lengths / obstacles to rebake visuals. Existing world / Playground routes and Upgrade Tree setup reapply the migration when the ladder is present, preventing passive controls from returning.

## Scripts added

- Runtime: `TrainingRaceRules.cs`, `TrainingLadderSettings.cs`, `PlayerTraining.cs`, `TrainingSelectionUI.cs`, `TrainingTierCardUI.cs`.
- Editor: `TrainingRaceSceneSetup.cs`.
- Tests: `TrainingRaceModelTests.cs`, `TrainingRacePlayModeTests.cs`.

## Existing scripts changed

- Runtime: `GameplayWorld.cs`, `RacePrototype.cs`, `RaceRewards.cs`, `RaceUI.cs`, `AreaNavigationUI.cs`, `PlayerProgression.cs`, `PlayerEquipment.cs`, `PlayerPlayground.cs`, `PlaygroundRules.cs`, `StableUI.cs`, `UIDetailFormatter.cs`, `CurrencyFeedback.cs`.
- Editor: `Milestone45SceneSetup.cs`, `UpgradeTreeSceneSetup.cs`.
- Tests: `Milestone4SceneTests.cs`, `Milestone4PlayModeTests.cs`, `ProgressionLoopTests.cs`, `MinimalUIPlayModeTests.cs`, `UpgradeTreePlayModeTests.cs`. Active-scene passive assertions were updated; standalone legacy-model coverage remains.

Runtime / Editor / Tests paths are under `Assets/Racing/`. The saved `Milestone3World.unity` and new ladder, three races and nine rivals are included with Unity-generated meta files. Existing world GUID remains `8789f1f4a37cec24183629f77a4555ec`.

## Verification

- Full Edit Mode suite: **62 passed, 0 failed, 0 skipped**.
- Full Play Mode suite: **13 passed, 0 failed, 0 skipped**.
- Final HUD-anchor check: **5 Training / UI Play Mode tests passed**, after the full suite. Final console query returned no errors or warnings.
- End-to-end scene test: Training I win → Training II unlock → 100 Coins + 6 Tokens → Speed purchase for 3 Tokens → actual Speed 10 → 10.75 → Training II uses the upgraded player and stronger fixed rivals.
- Scene tests also check loss rewards / no unlock, no passive income, no duplicate payment, retained scene / object identity, restored Main Race rivals and surviving Farm pickups.
- Model checks cover every nonwinning place, all three rewards / unlocks, best-place retention, replay payment, currency isolation, real simulation improvements, wrong / abandoned claims and fixed difficulty.
- Visuals in `Screenshots/`: selector at 1920×1080 and 1024×768, Training I result, Training II unlock and horse purchase. Existing layout tests cover Stable / Farm / tree / Main Race at 1920×1080, 1280×720, 1024×768 and 2560×1080.

## How to test

1. Open `Assets/Racing/Scenes/Milestone3World.unity`, press Play and wait in Stable. Coins should not rise automatically.
2. Click TRAINING: I is available; II / III show their preceding-tier win requirements.
3. Start I. After 3–2–1, tap SPACE regularly. Finish 1st: earn 100 Coins / 6 Tokens and unlock II.
4. RETURN TO STABLE and TRAIN Speed. Spend 3 Tokens, gain +0.75 Speed, retain the Coins.
5. Reopen TRAINING. I shows Best: 1st and II can start. Replay with weaker input: lower finishes still pay without removing its best place or unlock.
6. Win II to unlock III. E jumps hurdles. Main Race remains a manual entry and restores its original course / rivals.

## Known limits

- Progress remains session-only; stopping Play resets Tokens, stats, best places and unlocks. No save/load was added.
- Balance values are prototype tuning. Training III may require repeated races and upgrades.
- Existing Carrot feeding still grants permanent Speed; Farm behavior was preserved.
- Legacy production / crafting backend and Main Race Tokens remain for compatibility. Passive training and Blacksmith gameplay are inactive in the current demo.
- The remaining Stable / Farm UI is retained; this is not the full Master Brief redesign.
