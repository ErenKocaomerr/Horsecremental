# Minimal UI

The saved gameplay scene is `Assets/Racing/Scenes/Milestone3World.unity`.

- Upgrade Tree: 11 icon nodes, 80×80 frames, 40×40 pixel icons, connections clipped at square edges.
- Shared top-right details show on pointer hover or keyboard focus; retain the last inspected item and clear on area/phase changes.
- Tab / Shift+Tab inspect visible targets, including unavailable upgrades. Arrow keys traverse each upgrade group. Submit/click obey existing purchase guards.
- Resource counters use icons and numbers. Short stat totals, timers, navigation/action labels, and race results remain visible.
- Feedback clears after 2.5 seconds. Craft completion retains its existing timed toast.
- `UIPresentation.asset` contains icon sprites and tree positions; economic configuration is unchanged.
- `Horsecremental > Simplify UI` rebuilds the presentation in the current world. Upgrade Tree and Milestone 4 setup also apply it. Repeated execution keeps one details panel and the same object count.

Validation: 45 EditMode tests and 9 PlayMode tests passed during implementation. The three new UI tests cover hover/focus, unavailable/max-level inspection, atomic purchases, live details, clearing, feedback expiry, keyboard traversal, and layout bounds/overlap checks. Existing tests exercise the complete race/production/equipment/upgrade loop.

`Screenshots/` contains Stable, Farm, Blacksmith, Tree, and Race entry at 1920×1080, 1280×720, 1024×768, and 2560×1080. `Before/` preserves the original gameplay scene. The scene GUID remains `8789f1f4a37cec24183629f77a4555ec`.
