# Enemies & Hazards Framework Guide

## 1. Getting Started

This framework adds Enemies, Hazards, and Checkpoints to the GUP project.

### **Folder Structure**
- `Assets/Scripts/Enemies/`: Enemy logic (`CrabEnemy`, `SpikyFishEnemy`, `ExplodingFishEnemy`).
- `Assets/Scripts/Hazards/`: Hazard logic (`StickyTrashHazard`).
- `Assets/Scripts/Systems/`: System logic (`CheckpointManager`, `Checkpoint`).

## 2. How to Create Prefabs

### **A. Crab Enemy (Ground)**
1. Create a **Cube** or **Sphere**.
2. Add a `BoxCollider` and `Rigidbody` (set `IsKinematic = true` if using simple transform movement, or configure for physics).
3. Add the `CrabEnemy` script.
4. Create two empty GameObjects in the scene to act as **Point A** and **Point B**.
5. Assign them to the `Point A` and `Point B` fields in the inspector.
6. **Chase Settings**:
   - `Detection Radius`: Distance to spot the player.
   - `Chase Radius`: Distance to give up chase.
   - `Chase Speed`: Speed when chasing.

### **B. Spiky Fish (Swimming)**
1. Create a **Sphere** (visual).
2. Add `SpikyFishEnemy` script.
3. Create a list of empty GameObjects for **Waypoints**.
4. Drag them into the `Waypoints` list in the inspector.
5. **Wait Time**: Time to pause at each waypoint (default 1s).

### **C. Exploding Fish (Kamikaze)**
1. Create a **Sphere** (Red color recommended).
2. Add `ExplodingFishEnemy` script.
3. Set `Detection Radius` (e.g., 10) and `Explosion Radius` (e.g., 5).
4. (Optional) Assign an explosion particle effect prefab.

### **D. Sticky Trash Hazard**
1. Create a **Cube** (set material to something semi-transparent/green).
2. Set Collider to **Is Trigger**.
3. Add `StickyTrashHazard` script.
4. Adjust `Slow Factor` (0.1 - 0.9).

### **E. Checkpoints**
1. Create an empty GameObject.
2. Add a `BoxCollider` (Is Trigger).
3. Add `Checkpoint` script.
4. For the first spawn point, check `Is Start Point`.
5. Ensure a `CheckpointManager` exists in the scene (create an empty GameObject with `CheckpointManager` script).

## 3. Dynamic Difficulty Adjustment (DDA) Hooks

The framework is designed to be controlled by a future `DifficultyManager`.

**Global Multipliers:**
You can modify these static variables at runtime to affect ALL enemies:
```csharp
EnemyBase.GlobalSpeedMultiplier = 1.2f; // Make all enemies 20% faster
EnemyBase.GlobalDamageMultiplier = 1.5f; // Make all enemies deal 50% more damage
```

**Individual Tuning:**
Every enemy has public fields:
- `moveSpeed`
- `contactDamage`
- `contactDamage`
- `detectionRadius` (Crab, Exploding Fish)
- `chaseSpeed` (Crab)

## 4. Testing

1. Open a scene.
2. Place a `CheckpointManager`.
3. Place a `Checkpoint` (Is Start Point = true).
4. Place a `CrabEnemy` and assign patrol points.
5. Press Play.
6. Walk into the Crab -> You should take damage and get knocked back.
7. Fall off the map -> You should respawn at the checkpoint.
