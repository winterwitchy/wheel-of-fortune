# Wheel of Fortune – Vertigo Games Demo

Unity 2021.3.45f2 (LTS) · Android (IL2CPP, ARM64)

**APK, video and screenshots:** see [Releases](https://github.com/winterwitchy/wheel-of-fortune/releases#release-v1.1.0).

## Setup
1. Install Unity **2021.3.45f2** with Android Build Support (SDK, NDK, OpenJDK).
2. Clone the repository and open the project folder from Unity Hub.
3. Open `Assets/_Project/Scenes/Game.unity` and press Play. The Game view is designed for 1920x1080 and also checked at 4:3 and 20:9.

## Running the tests
Window → General → Test Runner → EditMode → Run All.

The tests live in `Assets/_Project/Tests/EditMode` and cover zone rules, reward formulas and pools, wheel generation, slice overrides and the bomb rule, the session flow, revive pricing, wallet, inventory, persistence and amount formatting.

## Building the APK
Reward icons are loaded through Addressables, so the Addressables content has to be built before the player.

1. File → Build Settings → select Android → Switch Platform.
2. Window → Asset Management → Addressables → Groups → Build → New Build → Default Build Script.
3. File → Build Settings → Build.

Player Settings already target IL2CPP and ARM64. Step 2 is only required when "Build Addressables on Player Build" is turned off in Preferences; running it manually is always safe.

## Gameplay
- Every zone has a wheel with 8 slices. Collected rewards are listed on the left and the progress bar shows the next safe and super zones.
- **Normal zones** (Bronze spin) contain exactly one bomb. Hitting it loses the collected rewards unless you revive.
- **Safe zones** (Silver spin) come every 5th zone, have no bomb and pay ×2.
- **Super zones** (Gold spin) come every 30th zone, have no bomb, pay ×5 and use their own special rewards.
- **Leave** becomes available after the first collected reward and can be used between spins. Leaving cashes out every collected reward.
- **Revive** costs gold: 25 for the first revive of a run, doubling with every further revive. The price resets when a new run starts. The player starts with 100 gold.
- Gold, currencies and items are saved between sessions.

Reward quality grows with depth:

| Zones | Slices |
|---|---|
| 1–10 | cash 2, gold 2, points 1, consumable 1, chest 1 |
| 11–20 | cash 1, gold 1, points 1, consumable 2, chest 2 |
| 21–30 | cash 1, gold 1, points 1, consumable 2, chest 1, tier 1 skin 1 |
| 31+ | cash 1, gold 1, consumable 1, chest 1, tier 1–2 skins 3 |

- Amounts grow with the zone and with the zone multiplier.
- If the same item lands on a wheel twice, the second copy pays double.
- Skins never repeat within a wheel. When the skin pool runs out, the slice falls back to a higher chest.
- The chest level rises every 10 zones.

## Architecture
- **Core** (`WheelGame.Core` assembly, no engine references): game rules and state. This includes `WheelSession`, `ZoneRules`, the wheel generators (`ProceduralWheelGenerator`, `OverriddenWheelGenerator` and `WheelBombRule`), reward sources, revive pricing, wallet and inventory, progress persistence and the typed event bus.
- **Data**: ScriptableObjects for everything that is tuned in the Editor (`GameConfig`, `ZoneDefinition`, reward pools, reward items). `CoreModelFactory` turns them into Core models.
- **Views**: `*Mono` MonoBehaviours that only display state, play animations and publish input events on the event bus.
- **Presentation**: `CurrencyPresenter`, `ZoneProgressPresenter`, `WheelPresenter`, `RewardsPresenter` and `PopupsPresenter` connect session events to the views.
- **Infrastructure**: `GameBootstrapperMono` is the composition root. `AddressableIconProvider` loads reward icons from a sprite atlas through Addressables, `JsonFileProgressStore` saves progress and `UnityRandomProvider` wraps `UnityEngine.Random`.
- Patterns: MVP, Observer through a typed event bus, Decorator (`OverriddenWheelGenerator`), Strategy (reward sources), object pooling for reward cells, constructor injection.

## Editing content in the Editor
All tuning starts from `Assets/_Project/Data/game_config.asset`:
- zone definitions (interval, priority, multiplier, bomb, visuals) in `Data/Zones`
- zone bands and their slice composition
- reward pools and amount formulas in `Data/Pools`, reward items in `Data/Rewards`
- leave rule, starting gold, revive currency, base cost and multiplier
- wheel spin duration, turns and ease, and the target frame rate
- **Slice Overrides**: force any slice of any zone to a specific reward or a bomb. After overrides, the bomb rule makes sure a normal zone still has exactly one bomb: the most recently applied bomb is kept, and if none remains the least recently altered slice becomes the bomb.

Invalid configuration is reported as Console warnings as soon as `game_config` is edited.

## Design notes
- Progress is saved to `Application.persistentDataPath`. To use an in-memory save instead, change `SAVE_MODE` in `GameBootstrapperMono` to `SaveMode.Temporary`.
- Unity 2021.3.45f2 (latest public 2021 LTS) does not support Android 15+ devices that use 16 KB memory pages; such devices require Unity 2022.3.56f1+.

## The answer to the question
UnityRandomProvider is the Unity-side implementation of the IRandomProvider interface defined in Core, and it wraps UnityEngine.Random.Range. It is created once in GameBootstrapperMono and passed through constructors to the session, the wheel generator and the bomb rule.

The idea was proposed by AI, and given the explanation behind, I took the initiative of separating it this way for the following reasons:

- **Keeping Core independent of Unity:** The WheelGame.Core assembly is compiled with noEngineReferences: true, so nothing in Core can call UnityEngine.Random. Randomness comes into Core from the outside through the interface, and the Unity dependency stays in the Infrastructure layer.
- **Deterministic tests:** In the EditMode tests, FakeRandomProvider returns predefined values. This lets me fix the winning slice, the shuffle result and the slice the bomb moves to. For example, Generate_RewardOverrideOnBombSlot_MovesBombToUnalteredSlice checks exactly which slice the bomb ends up on, which is only possible because the randomness is controllable. Since UnityEngine.Random is static and global, doing this reliably in tests would be difficult.

The cost is a small layer of indirection, but I think it is worth it for testability and a clean separation between layers.
