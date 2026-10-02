# Wheel of Fortune – Vertigo Games Demo

Unity 2021.3.45f2 (LTS) · Android (IL2CPP, ARM64)

**APK, video and screenshots:** see [Releases](../../releases).

## Gameplay
- Spin the wheel each zone; rewards scale with zone depth.
- Normal zones contain a bomb that wipes the collected rewards.
- Every 5th zone is a risk-free Silver spin, every 30th a Golden spin with special rewards.
- Leave before any spin to cash out. After a bomb: give up, or revive with gold.
- Gold collected on cash-out accumulates for the session and pays for revives.

## Architecture
- **Model (`Core`)**: pure C# game rules (`WheelSession`, `ZoneRules`, wheel generators), no Unity dependencies in the logic.
- **Views**: MonoBehaviours that only display state and expose button events.
- **Presenter**: connects model events to views; `GameBootstrapper` is the composition root.
- Patterns: MVP, Observer (C# events), Decorator (`OverriddenWheelGenerator`), Object Pool-style reuse of cells, constructor injection.
- EditMode tests cover zone rules, reward formulas, wheel generation, overrides, session flow and wallet.

## Editing content in the Editor
All tuning lives in `Assets/_Project/Data/game_config.asset`:
- zone intervals, leave rule, starting gold and revive cost
- reward formulas, reward pools, chest progression
- **Slice Overrides**: set any slice of any zone to a specific reward or a bomb

## Design notes
- The brief is ambiguous about when leaving is allowed; default is "before any spin", switchable to "safe zones only" in `game_config`.
- Unity 2021.3.45f2 (latest public 2021 LTS) does not support Android 15+ devices that use 16 KB memory pages; such devices require Unity 2022.3.56f1+.