# Milestone 4 audit and implementation proposal

Date: 2026-10-06. Status: audit complete; implementation awaits approval under the supplied AGENTS.md change-control rule. No gameplay scripts, configuration assets, or saved scenes have been modified during this audit.

## Existing project and systems

- Unity 6000.6.2f1; installed Cinemachine 6.6.0, Input System, uGUI, URP 2D, and Unity Test Framework.
- The active scene and only enabled build scene is `Assets/Racing/Scenes/Milestone3World.unity`. It already contains Stable, carrot plots, race entry, and the 420-unit Main Race track. Keep this scene and its GUID; do not create a Milestone 4 gameplay scene.
- The live scene has a `RacePrototype`, `GameplayWorld`, and `WorldCameraNavigation` on the world root. Three Cinemachine cameras select Stable, race entry, and race follow through one Brain.
- `PlayerProgression` is a pure C# model and the single owner of Coins, collected Carrots, stored Carrots, feeding bonuses, horse upgrade levels, and Training/Farm elapsed time.
- `ProgressionSession` supplies the same model to Unity adapters and resets it on entering Play. There is no persistence or offline production.
- `ProgressionSettings` supplies base horse stats, direct-upgrade rules, and positional Main Race Coin rewards. `WorldSettings` implements the pure `IIdleSettings` interface for production/feeding configuration.
- `PlayerProgression.Stats` already combines base stats, direct upgrades, and feeding. `RacePrototype.RefreshPlayer()` passes that result to the actual `HorseMovement` / `HorseSimulation` before each Main Race.
- `RaceRewards` uses a race token to pay once and prevent stale/duplicate payouts. Extend that same transaction to include Metal.
- `GameplayWorld.Update()` ticks Training only while management is active and ticks Farm independently. Training already pauses during preparation, countdown, racing, results, and the return blend, then resumes with its remainder.
- `ManagementUI` currently combines Farm, Training, and power guidance. `StableUI` presents direct upgrades. `CurrencyFeedback` animates the persistent Coin header. Farm does not yet have its own camera/view.
- The current Horse Power formula exists in UI: `Speed * 10 + Acceleration * 0.25 + BoostPower`. Move this calculation into the model so all presentations use the same final-stat result.
- SPACE Boost and E Jump are handled by the existing Input System controller. The cubic high-meter drain, jump physics, obstacle penalties, race simulation, and horse animation should be reused.
- Existing UI builders configure Scale With Screen Size, reference resolution 1920x1080. Preserve this configuration in new/updated panels.
- Legacy Milestone 1/2 scenes and setup commands remain as source/reference material. The Milestone 3 setup recreates the world from those sources; it is unsuitable for applying Milestone 4 because it would replace the current world. Add an in-place Milestone 4 setup command instead.
- Candidate art: the existing `Furnace and Sawmill.png` sheet, garden/decor assets, generated solid sprite, and horse frames. Inspect candidate sprites before selecting them. Anvil/workbench placeholders are sufficient; do not add a large art pipeline.
- The workspace is not a Git repository. Preserve existing files and GUIDs, and make a local backup of changed scripts/assets and the world scene before implementation.

## Current prototype values

| System | Current values |
|---|---|
| Starting resources | 0 Coins, 0 collected/stored Carrots |
| Base horse | Speed 10, Acceleration 12, Boost Power 5 |
| Direct Speed upgrade | +0.75; initial cost 60 Coins |
| Direct Acceleration upgrade | +3; initial cost 50 Coins |
| Direct Boost upgrade | +1; initial cost 60 Coins |
| Direct upgrade price growth | x1.45 per purchase |
| Training | 35 Coins / 15 seconds; 140 Coins per management minute |
| Farm | 1 Carrot / 20 seconds; storage capacity 10 |
| Feeding | 5 collected Carrots -> +0.1 Speed |
| Main Race Coin rewards | Places 1-4: 500 / 250 / 150 / 100 |
| Fixed rival Speed | 14 / 14.5 / 15; Acceleration 12 |
| Main Race | 420 units; hurdles at 65 / 145 / 230 / 315 / 385 |
| Camera blends | EaseInOut, 0.9 seconds |
| Boost | +0.06 energy/tap; drain = 0.02 + 0.58 * energy^3 |

## Baseline verification performed in this audit

The following existing EditMode tests were run through the connected Unity Editor; all 4 passed, none skipped:

1. `TrainingCarriesRemaindersAndDoesNotConsumeMainRaceReward`
2. `FarmCapsCollectsAndFeedChangesTheSameStatsModel`
3. `HighBoostDrainsFasterAndRhythmicTapsSettleBelowMaximum`
4. `FixedOpponentsBeatFreshRhythmicPlayerAndLoseAfterProgression`

The balance test reported: fresh player 33.88s, slowest rival 30.58s, fastest rival 28.62s, progressed player after ten simulated Training minutes 23.32s (Speed 14.35, Boost Power 8). This is a deterministic model test with rhythmic Boost and correctly timed jumps, not a new manual PlayMode verification.

The live Console query returned zero errors/warnings. Existing Milestone 3 reports contain broader prior validation; those historical results have not been rerun as part of this audit.

## Proposed implementation

### 1. Extend progression and rewards

Keep `PlayerProgression` as the only state owner. Add Metal, three equipment tier levels, six production-upgrade levels, and one active craft record/timer. Use small pure C# configuration/value types to keep economy and crafting calculations independent of Unity. Add resource/craft/upgrade events for presentation.

Training starts at 35 Coins + 2 Metal per 15 seconds. Main Race keeps its existing Coin rewards initially and adds configurable positional Metal rewards of 15 / 10 / 6 / 3. Validate actual saturated deltas and atomic payment behavior; a duplicate/stale race token must pay neither resource.

Keep stored Farm output and collected Carrots distinct, as they already are. The header will show Coins, Metal, collected Carrots, and Farm storage/capacity explicitly rather than presenting collected inventory as capped storage.

### 2. Deterministic equipment and final stats

Use one configurable equipment settings asset containing three categories with five tiers each (0-4). Definitions include display name, Coin/Metal costs, craft duration, and the relevant stat bonus. Store only current tiers in progression; no inventory, equipment copies, random outcomes, or manual equip controls.

Final stat = base + direct-upgrade bonus + feeding bonus + current equipment bonus. Tier bonuses replace the preceding tier bonus; they do not accumulate all previously crafted tiers. Feeding remains a Speed source. Keep Boost tap/drain and jump values supplied by the base horse.

The proposed initial tier table below is editable prototype data, to be checked against the 15-30 minute target during implementation:

| Tier | Horseshoes / Speed bonus | Saddle / Acceleration bonus | Bridle / Boost Power bonus | Coins | Metal | Base craft seconds |
|---|---|---|---|---:|---:|---:|
| 0 | Old Horseshoes / 0 | Old Saddle / 0 | Basic Bridle / 0 | 0 | 0 | 0 |
| 1 | Iron Horseshoes / +1.5 | Leather Saddle / +3 | Reinforced Bridle / +1.5 | 100 | 5 | 5 |
| 2 | Reinforced Horseshoes / +3 | Racing Saddle / +6 | Racing Bridle / +3 | 300 | 12 | 8 |
| 3 | Steel Racing Horseshoes / +5 | Professional Saddle / +10 | Professional Bridle / +5 | 700 | 25 | 10 |
| 4 | Champion Horseshoes / +7 | Champion Saddle / +14 | Champion Bridle / +7 | 1500 | 50 | 12 |

Stable UI will show final stats, Horse Power, and base/upgrades/food/equipment contributions. Equipment contributes to the same Horse Power used by Training presentation and future requirements. Keep the current guaranteed Training timer primarily governed by Training Speed; do not add Training tiers.

### 3. Crafting and production upgrades

Crafting requires sufficient Coins and Metal, atomically spends both when started, and allows one job. On completion, progression replaces/equips that category's next tier and emits completion feedback. Capture costs and duration at craft start so buying a production upgrade mid-craft does not alter already-paid costs or reset progress.

Craft completion during Main Race updates the progression model immediately. The active racer retains its attempt's parameter snapshot; new equipment is applied at the next attempt through the existing RefreshPlayer path. This avoids resetting the running horse or changing jump/obstacle simulation midway through a race.

Add six configurable, finite production upgrades, each with five purchasable increments (level 0-5). Initial proposal:

| Upgrade | Effect per purchase | Proposed initial Coin cost / cost growth | Cap |
|---|---|---|---|
| Forge Speed | -10 percentage points of base craft duration | 150 / x1.6 | 50% base duration |
| Metal Efficiency | -5 percentage points of base Metal cost | 200 / x1.6 | 75% base cost; ceil cost, minimum 1 Metal |
| Farm Grow Speed | -10 percentage points of base growth interval | 120 / x1.6 | 50% base interval (10s initially) |
| Farm Capacity | +5 stored Carrots | 100 / x1.6 | 35 stored Carrots initially |
| Training Speed | -10 percentage points of base completion interval | 150 / x1.6 | 50% base interval (7.5s initially) |
| Training Reward | +15% of base Coin payout | 180 / x1.6 | x1.75 Coin payout; keep Metal at 2 initially |

All upgrade effects/costs/caps live in configuration, not UI. Define and test integer rounding for Coin and Metal values. Keep positive minimum durations/costs and validate configuration to prevent runaway loops or free equipment. Preserve fractional production progress when upgrading interval speed.

### 4. Scene, navigation, UI, and feedback

Extend `GameplayWorld` with an explicit current management area (Horse/Stable, Farm, Blacksmith, Race entry). Add Farm and Blacksmith Cinemachine sub-cameras through `WorldCameraNavigation`, retaining one physical camera/Brain. Use existing fast blends, with a proposed 0.6s management duration after visual review.

Add a modest forge/workbench/anvil/crate workstation in the existing Stable environment. Move/organize carrot plots as needed to make the Farm view readable. The race track remains in its existing coordinate system.

Present only the current area's controls. Horse view has direct upgrades, stat breakdown, Training status/upgrades, and power guidance. Farm view has production/storage, collect/feed, Grow Speed, and Capacity. Blacksmith view has three deterministic next-tier cards, active job progress/countdown, Forge Speed, and Metal Efficiency. Race entry retains the existing start/difficulty controls. A clear area navigation bar and resource header remain available during management.

Reuse the existing button pulse, currency interpolation/popup, and race feedback. Add Metal/Carrot gain/spend feedback, explicit missing-resource amounts, a craft-complete banner and scale punch, and simple forge glow/sparks or hammer motion. Animate without per-frame string building, LINQ, or new arrays; refresh timer text at a bounded cadence.

### 5. Production phase rules

Tick Farm and Blacksmith from the always-active gameplay coordinator, independent of the visibility of their panels. Tick Training only in management, including browsing Farm/Blacksmith/race entry. Pause Training throughout Main Race preparation/countdown/running/results and return transition. Preserve its remainder and automatically resume after returning. No offline accumulation.

### 6. Balance and validation

Retune only the existing fixed rivals and recommendation guidance as needed for the new equipment bonuses and 15-30 minute prototype target. The current ten-minute win would otherwise become much quicker with equipment. Do not introduce dynamic opponent scaling, Championships, or new Training tiers. Choose final rival values after testing multiple progression spending paths; max equipment must not be mandatory.

Add focused model tests for Metal rewards and atomic spending, tier replacement and actual stat contributions, single-slot craft progress/completion, reduced costs/durations, caps, farm storage, and timer remainders. Update existing tests only where expected camera counts/UI/balance values intentionally change. Run EditMode and PlayMode checks, including navigation while crafting, craft/Farm continuation during Main Race, Training pause/resume, repeated races and one-time rewards, insufficient-resource feedback, and fresh/progressed race outcomes.

Inspect screenshots at 1920x1080, 1280x720, 1024x768, and ultrawide. Check no simultaneous management panels block navigation and no hidden panel receives input. Finish with Console verification and the requested twenty-part implementation report, exact final economy tables, full-loop test instructions, known limitations, obsolete-system notes, and a Milestone 5 recommendation only.

## Expected change scope

Existing runtime files likely modified: `PlayerProgression.cs`, `ProgressionSettings.cs`, `IIdleSettings.cs`, `WorldSettings.cs`, `RaceRewards.cs`, `GameplayWorld.cs`, `WorldCameraNavigation.cs`, `StableUI.cs`, `ManagementUI.cs`, `CurrencyFeedback.cs`, and `RaceUI.cs`. Keep `RacePrototype`'s existing final-stat application path unless a small integration change is needed.

New runtime files: small equipment/production definitions and settings adapters, stat breakdown values, Blacksmith presentation, area-specific production presentation, and lightweight forge/craft feedback components as needed. Keep responsibilities small; do not turn PlayerProgression or a UI component into a monolithic implementation.

New Editor files: an idempotent `Milestone4SceneSetup` and a small UI builder that upgrade the existing scene in place and assign references. Do not invoke the old world recreation setup. Preserve original meta/GUIDs; let Unity create metas for new assets/scripts.

Configuration changes: existing World/Progression and fixed rival assets; new equipment/production configuration asset(s). Scene change: existing Milestone3World only. No new packages or assembly-definition changes are expected.

## Approval requested

Approve extending the existing progression/reward/UI/camera scripts, modifying the existing world scene in place, adding the equipment/production configuration and Blacksmith workstation, and tuning the existing fixed Main Race balance within Milestone 4. Prototype values may be adjusted during validation to meet the requested pacing. Approval covers completing and testing Milestone 4 only.
