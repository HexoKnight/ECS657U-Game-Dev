# Migration Log - Architecture & Refactor Playbook

This log tracks all file movements, renames, and structural changes made during the architecture refactoring of Project G.U.P.

---

## Phase 1: Reorganization

### Git Setup
- **Branch**: `refactor/architecture-cleanup`
- **Base**: `feature/enemies-and-hazards-diego`
- **Date**: 2025-12-21

---

### Folder Structure Created

| Folder | Purpose |
|--------|---------|
| `Assets/_Project/` | First-party project assets |
| `Assets/_Project/Scripts/` | All project scripts |
| `Assets/_Project/Scripts/Core/` | Interfaces, data, events |
| `Assets/_Project/Scripts/Gameplay/` | Runtime gameplay logic |
| `Assets/_Project/Scripts/Gameplay/Player/` | Player-specific scripts |
| `Assets/_Project/Scripts/Gameplay/Enemies/` | Enemy AI and behaviors |
| `Assets/_Project/Scripts/Gameplay/Hazards/` | Hazard behaviors |
| `Assets/_Project/Scripts/Gameplay/Systems/` | Game systems |
| `Assets/_Project/Scripts/UI/` | UI logic |
| `Assets/_Project/Scripts/Editor/` | Editor-only tools |
| `Assets/_Project/Scenes/` | All scenes |
| `Assets/_Project/Prefabs/` | All prefabs |
| `Assets/_Project/Settings/` | Project settings |
| `Assets/Art/` | Visual source assets |
| `Assets/Audio/` | Audio assets |
| `Assets/ThirdParty/` | External packages |

---

### Asset Moves

*Moves will be logged below as they occur...*

