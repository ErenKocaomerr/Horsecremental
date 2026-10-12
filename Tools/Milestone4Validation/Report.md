# Milestone 4 implementation report

Implemented and validated on 2026-10-06 in Unity 6000.6.2f1. All 33 EditMode and 4 PlayMode tests passed; the final Console query returned zero errors and zero warnings. The existing `Assets/Racing/Scenes/Milestone3World.unity` remains the gameplay scene and retains its GUID. Milestone 5 has not been implemented.

## 1. What was implemented

Metal rewards, three deterministic equipment slots with five tiers each, one timed Blacksmith crafting slot, automatic equipment replacement, six production upgrades, final-stat breakdowns, improved Horse Power guidance, separate Farm/Blacksmith management views, and production/resource/craft feedback are integrated into the existing game.

The existing horse simulation, SPACE Boost, E Jump, direct upgrades, feeding, race track, race token protection, runtime session, and Cinemachine architecture are reused. The Blacksmith uses a small placeholder forge/anvil/workbench/crate workstation in the existing world. No new gameplay scene, inventory, crafting queue, extra material, crop, random equipment system, offline production, or Championship was added.

## 2. Scripts created

Under `Assets/Racing/Runtime/`:

| Script | Responsibility |
|---|---|
| `WorkshopRules.cs` | Pure C# equipment/production enums, immutable configuration values, settings contract, and stat breakdown |
| `WorkshopSettings.cs` | Inspector-editable equipment and production configuration asset |
| `PlayerEquipment.cs` | Equipment/crafting portion of the central PlayerProgression model |
| `PlayerProduction.cs` | Production-upgrade portion of the same central model |
| `AreaPanel.cs` | Area-specific visibility, fade, and input blocking |
| `AreaNavigationUI.cs` | Stable/Farm/Blacksmith/Race selection and active area heading |
| `ProductionUpgradeUI.cs` | Reusable production purchase control and cost/effect presentation |
| `FarmUI.cs` | Existing collect/feed loop presented in its own Farm view |
| `BlacksmithUI.cs` | Current/next equipment, costs, busy state, timer and craft controls |
| `CraftFeedback.cs` | Global completion toast, scale response, and serialized UnityEvent audio/effect hook |
| `ForgeVisual.cs` | Crafting hammer motion, forge glow, sparks, and completion burst |

Editor: `Milestone4SceneSetup.cs`, `Milestone4UIBuilder.cs`.

Tests: `Tests/Editor/Milestone4ModelTests.cs`, `Tests/Editor/Milestone4SceneTests.cs`, and `Tests/PlayMode/Milestone4PlayModeTests.cs`.

Unity generated the new `.meta` files. New configuration: `Assets/Racing/Config/Workshop.asset`.

## 3. Scripts and assets modified

Runtime: `PlayerProgression.cs`, `ProgressionSettings.cs`, `IIdleSettings.cs`, `WorldSettings.cs`, `RaceRewards.cs`, `RaceUI.cs`, `GameplayWorld.cs`, `WorldCameraNavigation.cs`, `StableUI.cs`, `ManagementUI.cs`, and `CurrencyFeedback.cs`.

Tests: `Milestone3ModelTests.cs` now checks the intentionally changed camera count and fixed rival values; its direct-upgrade regression still verifies improved actual racing performance. `ProgressionLoopTests.cs` exercises the complete Milestone 4 loop through UI controls.

Existing assets modified: `Milestone3World.unity`, `World.asset`, `Progression.asset`, `RivalSteady.asset`, `RivalQuick.asset`, and `RivalFast.asset`. No packages or assembly definitions were changed. The legacy scenes were not rebuilt or overwritten.

The pre-implementation scripts/assets/metas/world are preserved under `Tools/Milestone4Validation/BackupBeforeImplementation/Racing/`. The original Build Settings file is also backed up. The workspace has no Git repository.

## 4. How Metal works

`PlayerProgression.Metal` is the only Metal balance. Training credits Coins and Metal together. Main Race rewards use the existing race token, so both currencies are paid once and stale/duplicate results pay neither. Resource balances saturate at `int.MaxValue`; feedback events report the actual balance delta.

Training begins at **35 Coins + 2 Metal every 15 seconds**. Main Race places 1-4 award **500/250/150/100 Coins** and **15/10/6/3 Metal**. Positional rewards are editable in `Progression.asset`; Training Metal is editable in `World.asset`.

Crafting spends Coins and Metal atomically at start. A missing resource or occupied slot leaves both balances unchanged. Production upgrades spend Coins only.

## 5. Equipment data structure

`Workshop.asset` contains separate Horseshoes, Saddle, and Bridle tier arrays. Each tier exposes its name, Coin cost, Metal cost, base crafting duration, and relevant stat bonus in the Inspector. Its production array contains the six named upgrade rules: base Coin cost, growth, effect per purchase, and maximum level.

The runtime model stores three integer equipment tiers and one active craft record. `PlayerEquipment.cs` and `PlayerProduction.cs` are partial files of **the same PlayerProgression class**, not independent resource/state managers. Configuration values cross into the model through pure C# structs and `IWorkshopSettings`.

There are no inventory copies, random rolls, equipment rarity, equip/unequip UI, or loot drops. Crafting progresses to the next tier only.

## 6. Final Horse Stats and Horse Power

For each racing stat:

```text
Final = Base + Direct upgrade bonus + Feeding bonus + Current equipment bonus
Direct bonus = (direct upgrade level - 1) * configured increase
Feeding affects Speed; Food contribution is zero for Acceleration and Boost Power
Horseshoes -> Speed; Saddle -> Acceleration; Bridle -> Boost Power
Horse Power = Final Speed * 10 + Final Acceleration * 0.25 + Final Boost Power * 5
```

The current equipment bonus replaces the old bonus; past tier bonuses are not stacked. Base HorseStats assets remain separate from runtime progression. Stable UI displays all four contributions, final values, and Horse Power. Training presentation uses the same power property. Training remains guaranteed and timer-based; no difficulty tiers were introduced.

Boost weighting was increased from the old UI-only x1 to x5 so the guidance reflects sustained Boost's practical race contribution. Recommended Power is 245, with the existing Very Hard / Hard / Competitive guidance thresholds. It is guidance, not a race access gate or a guarantee of victory.

`RacePrototype.RefreshPlayer()` continues to pass final parameters into the actual HorseMovement/HorseSimulation before an attempt. Jump, gravity, obstacle penalties, Boost tap gain, and Boost drain retain their original rules. Equipment completing during a race updates progression immediately; the current attempt retains its parameter snapshot, and the next attempt uses the new equipment.

## 7. All equipment tiers and bonuses

| Tier | Horseshoes: Speed | Saddle: Acceleration | Bridle: Boost Power |
|---|---|---|---|
| 0 | Old Horseshoes: +0 | Old Saddle: +0 | Basic Bridle: +0 |
| 1 | Iron Horseshoes: +1.5 | Leather Saddle: +3 | Reinforced Bridle: +1.5 |
| 2 | Reinforced Horseshoes: +3 | Racing Saddle: +6 | Racing Bridle: +3 |
| 3 | Steel Racing Horseshoes: +5 | Professional Saddle: +10 | Professional Bridle: +5 |
| 4 | Champion Horseshoes: +7 | Champion Saddle: +14 | Champion Bridle: +7 |

The cards show both the new tier's total bonus and the gain over the current tier. Horseshoe improvements are +1.5/+1.5/+2/+2 Speed, compared with +0.75 from a direct Speed purchase.

## 8. Equipment costs and crafting times

All three slots share these initial prototype prices/durations; their data is independently editable:

| Next tier | Coins | Base Metal | Base time |
|---|---:|---:|---:|
| 1 | 100 | 5 | 5s |
| 2 | 300 | 12 | 8s |
| 3 | 700 | 25 | 10s |
| 4 | 1500 | 50 | 12s |

Tier 0 is the starting equipment and cannot be crafted. Forge upgrades reduce the duration and Metal cost as described below. Current reduced costs/time are shown before purchase. At maximum Forge Speed, times are 2.5/4/5/6s. At maximum Metal Efficiency, Metal costs are 4/9/19/38.

## 9. Blacksmith crafting

CRAFT checks the next tier, single-slot availability, and both balances. A successful start deducts resources once and captures the target tier/duration. Timer progress lives in PlayerProgression and is ticked by GameplayWorld rather than the Blacksmith UI.

At completion, the target tier automatically becomes equipped and the slot becomes available. Extra elapsed time cannot complete the same craft twice or manufacture another item. There is no queue, cancel/refund flow, or instant completion button. Production upgrades bought during a job affect future jobs, preserving the current duration/progress and already-paid costs.

Busy controls explain which item occupies the forge. Insufficient funds show missing Coin/Metal amounts; costs display separate availability marks. Buttons remain usable for feedback instead of silently rejecting an unaffordable click.

## 10. Forge upgrades

Each upgrade starts at level 0, has five purchases, and has Inspector-editable rules. Effective calculations retain safety floors even if configuration is made more aggressive.

```text
Craft duration = base duration * max(0.5, 1 - Forge Speed level * effect)
Metal cost = max(1, ceil(base Metal * max(0.75, 1 - Metal Efficiency level * effect)))
Upgrade price = ceil(base Coin cost * growth^current level)
```

Forge Speed's default effect is 0.10, giving 100/90/80/70/60/50% duration. Metal Efficiency's default effect is 0.05, giving 100/95/90/85/80/75% Metal cost. Fractional Metal rounds up; small early recipes may need multiple efficiency upgrades before losing a whole Metal from their price.

## 11. Farm upgrades

Grow Speed reduces the existing carrot interval by 10 percentage points per purchase, to a minimum of 50% of the base interval: **20/18/16/14/12/10 seconds**. Farm Capacity adds five stored Carrots per purchase: **10/15/20/25/30/35**.

Production stops at storage capacity without banking catch-up time. COLLECT transfers stored output to the separate collected-Carrot balance. Collected Carrots retain the existing uncapped runtime inventory behavior. FEED still spends five collected Carrots for +0.1 Speed. The header labels collected Carrots and Farm storage separately so these amounts cannot be mistaken for one capped balance.

Interval upgrades preserve fractional progress: a half-complete production cycle stays half complete after a purchase. Capacity upgrades let a full Farm resume production.

## 12. Training upgrades

Training Speed changes the completion interval to **15/13.5/12/10.5/9/7.5 seconds**. Training Reward increases base Coins by 15% per level, up to x1.75. The configured 35-Coin reward rounds up to **35/41/46/51/56/62 Coins** at levels 0-5.

Metal remains two per Training completion. Training Speed increases Metal per minute naturally by completing races more frequently. Equipment increases final stats and the shared Horse Power used in Training presentation; it does not add a separate Training stat model or uncontrolled reward multiplier.

## 13. Production during Main Race

| State | Training | Farm | Blacksmith |
|---|---|---|---|
| Stable, Farm, Blacksmith, or race-entry browsing | Active | Active | Active |
| Main Race preparation/countdown/running/results | Paused | Active | Active |
| Return camera blend | Paused | Active | Active |
| After return blend | Resumes from remainder | Continues | Continues |

The always-active world coordinator handles production. Hiding a panel cannot stop its machine. No closed-game time is calculated. No scene reload occurs on navigation, race attempts, or return.

## 14. Main Race balance changes

The three existing fixed rivals now have **23 / 24 / 25 Speed**, retaining 12 Acceleration, existing jump timing, and no Boost. They never scale with the player. The track remains 420 units with the original five hurdles.

Deterministic tests with correct jumps and two Boost taps per second report:

| Horse/path | Result |
|---|---|
| Fresh horse | 33.88s; loses to every rival |
| Slowest / fastest rival | 19.22s / 17.84s |
| Existing direct-upgrade/feeding progression after 10 minutes | 23.32s; faster than fresh, still behind the rivals |
| Production-first spending path | Wins after 21 simulated Training minutes, tiers 2/2/2, stats 18.05 / 24 / 13 |
| Equipment-first spending path | Wins after 24 simulated Training minutes, tiers 2/2/2, stats 18.25 / 21 / 13 |

These paths use earned Training resources, farming/feeding, production purchases, equipment, and direct upgrades. Maximum equipment is not required. The accelerated PlayMode loop also demonstrates a fresh fourth-place result, later first-place results, correct payouts, and repeated returns through actual UI controls. Race rewards remain substantially larger per completion than Training rewards.

## 15. Polish and game feel

- Five Cinemachine cameras through the existing physical camera/Brain: Stable, Farm, Blacksmith, race entry, race follow. EaseInOut blends are 0.6s.
- Clear active area heading and four management navigation buttons. Buttons hide during Main Race to leave the existing instructions clear.
- Relevant area panels only, short fades, and immediate input blocking for hidden views.
- Preserved 1920x1080 Scale With Screen Size setup; screenshots inspected at 1920x1080, 1280x720, 1024x768, and 2560x1080. Blacksmith also checked at 4:3.
- Persistent smoothed Coin/Metal counters, collected/storage Carrot labels, and separate gain/spend popups. Popups drift below their totals without crossing them.
- Existing button scale pulse, direct upgrade before/after feedback, and explicit production/craft shortages.
- Craft countdown/progress, busy messages, global completion toast with a scale punch, and automatic-equipment confirmation.
- Animated hammer, cycling forge glow, lightweight spark sprites, and a completion burst. A completion UnityEvent is available for future sound/effects.
- Existing Boost bar, horse interpolation/animation/tint, race countdown, result rows, and camera feedback remain functional.

## 16. Current prototype economy values

| Production upgrade | Effect | Coin prices for purchases 1-5 |
|---|---|---|
| Forge Speed | -10 percentage points of base time | 150 / 240 / 384 / 615 / 984 |
| Metal Efficiency | -5 percentage points of base Metal | 200 / 320 / 512 / 820 / 1311 |
| Grow Speed | -10 percentage points of base time | 120 / 192 / 308 / 492 / 787 |
| Farm Capacity | +5 stored Carrots | 100 / 160 / 256 / 410 / 656 |
| Training Speed | -10 percentage points of base time | 150 / 240 / 384 / 615 / 984 |
| Training Reward | +15% of base Coins | 180 / 288 / 461 / 738 / 1180 |

All use default price growth x1.6 and maximum level 5. Prices and reward calculations normalize floating-point noise before rounding up.

Baseline Training produces 140 Coins and 8 Metal per management minute. Fully upgraded Training produces 496 Coins and 16 Metal per management minute. Baseline/max Farm growth is 3/6 Carrots per active game minute, subject to storage capacity. Feeding, direct stat bonuses, and direct price growth x1.45 remain unchanged from Milestone 3. Starts remain zero resources, direct upgrade levels 1, equipment tiers 0, and production levels 0.

## 17. How to test the complete loop

1. Open **Horsecremental -> Open Milestone 3 World** and press Play. The existing world is already saved with Milestone 4; setup is not required for ordinary testing.
2. At Stable, wait 15 seconds for +35 Coins and +2 Metal. Inspect stat source breakdowns and Horse Power. Buy direct or Training upgrades when affordable.
3. Choose FARM. Observe storage/growth, buy Grow Speed or Capacity, COLLECT carrots, and FEED with five collected Carrots. Check the food Speed contribution.
4. Choose BLACKSMITH. Check all current/next tiers and costs. Try an unaffordable craft to see exact missing amounts. Once funded, craft Horseshoes; immediately try another slot to verify busy feedback and no second payment.
5. Browse Farm/Stable while the job advances. Observe the completion toast and newly equipped tier; Speed/Power should increase. Buy Forge Speed or Metal Efficiency and compare future crafting time/cost.
6. Choose RACE or press D, then START MAIN RACE. Tap SPACE rhythmically and use E near the yellow hurdle markers. A fresh horse should lose badly.
7. Check placement and Coin/Metal payouts. Farm and any active craft should have progressed while Training's timer remained paused. Return to Stable and confirm Training resumes.
8. Continue using multiple progression paths for approximately 15-30 minutes of prototype management. Tier-2 equipment plus feeding/direct/production upgrades can become competitive; no max-tier requirement exists.
9. Repeat attempts/returns and verify no duplicated rewards, scene loads, or extra racers/cameras. Equipment completing mid-race affects the next attempt's parameters.

Automation: run EditMode assembly `Horsecremental.Racing.Tests` and PlayMode assembly `Horsecremental.Racing.PlayModeTests`. Final validation is **33 EditMode tests + 4 PlayMode tests**, all passing, zero skipped. The loop advances 24 minutes through actual world timers at an accelerated time scale, using UI controls. Keyboard/Boost/Jump and interpolation at 30/60/120 FPS are also checked. Detailed results are in `Validation.json`; screenshots are under `Screenshots/`.

The in-place **Horsecremental -> Setup Milestone 4 World** command can rewire/rebuild Milestone 4 management presentation if needed. It was run repeatedly without duplicating workstation cameras or controls. It restores the prototype rival speeds, Training Metal, recommended Power, and blend values; equipment/production prices already saved in Workshop.asset are retained. Do not use the Milestone 3 setup to update this world: that older command recreates it from source scenes.

## 18. Known issues and limits

- Workstation and Farm artwork are deliberately simple placeholders. No visual saddle/bridle/horseshoe attachments were added to the horse.
- Progression is runtime-only and resets on entering Play, as before. No save/load or offline production was requested.
- Equipment completing during an active attempt takes effect in the next attempt. The active simulation remains stable instead of being reset halfway through a race.
- The 21/24-minute model paths assume good jumping and rhythmic Boost. Human results depend on input and spending choices; the target is prototype pacing rather than a guaranteed win time.
- Metal can accumulate when Coins limit purchases; no additional resource sink or economy expansion was introduced.
- The audio hook exists but has no final sound asset assigned.
- Unity MCP emitted a transport warning during editor recompilation/domain reload in earlier setup passes. This is separate from gameplay. Final gameplay validation checks for unexpected logs and Console errors/warnings; exact final Console results are recorded in Validation.json.

## 19. Obsolete systems / scripts that can safely be removed

No working system was deleted. Legacy Milestone 1/2 scenes are not used by gameplay but remain source/reference assets and are used by existing editor/test workflows. Their builders and `Milestone3UIBuilder` should be retained until those workflows are intentionally retired; deleting individual files would break their callers.

The unused `GameScenes.STABLE`, `GameScenes.RACE`, and `GameScenes.ConsumeRaceStart()` compatibility members have no remaining C# callers and can be removed in a later cleanup. Keep `GameScenes.StartRace/ReturnToStable/WORLD`, which remain active navigation/test entry points. Old combined Farm UI controls in ManagementUI were replaced by FarmUI rather than duplicated.

## 20. Recommendation for Milestone 5

Use the now-working Stable loop as the foundation for a small fixed Championship progression with clear win requirements and larger rewards. Before increasing progression length, consider runtime save/load and additional human pacing tests. Keep equipment deterministic, keep one material/crop, and avoid inventory/rarity/prestige expansion. This is a recommendation only; no Milestone 5 work was started.
