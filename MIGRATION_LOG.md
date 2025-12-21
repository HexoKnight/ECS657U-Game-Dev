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

## Next Steps

1. **Verify in Unity Editor**: Open project, check for compile errors
2. **Test each scene**: StartMenu, Level1, MainScene, EnemiesDemo
3. **Phase 2** (optional): Add namespaces, implement SO-based event channels
