# Milestone 2 validation — 5 October 2026

Validated with Unity **6000.6.2f1**, using the project's installed packages, in an isolated project copy. Delivered C# sources, scene files, progression configuration, and build settings were compared by SHA-256 with the validated copy before cleanup. The open main Editor was not used for test execution.

- **EditMode: 20/20 passed.** Includes the original horse simulation/scene checks, atomic purchases, insufficient funds, increasing costs, duplicate/stale reward tokens, overflow protection, all finishing-position reward amounts, and configured scene cameras/Canvases/Stable asset references.
- **PlayMode: 3/3 passed.** Includes the existing synthetic SPACE/E input test, obstacle penalties, every racer finishing, first/fourth positions, debug restart, result rows updating while AI still race, and visual interpolation at 30/60/120 FPS.
- The new loop test completed **four Stable → Race → Result → Stable cycles**, purchased all three upgrade categories, checked immediate Coins/level/price/feedback refreshes, rejected an unaffordable purchase, and verified that the next race uses the purchased parameters while AI speed stays fixed.
- Stat verification: Speed increases movement/top speed, Acceleration increases early speed, and Boost Power increases the actual boost ceiling and strength per tap. Four Speed upgrades reduced simulated finish time by at least **15%**, with the same missed obstacles.
- Fresh progression asset creation and repeated scene setup were verified. The scene setup saves/reloads new configuration before assigning the Stable reference.
- No unexpected gameplay logs were received in the passing tests. No C# compilation errors occurred in the final runs.
- Screenshots were visually inspected at **1920×1080** and **1024×768**. The final Stable label was moved above the horse to avoid overlap.

## Evidence

- `editmode-results.xml`, `playmode-results.xml`: Unity Test Framework results.
- `unity-bake.log`, `unity-editmode.log`, `unity-playmode.log`: final setup and test logs.
- `stable-start.png`, `stable-start-1024x768.png`, `stable-upgraded.png`: horse, stats, shop, and upgrade feedback.
- `race-reward.png`, `race-result.png`, `race-results-pending.png`: position, reward, balance, return button, and unfinished AI rows.
- `race-gameplay.png`, `race-gameplay-1920x1080.png`, `race-gameplay-1024x768.png`: preserved racing camera/gameplay.

The isolated project's generated Library and package-cache copy were removed after validation. Tests and editor setup commands remain in the main project.

## Limits

These checks establish the technical loop and observable stat growth; subjective pacing/fun still benefits from manual playtesting. Progression intentionally resets when Play stops or the application closes. One race and prototype visuals/UI are retained. No permanent save system or Milestone 3 features were added.
