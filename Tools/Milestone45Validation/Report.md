# Milestone 4.5 — Stable playground implementation report

Implemented in the existing `Assets/Racing/Scenes/Milestone3World.unity` gameplay scene. Approval: the user accepted the audited proposal with “yes continue.” Work stops at this milestone. The pre-change snapshot is `BackupBeforeImplementation/Racing`; no Git repository was available.

## 1. What was implemented

A connected Blacksmith → Farm → Stable → Training Track world. The Stable now contains running training horses, six independent clickable crops, harvest drops, hover collection, shared resource purchases and feeding. The existing Main Race remains in this scene. Farm is immediately available. Crafting is inactive and the smith stays visibly animated. Large training/farm bars and Collect Carrots are removed from the active loop.

The original scene GUID remains `8789f1f4a37cec24183629f77a4555ec`. The scene has one physical camera, one AudioListener, one EventSystem and five Cinemachine cameras. All ten canvases, including retained inactive canvases, use Scale With Screen Size and a 1920×1080 reference. Reapplying the focused setup twice produced 596 transforms both times and zero missing scripts. No existing meta files changed.

Final verification: **54/54 EditMode and 10/10 PlayMode tests passed, zero failures/skips**. Console checks after normal live gameplay and the final suite returned **zero errors/warnings**. The direct mouse acceptance test verifies real Input System events, crop growth/harvest and hover-only collection. Screenshot/layout coverage includes Stable, Farm, tree and race at 1920×1080, 1280×720, 1024×768 and 2560×1080. `EditModeResults.json`, `PlayModeResults.json` and `Screenshots` hold the evidence. `StableFourHorses.png` shows the purchased-horse playground with ripe crops and physical drops.

## 2. Scripts created

Paths below are relative to `Assets/Racing`:

- `Runtime/PlaygroundRules.cs`: Unity-independent settings interface, TrainingProduction and CropGrowth models.
- `Runtime/PlayerPlayground.cs`: shared progression integration, lap rewards, harvest events, pickup credit and inactive-system guards.
- `Runtime/StablePlaygroundSettings.cs`: serialized playground configuration.
- `Runtime/TrainingTrackView.cs`: physical positions, frames, orientation, shadows and new-horse entrance.
- `Runtime/FarmWorldView.cs`: crop stages, click cooldown, harvest presentation and highlights.
- `Runtime/WorldPickupManager.cs`: bounded drop pool, scattering, stacks, collection and resource credit.
- `Runtime/StableWorldInteraction.cs`: pointer conversion, click action, UI blocking and hover/sweep collection.
- `Runtime/WorldFeedback.cs`: reusable floating-text pool.
- `Runtime/StableAudioHooks.cs`: optional sound slots and playback hooks.
- `Runtime/PlaygroundUpgradeUI.cs`, `Runtime/PlaygroundFarmUI.cs`: compact purchases and feeding.
- `Editor/Milestone45SceneSetup.cs`, `Editor/Milestone45UIBuilder.cs`: focused, repeatable scene setup.
- `Tests/Editor/Milestone45ModelTests.cs`, `Tests/PlayMode/StableMouseInputTests.cs`: model, economy, migration, balance and direct mouse acceptance checks.

New asset: `Config/StablePlayground.asset` and imported Unity meta files. No packages were added.

## 3. Scripts modified

Runtime: GameplayWorld, PlayerProgression, PlayerProduction, PlayerEquipment, PlayerUpgradeTree, WorkshopRules, WorkshopSettings, UpgradeTreeRules, ForgeVisual, AreaPanel, AreaNavigationUI, StableUI, RaceUI and UIDetailFormatter.

Editor: Milestone3SceneSetup, Milestone4SceneSetup and MinimalUISetup now route existing-world setup to the playground after configuration, avoiding restoration of obsolete bars/crafting controls.

Tests: Milestone3ModelTests, Milestone4ModelTests, Milestone4SceneTests, UpgradeTreeTests, UpgradeTreeTestSupport, Milestone4PlayModeTests, MinimalUIPlayModeTests, ProgressionLoopTests, RacePlayModeTests and UpgradeTreePlayModeTests. Pure legacy crafting/storage contracts still run against explicit legacy fixtures. Active scene tests now require playground behavior.

Configuration changes: World training Metal becomes zero; Farm starts unlocked with zero unlock cost; Workshop appends three rules while preserving the six existing serialized entries. Scene references, framing and layout were updated. README now identifies the actual gameplay scene. `ChangedFiles.json` lists every created/modified file against the backup.

## 4. Old scripts/UI disabled

Management UI/ManagementUI, Farm UI/FarmUI and Blacksmith UI/BlacksmithUI are inactive scene objects. Their six legacy ProductionUpgradeUI controls are inactive. CraftFeedback is disabled; the craft-complete toast, Metal counter/icon/popups, Blacksmith navigation and Blacksmith/Forge Speed/Metal Efficiency/Farm Capacity tree controls and connections are hidden. The four old decorative farm plots/leaves and old Stable Track Connection are inactive. They remain available for later cleanup.

## 5. How the physical Training Track works

The dirt loop has 40 cached path points and a marked finish at phase zero. Each horse position is sampled by accumulated path distance from its production model's normalized lap phase. GameplayWorld advances production once per frame; crossing phase one subtracts one and emits exactly one lap reward. There is no independent visual progress clock. Large valid time steps carry every completed lap and the remainder.

Horse frame animation speeds up with lap speed. Direction flips with the tangent and sorting follows vertical position. Stable/Farm/tree management and race entry keep training active. Preparation, countdown, racing, results and the return transition pause it; management restoration resumes the same phases.

## 6. Multiple Training Horses

Start with one; the configured maximum is four. The model/setup supports up to six. Every horse owns a phase and earns its own lap reward. New horses spawn in the midpoint of the largest cyclic gap, preserving all existing phases and separating the horses around the track. Existing horses do not teleport when an upgrade is bought. Color variants, run frames and a brief scale entrance distinguish purchases. Horse count and speed upgrades do not rebuild the track or spawn objects every lap.

## 7. Horse Count formula and costs

At count-upgrade level L, next cost is `ceil(500 × 1.8^L)` Coins: **500, 900, 1620**. Each purchase adds one physical horse. Level three reaches four horses and subsequent purchases are blocked without spending. Count access uses the existing initially unlocked Speed node. Count/state enum IDs were appended rather than renumbered.

## 8. Training Speed formula

`lapSeconds = max(1, baseSeconds × max(0.5, 1 − 0.10 × L))`.

Base 15 seconds, levels 0–5: **15, 13.5, 12, 10.5, 9, 7.5 seconds**. Upgrade base cost 150, multiplier 1.6. Normalized horse phases remain unchanged; the next frame advances at the new rate. Unlock Acceleration, then Training Speed in the existing tree.

## 9. Training Reward formula

`coinsPerHorseLap = ceil(35 × (1 + min(0.75, 0.15 × L)))`.

Levels 0–5: **35, 41, 46, 51, 56, 62 Coins**. Upgrade base cost 180, multiplier 1.6. Training Speed is its tree prerequisite. Training grants zero Metal and no Race Tokens, and cannot consume a Main Race claim. Four horses at maximum speed/reward produce **1984 Coins/minute**.

## 10. Crop growth calculation

Each of six plots stores its own normalized progress. Automatic growth adds `dt / effectiveGrowthSeconds`, clamped to one. Base time is 20 seconds. Initial progress is evenly staggered from zero to 0.45; the last plot initially becomes ready after about 11 seconds without clicks. Ready crops wait indefinitely. There is no automatic harvest, storage credit or inventory credit.

## 11. Exact stages

| Stage | Normalized growth | Visual scale |
| --- | --- | --- |
| Sprout | [0, 0.30) | 0.32 |
| Mid | [0.30, 0.65) | 0.58 |
| Large | [0.65, 1) | 0.85 |
| Ready | 1 | 1.08, green halo and gentle bounce |

Stage thresholds are serialized floats. Tests use the configured boundary values directly. Crop sprites grow from tiny plants to prominent carrots; growth/clicks add a brief squash/stretch.

## 12. Clicking accelerates growth

Input System click actions feed the world interaction controller. A click on a non-ready plot adds **0.10** normalized progress initially. Each plot has a **0.25-second** cooldown. Growth clamps at one. Holding the button does not repeatedly grow a crop. Clicking over blocking UI or outside Stable/Farm management does not reach the plot. Clicks show +Growth, a scale punch, hover highlighting and an optional sound.

## 13. Harvesting

A click on a ready crop resets that plot to Sprout and emits a harvest event. Its automatic growth immediately starts again. The view spawns physical drops using the current Harvest Yield. Harvesting itself adds **zero** Carrots to the wallet. A click that reaches Ready grows the crop; a subsequent click harvests it.

## 14. World drops

Base harvest creates two carrot sprites above the plot. They scatter within a configured 0.65-world-unit radius, follow a short pop arc and landing bounce for 0.45 seconds, then settle and bob slightly. They remain in the scene, including across Main Races. Multiple harvests accumulate visible piles. Resource values are independent of the number of pooled visual items.

## 15. Hover collection

Every eligible pointer update checks settled pickups against the segment from the previous world pointer position to the current one; this also covers a stationary hover. Radius is 0.36 world units. Every intersected pickup begins collection, so overlapping piles and quick cursor sweeps work. No extra click is needed. Items move upward toward the pointer and shrink over 0.20 seconds; only after that animation do they credit PlayerProgression and emit floating +Carrots, currency feedback and an optional sound. Sweeps reset when UI/state blocks input, preventing collection through menus or the Main Race.

## 16. Pickup limits and performance

80 reusable pickup transforms and 16 floating-text slots are created in the saved scene. Their state arrays, component references, path lengths, UI raycast list and pointer event data are cached. No per-drop Instantiate/Destroy, per-horse Update component, per-frame LINQ or per-frame array creation was added. The centralized loops are bounded by the configured pool/horse/plot sizes.

When the pool is full, all remaining harvest value merges into a nearby same-resource stack. Larger stacks scale slightly. No drops expire. At a full integer wallet, only available room is credited; uncredited value remains in a settled stack. Tests cover 102 dropped Carrots through the 80-item pool with no loss or duplicate credit. The scene contains 596 transforms including the retained legacy and Main Race objects; this is a structural count, not a profiler benchmark.

## 17. Farm upgrade values

| Upgrade | Effect | Base cost | Cost multiplier | Maximum level |
| --- | --- | --- | --- | --- |
| Auto Growth | −10% base growth time/level; 20 → 10s | 120 | 1.6 | 5 |
| Click Growth | +0.02 normalized growth/level; 0.10 → 0.20 | 120 | 1.6 | 5 |
| Harvest Yield | +1 Carrot/harvest/level; 2 → 7 | 160 | 1.6 | 5 |

Auto Growth retains its existing Grow Speed tree unlock costing 120 Coins. Click Growth and Yield use the immediately available Farm node. Purchases preserve every crop's normalized growth. Farm Capacity is blocked in the active playground.

## 18. Safe Blacksmith deactivation

The coordinator rejects visiting Blacksmith and stops advancing crafting. The playground model rejects starting crafts and purchasing inactive production/tree branches, even with debug resources or a previously unlocked node. UI/nav/tooltips that expose those controls are inactive. The backend EquipmentType IDs, Workshop tiers, Metal balance, equipment levels and stat breakdown remain intact. Previously equipped bonuses survive enabling the playground. Legacy stored carrots migrate once into physical drops, and partial training/farm progress is retained.

ForgeVisual's ambient mode flickers the fire and animates sparks independently of crafting. There is no craft reward, active forge purchase or equipment inventory added to the new loop. Main Race Metal remains in the backend reward contract but is hidden because it has no current active sink.

## 19. Main Race camera transition

Cinemachine remains the camera system. Stable and Farm share wide connected framing centered around x −25.7, orthographic size 9.5. Race entry still previews the existing Main Race; Start refreshes the player's real stats, prepares participants, selects the follow camera, waits the **0.15-second EaseInOut** blend, then starts the existing countdown. Return uses the same short blend and restores management.

The Main Race track rendering is hidden in Stable/tree views and restored for race views, keeping race terrain out of the training area. Participants, race logic and the scene stay intact. PlayMode tests verify two completed races, one reward claim per result, no scene reload/object growth, crop progress/drop retention and training pause/resume. Boost/jump input and 30/60/120-FPS race interpolation also pass.

## 20. Polish/game feel

Running horse frames and directional flips; shadows; separated horse variants; new-horse scale entrance and announcement; floating Coins at the physical finish; independent staggered crops; four sizes; green Ready halos; yellow hover highlights; squash/stretch and ready bounce; +Growth text; harvest pop/scatter/landing bounce; settled pile bobbing and stack size; pickup motion/shrink and +Carrots; retained animated currency feedback and horse upgrade flashes; ambient forge glow/sparks; compact upgrade/feeding controls; accurate shared hover/focus details; keyboard inspection; fast camera transitions. Eight optional audio hooks cover crop click/stage/harvest/pickup/lap/upgrade/new horse/transition. Final sound clips are unassigned.

## 21. Current prototype balance

World/Workshop/Playground assets hold the values above. Direct upgrades retain their previous costs: Speed and Boost base 60 Coins, Acceleration base 50, growth 1.45; increases +0.75 Speed, +3 Acceleration and +1 Boost Power. Feed costs **5 Carrots** for permanent **+0.1 Speed**. Initial actual stats are **10 / 12 / 5**. Main Race rivals remain **23 / 24 / 25 Speed**. Coins by place are 500/250/150/100, Tokens 5/3/2/1.

Measured pure-model races with rhythmic boost and correctly timed jumps: fresh player 33.88s versus fastest rival 17.84s. The active policy (one growth click/plot/second, harvesting/collecting ripe crops, feeding and direct upgrades) first wins at **300 seconds**, stats 22.45/21/5. The training-first policy (save for four horses, then three speed/reward levels, then direct stats; no harvesting/feeding) first wins at **1590 seconds**, stats 17.5/21/15. These are deterministic strategy simulations, excluding Main Race time and physical collection delay; they are not promised real-player completion times. Rivals did not need adjustment.

## 22. How to test the complete loop

1. Open `Assets/Racing/Scenes/Milestone3World.unity`, press Play, focus Game. Observe Blacksmith → Farm → Stable → Track and the running horse.
2. Wait through its first 15-second lap. Confirm +35 Coins at the finish and in the wallet, without Metal or Tokens.
3. Click a small carrot and observe +Growth/scale feedback. Compare it with untouched plots. Watch Mid, Large and bouncing/green Ready stages.
4. Click a ready plot. Confirm it resets and drops scatter; the wallet does not credit immediately. Hover/sweep the landed carrots and confirm delayed pickup feedback and wallet credit.
5. Harvest several plots and leave the piles; sweep across multiple/overlapping items. Click over UI to confirm crops are protected.
6. Buy Horses at 500/900/1620 Coins. Confirm two, three and four separated physical horses, no resets of existing horses, and MAX at four. K may be used to test purchases quickly; it is a debug grant.
7. Unlock Acceleration → Training Speed → Training Reward in the existing tree, then buy their compact upgrades. Confirm faster movement and larger lap rewards. Unlock Grow Speed, buy Farm upgrades, compare click progress and harvest counts.
8. Collect five Carrots and Feed. Confirm −5 Carrots and +0.1 real horse Speed. Buy direct stat upgrades too.
9. Leave a crop partially grown and a pile uncollected. Enter Main Race, Start, tap SPACE and jump with E. Training should pause; crops should keep growing. Finish and Return to Stable. Confirm the same world/crops/drops, resumed training and a single result reward.
10. Check no active Blacksmith nav, crafting panel, storage bar, Collect button or Metal display. Check Stable/Farm/tree/race at 1920×1080, 1280×720, 1024×768 and 2560×1080.
11. Run EditMode and PlayMode suites. Evidence is stored alongside this report and in `Screenshots`. Final verification totals are recorded in the results JSON files.

## 23. Known issues and limits

Placeholder sprite/rectangle art remains; the loop has no final sound clips. Progress resets when Play stops; persistent saves and offline production are outside this milestone. Feeding is intentionally uncapped, so sustained active Farm use can quickly dominate direct Speed purchases. Race Tokens currently accumulate without an active spending branch. Hidden legacy objects/code remain in the project. Reapply the focused editor action after changing horse/plot/pool counts so serialized presentation arrays match the settings. No standalone player build or hardware profiler benchmark was run.

## 24. Obsolete systems removable later

After reviewing compatibility and retaining any needed data, remove the inactive ManagementUI progress/icon display, old FarmUI storage/collect controls and decorative crops, inactive production presenters, BlacksmithUI/CraftFeedback and hidden craft/Metal UI. Backend crafting/equipment fields should stay until a save/data migration or deliberate return of that system is defined. Historical scenes/documents/setup logic can be archived separately after checking shared generated assets and references.

## 25. Next milestone recommendation

Prioritize persistent saves for levels, resources, crops, horse phases and pending pickup value, with a defined offline-growth rule. Then tune the active/idle gap and feeding alongside clearer final crop/Stable art and short sound effects. Keep Blacksmith gameplay disabled until it has a distinct purpose. No next-milestone work has been started.
