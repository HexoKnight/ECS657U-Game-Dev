# Enemy AI Documentation

## Overview

The Enemy AI system uses a **State Machine Pattern** to create modular, extensible enemy behaviors. Each enemy type extends `EnemyBase` and can use shared states or create custom ones.

## Architecture

```
EnemyBase (abstract)
├── Implements: IDamageable
├── Contains: EnemyStateMachine
├── Contains: PlayerDetector
└── Derived classes: CrabEnemy, ExplodingFishEnemy, SpikyFishEnemy, etc.

EnemyStateMachine
└── Manages: EnemyState transitions

EnemyState (abstract)
├── IdleState
├── PatrolState
├── AlertState
├── ChaseState
├── AttackState
├── DeadState
└── Custom states per enemy type
```

---

## Core Components

### EnemyBase
**Location:** `Assets/Scripts/Enemies/EnemyBase.cs`

Base class for all enemies. Provides health management, state machine integration, and combat functionality.

**Key Inspector Sections:**

| Section | Fields |
|---------|--------|
| Health | maxHealth, invulnerabilityTime |
| Movement | moveSpeed, chaseSpeed, rotationSpeed |
| Combat | attackDamage, knockbackForce, attackRange, attackCooldown |
| Detection | detectionRadius, chaseRadius, losePlayerTime |
| Patrol | usePatrol, patrolWaypoints, patrolRadius, patrolWaitTime |
| State Machine | useAlertState, alertDuration, idleDuration, deathDuration |

**Key Methods:**
- `GetInitialState()` - Override to set starting state
- `PerformAttack()` - Override for custom attack behavior
- `OnSpawn()` - Called when enemy is initialized

---

### EnemyStateMachine
**Location:** `Assets/Scripts/Enemies/StateMachine/EnemyStateMachine.cs`

Manages state transitions and executes state logic.

```csharp
// Change state
stateMachine.ChangeState(new ChaseState(stateMachine, enemy));

// Get current state
string currentState = stateMachine.CurrentStateName;
```

---

### EnemyState
**Location:** `Assets/Scripts/Enemies/StateMachine/EnemyState.cs`

Abstract base for all states.

```csharp
public abstract class EnemyState
{
    public abstract string StateName { get; }
    public virtual void Enter() { }
    public abstract void Execute();     // Called every frame
    public virtual void Exit() { }
    public abstract void CheckTransitions();
    public virtual void FixedExecute() { }
    public virtual void OnTakeDamage(DamageData damage) { }
}
```

---

### PlayerDetector
**Location:** `Assets/Scripts/Enemies/Sensors/PlayerDetector.cs`

Sensor for detecting player proximity.

**Key Methods:**
- `IsPlayerInRange(float range)` - Distance check
- `GetDistanceToPlayer()` - Exact distance
- `GetDirectionToPlayer()` - Normalized direction
- `HasLineOfSight(Vector3 target)` - Raycast check

---

## Available States

### IdleState
Enemy stands still, occasionally looks around. Transitions to Alert/Chase when player detected, or Patrol after idle duration.

### PatrolState
Enemy moves between waypoints or random positions within patrol radius. Supports configurable wait times at each waypoint.

### AlertState
Transitional state before chase. Enemy pauses, looks at player, can trigger warning animations. Creates tension before pursuit.

### ChaseState
Enemy pursues player at chase speed. Transitions to Attack when in range, or returns to Patrol if player escapes.

### AttackState
Enemy performs attacks with configurable windup and cooldown. Calls `enemy.PerformAttack()` which can be overridden.

### DeadState
Death sequence - disables colliders, plays animation, cleans up after delay.

---

## Enemy Types

### CrabEnemy
**Behavior:** Patrol → Alert → Chase → Melee Attack
**Special:** Uses A-B waypoints, snap claw animation

### ExplodingFishEnemy
**Behavior:** Idle → Chase → PrimedToExplode → Explode
**Special:** Color flash warning, area damage explosion

### SpikyFishEnemy
**Behavior:** Patrol → PuffedUp (shoot spines) → Patrol
**Special:** Inflates and shoots projectile spines, doesn't chase

### JellyfishEnemy
**Behavior:** Drift → Electric Shock Pulse
**Special:** Area damage when player nearby, passive movement

### AnglerFishEnemy
**Behavior:** Lurking → Ambush (lunge attack)
**Special:** Glowing lure, high-damage surprise attack

---

## Creating a New Enemy

### Step 1: Create Enemy Class

```csharp
public class SharkEnemy : EnemyBase
{
    [Header("Shark Settings")]
    [SerializeField] private float chargeSpeed = 15f;
    
    protected override EnemyState GetInitialState()
    {
        return new PatrolState(stateMachine, this);
    }
    
    public override void PerformAttack()
    {
        // Custom bite attack
        base.PerformAttack(); // Or fully custom
    }
}
```

### Step 2: Create Custom State (if needed)

```csharp
public class ChargeState : EnemyState
{
    public override string StateName => "Charge";
    
    private SharkEnemy shark;
    
    public ChargeState(EnemyStateMachine sm, EnemyBase enemy) 
        : base(sm, enemy)
    {
        shark = enemy as SharkEnemy;
    }
    
    public override void Enter()
    {
        base.Enter();
        // Wind up animation
    }
    
    public override void Execute()
    {
        // Move fast towards player
    }
    
    public override void CheckTransitions()
    {
        // If hit wall or reached target, go to recovery state
    }
}
```

### Step 3: Configure in Inspector

1. Add SharkEnemy script to prefab
2. Set up PlayerDetector (auto-added)
3. Configure detection/attack ranges via gizmos
4. Set up patrol waypoints if needed
5. Assign animator for animations

---

## Debug Features

Enable `debugStates` on any enemy to see state changes in console:
```
[SharkEnemy] Entering state: Patrol
[SharkEnemy] Entering state: Chase
[SharkEnemy] Attacked player for 5 damage
```

Gizmos show:
- **Yellow:** Detection radius
- **Orange:** Chase radius
- **Red:** Attack range
- **Cyan:** Patrol area
- **Green:** Patrol waypoints

---

## DDA Integration

All enemies respect global multipliers:
- `EnemyBase.GlobalSpeedMultiplier` - Movement speed
- `EnemyBase.GlobalDamageMultiplier` - Damage dealt

These are automatically adjusted by `DifficultyManager` based on player performance.
