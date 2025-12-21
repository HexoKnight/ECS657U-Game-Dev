# Migration Log - Architecture & Refactor Playbook

This log tracks all file movements, renames, and structural changes made during the architecture refactoring of Project G.U.P.

---

## Phase 1: Reorganization

### Git Setup
- **Branch**: `refactor/architecture-cleanup`
- **Base**: `feature/enemies-and-hazards-diego`
- **Date**: 2025-12-21

---

## Commits

### Commit 1: Folder Reorganization (276 files)
`1f2ab9d` - Phase 1: Reorganize project folder structure

### Commit 2: Assembly Definitions (58 files)
`700088e` - Add assembly definitions and fix circular dependencies

---

## Folder Structure Created

| Folder | Purpose |
|--------|---------|
| `Assets/_Project/` | First-party project assets |
| `Assets/_Project/Scripts/Core/` | Interfaces, data, events, utilities |
| `Assets/_Project/Scripts/Gameplay/` | Runtime gameplay logic |
| `Assets/_Project/Scripts/Gameplay/Player/` | Player-specific scripts |
| `Assets/_Project/Scripts/Gameplay/Enemies/` | Enemy AI and behaviors |
| `Assets/_Project/Scripts/Gameplay/Hazards/` | Hazard behaviors |
| `Assets/_Project/Scripts/Gameplay/Systems/` | Game systems (checkpoint, difficulty) |
| `Assets/_Project/Scripts/UI/` | UI logic (menus, buttons) |
| `Assets/_Project/Scripts/Editor/` | Editor-only tools |
| `Assets/_Project/Scenes/` | All scenes |
| `Assets/_Project/Prefabs/` | All prefabs |
| `Assets/_Project/Settings/` | Project settings |
| `Assets/_Project/Collectables/` | Collectable assets |
| `Assets/Art/Materials/` | Material assets |
| `Assets/Art/Models/` | (Empty, for models) |
| `Assets/Art/Textures/` | (Empty, for textures) |
| `Assets/Audio/Music/` | (Empty, for music) |
| `Assets/Audio/SFX/` | (Empty, for SFX) |
| `Assets/ThirdParty/` | External packages |

---

## Asset Moves

### Scripts

| Original Location | New Location | Files |
|------------------|--------------|-------|
| `Scripts/Core/*` | `_Project/Scripts/Core/` | DamageData, DamageDealer, DamageType, GameEvents, HealthComponent, IDamageable |
| `Scripts/Static/*` | `_Project/Scripts/Core/` | Options |
| `Scripts/PlayerController.cs` | `_Project/Scripts/Gameplay/Player/` | PlayerController |
| `Scripts/MagneticWalker.cs` | `_Project/Scripts/Gameplay/Player/` | MagneticWalker |
| `Scripts/Enemies/*` | `_Project/Scripts/Gameplay/Enemies/` | All enemy scripts + StateMachine |
| `Scripts/Hazards/*` | `_Project/Scripts/Gameplay/Hazards/` | BubbleStream, StickyTrashHazard |
| `Scripts/Systems/*` | `_Project/Scripts/Gameplay/Systems/` | Checkpoint, CheckpointManager, DifficultyManager |
| `Scripts/WaterCurrent.cs` | `_Project/Scripts/Gameplay/Systems/` | WaterCurrent |
| `Scripts/DollyTrackMount.cs` | `_Project/Scripts/Gameplay/Systems/` | DollyTrackMount |
| `Scripts/*Menu.cs, *Screen.cs` | `_Project/Scripts/UI/` | OptionsMenu, PauseMenu, StartMenu, StartScreen, ImmediateButton |

### Other Assets

| Original Location | New Location | Reason |
|------------------|--------------|--------|
| `Scenes/` | `_Project/Scenes/` | Project scenes |
| `Prefabs/` | `_Project/Prefabs/` | Project prefabs |
| `Settings/` | `_Project/Settings/` | Pipeline assets |
| `Materials/` | `Art/Materials/` | Visual assets |
| `Collectables/` | `_Project/Collectables/` | Game collectables |
| `KinematicCharacterController/` | `ThirdParty/KinematicCharacterController/` | Third-party package |
| `TextMesh Pro/` | `ThirdParty/TextMesh Pro/` | Third-party package |
| `InputSystem/` | `ThirdParty/InputSystem/` | Input configuration |

---

## Assembly Definitions Created

| Assembly | Location | Dependencies | Purpose |
|----------|----------|--------------|---------|
| `GUP.Core` | `_Project/Scripts/Core/` | None | Foundational types, interfaces, events |
| `GUP.Gameplay` | `_Project/Scripts/Gameplay/` | GUP.Core | Runtime gameplay logic |
| `GUP.UI` | `_Project/Scripts/UI/` | GUP.Core | UI logic |
| `GUP.Editor` | `_Project/Scripts/Editor/` | GUP.Core, GUP.Gameplay | Editor-only tools |

---

## Code Changes

### New Files
- `Assets/_Project/Scripts/Core/EntityType.cs` - Enum for cross-assembly entity identification

### Modified Files
- `Assets/_Project/Scripts/Core/IDamageable.cs` - Added `EntityType` property
- `Assets/_Project/Scripts/Core/GameEvents.cs` - Refactored to use interface-based detection
- `Assets/_Project/Scripts/Core/HealthComponent.cs` - Added `EntityType` field and property
- `Assets/_Project/Scripts/Gameplay/Player/PlayerController.cs` - Added `EntityType` property
- `Assets/_Project/Scripts/Gameplay/Enemies/EnemyBase.cs` - Added `EntityType` property

---

## Issues Encountered & Resolutions

### Nested Folder Issues
- **Problem**: Moving directories into pre-created directories caused nesting (e.g., `Settings/Settings/`)
- **Resolution**: Moved contents to parent directory using `Move-Item`, removed empty nested folders

### Circular Dependencies
- **Problem**: `GameEvents.cs` (Core) referenced `PlayerController` and `EnemyBase` (Gameplay)
- **Resolution**: Created `EntityType` enum in Core, updated `IDamageable` interface, refactored `GameEvents.cs` to use `IDamageable.EntityType` instead of `GetComponent<PlayerController>()`

---

## Files Remaining in Old Locations

The following empty folders remain from the old structure (Unity should clean these up):
- `Assets/Scripts/Core/` (empty, .meta only)
- `Assets/Scripts/Static/` (empty, .meta only)  
- `Assets/Scripts/Systems/` (empty, .meta only)

---

## Phase 2: Folder Structure Cleanup

### Commit: Fix duplicated folder nesting

Fixed nested folder structures created during Phase 1 moves:

| Problem | Solution |
|---------|----------|
| `_Project/Scenes/Scenes/` | Moved 4 scenes to `_Project/Scenes/`, removed nested folder |
| `_Project/Prefabs/Prefabs/` | Moved all prefabs (+ 4 subdirs) to `_Project/Prefabs/`, removed nested folder |

### Files Moved

**Scenes (8 files):**
- EnemiesDemo.unity (+.meta)
- Level1.unity (+.meta)
- MainScene.unity (+.meta)
- StartMenu.unity (+.meta)

**Prefabs (22 files + 4 dirs):**
- Collectable.prefab, GenericPlatform.prefab, MagneticPlatform.prefab
- MainCamera.prefab, NormalPlatform.prefab, OldWaterCurrent.prefab
- Player.prefab, VirtualCamera.prefab, WaterCurrent.prefab
- Enemies/, Hazards/, Projectiles/, UI/ subdirectories

---

## Phase 3A: Structural Cleanup & Ownership Enforcement

### Branch: `refactor/phase3-ownership`
### Base: `refactor/phase2-production-quality`

### Milestone 1: Settings vs Gameplay Data

| Move | Rationale |
|------|-----------|
| `Settings/Config.meta` → `Data/Config.meta` | Config assets for gameplay tunables belong in Data |
| `Settings/Events.meta` → `Data/Events.meta` | Event channel assets belong in Data |

**Result**: `_Project/Settings` now contains only URP pipeline/renderer assets.

### Milestone 2: Core Ownership Enforcement

Moved gameplay-specific MonoBehaviours out of Core into Gameplay.

| File | From | To | Namespace Change |
|------|------|----|------------------|
| `DamageDealer.cs` | Core | Gameplay/Combat | GUP.Core → GUP.Gameplay |
| `HealthComponent.cs` | Core | Gameplay/Combat | GUP.Core → GUP.Gameplay |

**Files Updated:**
- `DeadState.cs`: Added `using GUP.Gameplay;`

**Result**: `GUP.Core` now contains only:
- Interfaces: `IDamageable`, `IState`, `IStateMachine`
- Data: `DamageData`, `DamageType`, `EntityType`
- Events: `GameEvents`, Event Channels
- Config SO Types: `PlayerMovementConfig`, `EnemyConfig`, `HazardConfig`
- State Machine Base: `StateBase`, `StateMachineBase`
- Utility: `Options`

---

## Phase 3B: Event Channel Vertical Slice Migration

### Branch: `refactor/phase3-migrations`
### Base: `refactor/phase3-ownership`

### Damage Pipeline Migration

Migrated the damage pipeline from static `GameEvents.cs` to ScriptableObject Event Channels.

#### Commits

| Hash | Description |
|------|-------------|
| `102c4cf` | Add event channel integration to HealthComponent |
| `2edf846` | Add event channel ScriptableObject assets |

#### HealthComponent Changes

Added optional event channel fields:
- `DamageEventChannel damageEventChannel` - Broadcasts when entity takes damage
- `FloatEventChannel healthChangedChannel` - Broadcasts normalized health (0-1)
- `VoidEventChannel deathEventChannel` - Broadcasts when entity dies

Modified methods:
- `TakeDamage()` - Raises `damageEventChannel` and `healthChangedChannel`
- `Die()` - Raises `deathEventChannel`

Legacy `GameEvents` calls retained for backward compatibility.

#### Event Channel Assets Created

| Asset | Type | Purpose |
|-------|------|---------|
| `OnDamageDealt` | DamageEventChannel | Any entity takes damage |
| `OnPlayerHealthChanged` | FloatEventChannel | Player health changes |
| `OnPlayerDied` | VoidEventChannel | Player dies |
| `OnEnemyHealthChanged` | FloatEventChannel | Enemy health changes |

#### Wiring Required

In Unity, assign event channels on Player and Enemy prefabs:
1. Select Player prefab → HealthComponent
2. Assign `OnPlayerHealthChanged` to `Health Changed Channel`
3. Assign `OnPlayerDied` to `Death Event Channel`
4. Repeat for enemies with their respective channels

---

## Phase 3C: Config ScriptableObject Migration

### Branch: `refactor/phase3-configs`
### Base: `refactor/phase3-migrations`

### Milestone 1: PlayerController → PlayerMovementConfig

| Commit | Description |
|--------|-------------|
| `edcde26` | Add PlayerMovementConfig consumption (fallback preserved) |
| `c3e3340` | Add PlayerMovementConfig asset |

**Changes:**
- Added optional `PlayerMovementConfig` field to `PlayerController`
- Config is optional: existing serialized fields work as fallback
- Created `Data/Config/PlayerMovementConfig.asset` with default values

### Milestone 2: EnemyBase → EnemyConfig

| Commit | Description |
|--------|-------------|
| `e415573` | Add EnemyConfig consumption (fallback preserved) |
| `83bd81a` | Add EnemyConfig assets for enemy archetypes |

**Changes:**
- Added optional `EnemyConfig` field to `EnemyBase`
- Config is optional: existing serialized fields work as fallback

**EnemyConfig Assets Created:**
- `AnglerFishConfig.asset`
- `CrabEnemyConfig.asset`
- `ExplodingFishConfig.asset`
- `JellyfishConfig.asset`
- `SpikyFishConfig.asset`

### Milestone 3: Hazards → HazardConfig

**Status:** Skipped - existing hazard scripts (BubbleStream, StickyTrashHazard) don't use damage tunables.
HazardConfig is designed for damage-dealing hazards. If damage hazards are added later, they can use HazardConfig.

---

## Wiring Required (Unity Editor)

### Player Prefab
1. Select Player prefab → PlayerController
2. Assign `Data/Config/PlayerMovementConfig` to `Movement Config`

### Enemy Prefabs
1. Select each enemy prefab → Enemy script (extends EnemyBase)
2. Assign corresponding config from `Data/Config/`

---

## Phase 3D: Hazard Config Migration

### Branch: `refactor/phase3d-hazard-configs`
### Base: `refactor/phase3-configs`

### HazardConfig Extension

Extended `HazardConfig` with additional fields for non-damage hazards:

**Push Effect (BubbleStream):**
- `pushForce` - Upward force applied to player
- `forceRampSpeed` - How quickly force builds up
- `maxVelocityBoost` - Maximum velocity boost

**Slow Effect (StickyTrash):**
- `slowFactor` - How much to slow player (0-1)
- `slowTransitionSpeed` - How quickly slow applies
- `impairVision` - Enable vision impairment

### Commits

| Commit | Description |
|--------|-------------|
| `7357838` | Fix SpikyFishConfig.asset malformed content |
| `1b3d20a` | Add HazardConfig consumption (fallback preserved) |
| `06d0c2f` | Add HazardConfig assets for hazard types |

### HazardConfig Assets Created

| Asset | Purpose |
|-------|---------|
| `BubbleStreamConfig.asset` | Push effect settings (pushForce=15) |
| `StickyTrashConfig.asset` | Slow effect settings (slowFactor=0.5) |

---

## Wiring Required (Unity Editor)

### Hazard Prefabs
1. Select BubbleStream prefab → Assign `BubbleStreamConfig`
2. Select StickyTrashHazard prefab → Assign `StickyTrashConfig`

---

## Phase 4: Player HFSM Implementation

### Branch: `refactor/phase4-player-hfsm`
### Base: `refactor/phase3d-hazard-configs`

### Milestone 1: Core HFSM Support

| Commit | Description |
|--------|-------------|
| `8c17713` | Core: add hierarchical state support (HFSM) |

Added `CompositeStateBase<TContext>`:
- Parent states own and delegate to child substates
- `ChangeSubstate()` for internal transitions
- `RequestParentTransition()` for bubbling up
- `FullStatePath` for debugging hierarchy

### Milestone 2: PlayerContext + HFSM Scaffold

| Commit | Description |
|--------|-------------|
| `ad2db0a` | Player: add PlayerContext + HFSM scaffold |

**New Files:**
- `PlayerContext.cs` - Component refs, config, runtime state
- `PlayerState.cs` - Base class for leaf states
- `PlayerCompositeState.cs` - Base for hierarchical states

### Milestone 3: State Tree Structure

| Commit | Description |
|--------|-------------|
| `47899b1` | Player: add HFSM state tree + transitions scaffold |

**Hierarchy Created:**
```
Root
└─ AliveState (composite)
    └─ LocomotionState (composite)
        ├─ SwimmingState (leaf)
        └─ MagneticTraversalState (leaf)
```

### Design Notes

Current PlayerController uses a simpler enum-based state (Normal/StaticSpline) with unified movement logic.
The HFSM infrastructure is now in place for future state-based expansion.

**What Was Preserved:**
- All existing movement behavior unchanged
- PlayerController class/file stable (prefab safety)
- No GC allocations in state updates

---

## Next Steps

1. **Verify in Unity Editor**: 0 compile errors
2. **Play MainScene**: Movement works as before
3. **Future expansion**: Migrate more logic into HFSM states as needed
