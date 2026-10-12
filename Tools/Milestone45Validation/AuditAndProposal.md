# Milestone 4.5 — project audit and implementation proposal

Date: 6 October 2026. Status: audit complete; implementation awaits approval under the user's Change Control rule.

This document describes proposed work, not completed Milestone 4.5 features. Existing gameplay scripts, configuration assets, and the gameplay scene have not been edited during this audit.

## 1. Current project and verified baseline

- Unity 6000.6.2f1, Windows, 2D sprite presentation.
- Manifest: Cinemachine 6.6.0, Input System 1.20.0, uGUI 2.6.0, URP 17.6.0, Test Framework 1.8.0.
- Only enabled build scene: Assets/Racing/Scenes/Milestone3World.unity.
- Scene GUID: 8789f1f4a37cec24183629f77a4555ec.
- The open scene was clean before inspection. It contains one GameplayWorld coordinator and one RacePrototype.
- Live inspection confirmed one Unity Camera, one AudioListener, one EventSystem, five Cinemachine cameras.
- All nine scene canvases use Scale With Screen Size with reference resolution 1920 × 1080.
- Fresh player state: Coins/Carrots/Metal/Race Tokens start at zero; only Speed starts unlocked.
- New baseline EditMode run: 45/45 passed, no failures or skips.
- PlayMode discovery lists nine tests. Two new runner attempts returned zero executed tests, including an explicit assembly-filtered attempt. These do not verify PlayMode behavior. Prior MinimalUI validation documents nine passing tests, but that is historical evidence.
- A separate live Play smoke check successfully initialized StableManagement, four Main Race participants, and training income (35 Coins after the first cycle). The Farm remained locked as expected. Console returned no errors or warnings. Play was stopped afterward.
- Screenshots in this folder: BeforeStable.png (world only) and BeforeStableUI.png (live world plus UI).
- There is no Git repository in the workspace. Before implementation, preserve a filesystem backup of affected scripts, assets, scenes and their meta files outside Assets.

## 2. Audit scope and dependency map

Reviewed the request, runtime gameplay/economy/presentation code, editor setup and UI builders, configurations, test code, assembly definitions, package manifest, build settings, input settings, asset inventory, live scene hierarchy/cameras/canvas configuration, and prior milestone/upgrade-tree/minimal-UI reports. Art/Concepts contains direction boards rather than production sprites. Assets contain no existing gameplay prefabs, animator controllers or animation clips; horse animation uses imported sprite frames.

The dependency chain is:

Progression.asset → PlayerHorse.asset + Workshop.asset + UpgradeTree.asset
→ ProgressionSession → one pure-C# PlayerProgression
→ GameplayWorld / RaceRewards / UI presenters.

RacePrototype.Start initializes the existing participants, race, rewards, UI and GameplayWorld. It does not regenerate the scene. RaceManager advances the four racers in FixedUpdate. HorseSimulation owns boost, jump, obstacle penalties and movement; HorseView interpolates/render-animates those racers. These race systems can be retained.

GameplayWorld.Update currently calls:
1. AdvanceTraining only while IsManagement.
2. AdvanceFarm in all world phases (the model rejects production while Farm is locked).
3. AdvanceCraft in all world phases.

The same PlayerProgression owns Coins, Metal, Carrots, StoredCarrots, direct horse levels, production levels, tree unlocks, feeding bonuses, equipment tiers and a one-use Main Race reward claim. Currency events already drive useful feedback.

ProgressionSession is runtime-only and resets when Play begins. No persistent save/load was found. Preserve existing fields and enum IDs; no save format conversion is needed in this milestone.

## 3. Existing Training

- One elapsed timer in PlayerProgression; no physical training horses.
- Base cycle 15 seconds, reward 35 Coins plus 2 Metal.
- Training completion does not consume Main Race reward claims.
- Training Speed: duration = max(1, 15 × max(0.5, 1 − 0.10 × level)); levels 0–5.
- Training Reward: ceil(35 × (1 + min(0.75, 0.15 × level))); levels 0–5.
- Existing reward sequence: 35, 41, 46, 51, 56, 62 Coins.
- Speed purchase base cost 150; Reward base cost 180; both grow by 1.6 per level.
- One-time tree unlocks separately gate the existing speed/reward upgrade buttons.
- ManagementUI displays a prominent progress bar and a moving UI horse icon. This presenter must be replaced/disabled when the physical track becomes primary.

## 4. Existing Farm and feeding

- Four decorative dirt plots/leaves at x −45 through −41.4; no colliders, crop state or world input.
- One farm timer adds one StoredCarrot each 20 seconds, capped at 10.
- Farm unlock costs 5 Race Tokens. A fresh last-place result pays one token, requiring five races before Farm becomes usable.
- CollectCarrots transfers storage directly to Carrots.
- Feeding costs 5 collected carrots and permanently adds 0.1 Speed to the shared horse stat model.
- Grow Speed reduces the cycle by 10% of base time per level, up to five levels / 10-second minimum.
- Farm Capacity adds five storage per level, up to 35. Storage capacity no longer suits world harvesting.
- FarmUI drives the progress bar, Collect button and Feed button.
- Feeding should remain a carrot sink. Replace the storage/collect interaction with physical crops and pickups; retain compact feeding access.

## 5. Existing Blacksmith and dependent systems

- Blacksmith at x −9, separate from the Farm, currently on its right.
- Crafting requires the Blacksmith unlock (10 Race Tokens after Farm).
- Horseshoes add Speed, Saddles add Acceleration, Bridles add BoostPower. Equipment bonus is included in PlayerProgression.Breakdown and HorsePower.
- Five tiers per equipment slot; crafting pays Coins/Metal atomically, runs a timer, then automatically equips.
- Forge Speed and Metal Efficiency upgrades have their own tree branch and UI.
- BlacksmithUI, CraftFeedback, ForgeVisual, UIDetailFormatter, resource icons/tooltips, navigation and tree nodes refer to crafting.
- ForgeVisual currently becomes most animated only during crafting. It needs an ambient mode for the visual-only smith.
- Main Race pays Metal as well as Coins and Tokens. Metal has no active purpose outside crafting.
- Existing rivals run at 23 / 24 / 25 Speed; recommended power is 245. The long progression tests include crafting, so disabling gear requires a new playable-balance check.
- Preserve PlayerEquipment, Workshop tier definitions, Metal, equipment state and stat calculations. Disable their active player-facing entry points rather than delete them.
- Hide Blacksmith navigation/crafting, crafting toast, forge production upgrades, Blacksmith tree branch and unused Metal display/tooltips. Remove obsolete crafting promises from race/resource/tree text.
- Keep any previously equipped bonuses valid. In the active Milestone 4.5 world, stop advancing craft and prevent starting new crafts. Ambient fire/sparks must not depend on a craft timer.

## 6. Existing camera and UI flow

- Stable camera x −26; Farm x −43; Blacksmith x −9; Race Entry x 7; race follow camera targets the player render root.
- Default Cinemachine blend: EaseInOut, 0.6 seconds.
- Farm and Blacksmith presently behave as separate camera/panel stations.
- Main Race starts only after ShowRaceArea, refreshes the player's real stats, prepares participants, changes camera priority, then starts its 3-second countdown.
- Returning resets race participants after the return blend and resumes management. GameScenes entry points navigate the loaded world and do not load another scene.
- Existing UI has compact icon buttons and a shared hover/focus detail panel; reusing its purchase guards and feedback is sensible.
- World input must ignore blocking UI and race/tree phases. Hover pickup detection must support overlapping piles and cursor sweeps, not depend only on a click or a single topmost collider.

## 7. Proposed gameplay decisions requiring approval

1. Keep the existing gameplay scene and race objects; arrange a connected Blacksmith → Farm → Stable → nearby Training Track.
2. Start with one Training Horse; configurable maximum four, with architecture able to support six without dozens of objects.
3. Replace the single training timer with independent normalized lap phases in the production model. Central advancement pays a lap reward at each wrap; physical positions sample those exact phases. There is no second visual-only production clock.
4. Keep existing Training Speed/Reward formulas and costs for initial migration. Add Horse Count as a direct compact Training upgrade with access through the existing Speed unlock, without adding a new progression screen.
5. Six carrot plots, each with independent pure-C# growth state and staggered initial progress.
6. Make Farm available immediately in the Milestone 4.5 configuration. This deliberately changes the current five-token Farm gate so the new active loop can be played from the start. Preserve Race Tokens and the existing tree for other unlocks.
7. Keep three Farm upgrades: existing Auto Growth Speed, new Click Growth Power, new Harvest Yield. Hide Farm Capacity; retain its ID/state for compatibility.
8. New Click Power and Yield upgrades require the Farm unlock directly. Existing training-speed/reward and other useful tree unlocks can remain.
9. Training pauses from race preparation through results/return and resumes when management is restored. Crops continue growing through the Main Race; settled drops persist in the same scene. Farm input/pickup sweeping is disabled while racing.
10. Blacksmith is ambient visual-only. Preserve backend equipment/Metal data and hide inactive controls/branches.
11. Keep stable/farm framing wide enough to see training and crops together. Use a separate fast Cinemachine transition for Main Race (target 0.15 seconds) without changing scene.
12. Verify Main Race reachability through direct stats and feeding without equipment. Retain fixed rivals first; adjust only related prototype balance if measured progression becomes excessive.

## 8. Proposed configurable prototype balance

All values below are proposals; they have not been applied.

| Mechanic | Initial value / formula |
| --- | --- |
| Training horses | 1 initially; maximum 4 |
| Horse count upgrade | +1 horse; cost ceil(500 × 1.8^level): 500, 900, 1620 Coins |
| Training base lap | 15 seconds |
| Training speed | Existing 10% base-duration reduction per level, five levels; 15 → 13.5 → 12 → 10.5 → 9 → 7.5 seconds |
| Training reward | Existing 35 → 41 → 46 → 51 → 56 → 62 Coins per horse lap |
| Training Metal | 0 in active Milestone 4.5 configuration; retain Metal model fields |
| Farm plots | 6, initially staggered over the first 45% of growth |
| Automatic growth | Normalized growth += dt / effectiveGrowthSeconds; base 20 seconds |
| Crop stages | Sprout [0, 0.30), Mid [0.30, 0.65), Large [0.65, 1), Ready at 1 |
| Click growth | +0.10 normalized growth per click; configurable 0.25-second interaction cooldown |
| Active growth target | Around 7 seconds at one comfortable click/second from zero (about seven clicks plus passive growth) |
| Click Power | +0.02 normalized growth per level, five levels; 120 × 1.6^level Coin costs, rounded up |
| Auto Growth | Existing GrowSpeed: 20 × max(0.5, 1 − 0.10 × level), five levels; existing 120 × 1.6^level costs |
| Harvest Yield | 2 initially, +1 per level up to 7; five levels, ceil(160 × 1.6^level) Coin costs |
| Feeding | Retain 5 Carrots → permanent +0.1 main-horse Speed |
| Uncollected pickups | Configurable 80-object pool/cap; merge overflow value into existing carrot stacks |
| Drop motion | Controlled ~0.45-second arc/scatter and a small bounce; bounded ground positions |
| Pickup collection | Sweep hover after settling; ~0.2-second pop/fly/shrink, then credit exactly once |
| Race transition | ~0.15-second Cinemachine transition; normal 3–2–1–GO |

Reward production = activeHorseCount × rewardPerLap / lapDuration. Purchasing speed preserves normalized progress instead of restarting laps or paying immediately. New horses enter with separated path positions and do not grant purchase-time lap rewards.

Growth clamps at Ready until harvested; passive progression never bypasses hover collection. Clicking a growing crop changes progress and gives a punch/stage pop. Clicking Ready resets it immediately to Sprout and emits drops. Harvest does not credit Carrots.

Pickups have no expiry initially, allowing persistent piles during the current Play session. Overflow merges resource values rather than silently losing items. If carrot balance reaches the integer limit, uncredited value stays in the pending stack. No inventory menu or planting step is introduced.

## 9. Implementation structure and expected change surface

Reuse the existing partial PlayerProgression model, atomic purchases, resource events, direct stat upgrades, feeding, race claim protection, imported Horse0–3 sprite frames, square sprite, carrot icon/art, Cinemachine references and canvas helpers.

Expected new small files (final names may be refined):
- StablePlaygroundSettings / pure configuration interface and rules.
- TrainingProduction and CropGrowth state/model classes, independent of UnityEngine.
- A PlayerProgression partial for playground state/resource integration where appropriate.
- TrainingTrackView: cached path samples, separated horse visuals, frame animation and lap feedback.
- FarmWorldView: six crop visuals, four stages, central feedback updates.
- StableWorldInteraction: cached camera and Input System pointer input; UI blocking; plot clicks and pickup sweeps.
- WorldPickupManager and lightweight pickup view/state: pool, drop/collection animations and value merging.
- WorldFeedback: bounded reused floating text/effect slots and optional audio event hooks.
- Milestone45SceneSetup and focused UI construction helpers.
- Milestone45 model, saved-scene and PlayMode acceptance tests.

Expected existing modifications:
- PlayerProgression.cs / PlayerProduction.cs: production integration, extra upgrade states, direct pickup credit and removal of duplicate active timer sources.
- GameplayWorld.cs: central tick ownership, new initialization, race pause/resume rule and disabled crafting access.
- WorldSettings.cs or a referenced playground settings asset: serialized values and transition settings.
- WorkshopRules.cs / WorkshopSettings.cs: append new upgrade IDs/rules without renumbering current IDs.
- PlayerUpgradeTree.cs / UpgradeTreeRules.cs as needed: map new upgrades to existing prerequisites; block inactive crafting purchases in the active milestone.
- ManagementUI.cs / FarmUI.cs / ProductionUpgradeUI.cs: compact upgrades and feeding, remove timer/collect presentation.
- AreaNavigationUI.cs / AreaPanel.cs / StableUI.cs: connected management access and compact panels.
- WorldCameraNavigation.cs: connected stable framing and fast race transitions.
- ForgeVisual.cs: ambient animation independent of crafting.
- CurrencyFeedback.cs / RaceUI.cs / UIDetailFormatter.cs / UpgradeIconState.cs: active resources and accurate upgrade/world descriptions.
- Targeted adjustments to UI builders/setup only where necessary to stop obsolete controls returning; retain legacy tools without rerunning broad scene reconstruction.
- The existing Milestone3World.unity and relevant configuration assets, preserving their GUIDs.
- Existing tests whose assertions explicitly demand storage, crafting UI, old gate state or fixed object counts: replace those expectations with Milestone 4.5 contracts; retain pure backend tests where applicable.
- Update stale README guidance so it opens the actual unified gameplay world.

Avoid reusing HorseView directly for Training: its logic is coupled to HorseMovement and a Main Race simulation. Reuse the sprite assets/frame approach in a small training presenter without copying race input/participant/reward components.

Keep per-frame work in a few managers. Preallocate crop, horse, pickup and effect arrays; cache references; no per-frame LINQ, new arrays, string concatenation or dynamic component searches. Format event text on discrete state changes. World pickup value safety and lifecycle remain separate from animation state.

## 10. Setup and compatibility

- Create an idempotent Horsecremental > Setup Milestone 4.5 World command that updates the existing saved world, assigns references and builds only milestone-owned content.
- Preserve Main Race hierarchy and the scene GUID; do not call the older full Milestone 2/3 reconstruction commands.
- Back up current relevant source/config/scene/meta files before edits because Git recovery is unavailable.
- Preserve old enum numeric IDs; append new production IDs. Keep legacy farm-capacity/equipment data even when hidden.
- Existing StoredCarrots, if present in a live model being converted, become equivalent pending carrot drops once; do not auto-credit or discard them. Existing farm elapsed progress can seed the first crop before offsets are applied.
- There is no persistent save/load migration to perform. New pending drops and crops only need to survive same-scene Main Race travel during this milestone.
- Keep existing equipment bonuses in stat breakdowns; do not reset prior state as a side effect of disabling interaction.
- Repeated setup must keep exactly one world coordinator, one camera/listener/EventSystem, one physical training source, one farm source, and one details panel.

## 11. Validation and delivery plan

Follow the request's order: disable smith → physical single-horse lap → multiple horses → Training upgrades → farm stages → click growth → harvest → pickups → Farm upgrades → camera flow → polish → obsolete UI cleanup → full loop test.

Meaningful checks:
- Each exact model lap wrap pays one reward; no duplicate timer payout; speed upgrade preserves progress and advances visuals faster.
- Purchase failure is atomic; count purchases visibly enable another distinct horse; maximum count enforced.
- Farm passive growth and clicks reach all stages; harvest creates the correct value without currency gain; reset starts at Sprout.
- Hover sweep collects overlapping piles; UI/race/tree block interaction; collection cannot double-credit; cap/merge preserves total value.
- Main Race uses the same scene handle; training stays paused, crop growth continues, drops persist, return resumes training.
- No active smith crafting/navigation/tree purchases or misleading Metal controls.
- One physical camera/listener and configured 1920×1080 scaling after save/reopen and repeated setup.
- Input tests exercise actual mouse hover/click, not just direct model methods.
- Visual checks at 1920×1080, 1280×720, 1024×768 and 2560×1080, including pending piles, multiple horses and race return.
- End-to-end race progression without gear, accounting for the faster multi-horse and six-plot economy.
- Repair or work around the observed zero-test PlayMode runner behavior before claiming live test success. Do not modify third-party packages as part of gameplay work without a justified need.
- Finish with the user's requested 25-topic implementation report, exact formulas, file lists, test results, known issues, removable legacy features and a recommendation. Stop after Milestone 4.5.

## Approval boundary

The user's supplied AGENTS.md rule 5 says:
> Before modifying existing working code or core asset setups, explain the intended changes and obtain the user's approval.

Approval is requested for the concrete implementation above, including immediate Farm access, replacement of active Training/Farm timer presentation, hidden crafting/tree/resource UI, and the connected scene layout. The audit/report and baseline screenshots are new review artifacts; Milestone 4.5 gameplay changes have not begun.

