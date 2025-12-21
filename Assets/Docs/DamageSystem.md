# Damage System Documentation

## Overview

The Damage System provides a unified, modular architecture for handling health and damage across all entities in the game. It follows **SOLID principles** and the **Strategy Pattern** to ensure scalability and maintainability.

## Core Components

### 1. IDamageable Interface
**Location:** `Assets/Scripts/Core/IDamageable.cs`

Interface implemented by all entities that can receive damage.

```csharp
public interface IDamageable
{
    void TakeDamage(DamageData damage);
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsDead();
    void Heal(float amount);
}
```

**Implemented by:** PlayerController, EnemyBase (and all enemies)

---

### 2. DamageData Struct
**Location:** `Assets/Scripts/Core/DamageData.cs`

Structured data for damage events containing all necessary information.

| Field | Type | Description |
|-------|------|-------------|
| Amount | float | Damage value |
| HitPoint | Vector3 | World position of impact |
| HitNormal | Vector3 | Direction of impact |
| Attacker | GameObject | Source of damage |
| Type | DamageType | Category of damage |
| KnockbackForce | float | Pushback force |

---

### 3. DamageType Enum
**Location:** `Assets/Scripts/Core/DamageType.cs`

```csharp
public enum DamageType
{
    Contact,      // Touching enemy
    Melee,        // Crab claws, bite attacks
    Projectile,   // Spines, bullets
    Explosion,    // Exploding fish
    Environmental,// Spikes, falling
    Electric      // Jellyfish
}
```

---

### 4. HealthComponent
**Location:** `Assets/Scripts/Core/HealthComponent.cs`

Reusable component for health management.

**Inspector Settings:**
- `maxHealth` - Maximum health value
- `invulnerabilityDuration` - I-frames after damage
- `initializeOnAwake` - Auto-initialize on start

**Events (UnityEvents for Inspector wiring):**
- `OnHealthChanged(float normalized)` - Health percentage changed
- `OnDamaged(DamageData)` - Damage received
- `OnDeath()` - Entity died
- `OnHealed(float amount)` - Health restored

**C# Events (for code subscriptions):**
- `HealthChanged`, `Damaged`, `Died`, `Healed`

---

### 5. DamageDealer
**Location:** `Assets/Scripts/Core/DamageDealer.cs`

Generic component for applying damage on contact.

**Inspector Settings:**
- `damageAmount` - Base damage
- `damageType` - Type of damage
- `knockbackForce` - Pushback strength
- `targetTags` - Valid targets (default: "Player")
- `useTrigger` - Use OnTriggerEnter vs OnCollisionEnter
- `destroyOnHit` - Self-destruct after hitting
- `damageCooldown` - Time between hits on same target

**Usage:**
1. Add to projectile prefab
2. Set damage values
3. Ensure collider is configured (trigger or collision)
4. Projectile will automatically damage valid targets

---

### 6. GameEvents
**Location:** `Assets/Scripts/Core/GameEvents.cs`

Static event system for decoupled communication.

**Available Events:**
```csharp
// Entity events
GameEvents.OnEntityDamaged += (GameObject entity, DamageData damage) => { };
GameEvents.OnEntityDied += (GameObject entity) => { };
GameEvents.OnPlayerDamaged += (DamageData damage) => { };
GameEvents.OnPlayerDied += () => { };
GameEvents.OnEnemyKilled += (GameObject enemy) => { };

// Game events
GameEvents.OnCheckpointActivated += (GameObject checkpoint) => { };
GameEvents.OnPlayerRespawned += (Vector3 position) => { };
GameEvents.OnDifficultyChanged += (float difficulty) => { };
```

---

## Usage Examples

### Making an Entity Damageable

```csharp
public class Barrel : MonoBehaviour, IDamageable
{
    private float health = 50f;
    
    public void TakeDamage(DamageData damage)
    {
        health -= damage.Amount;
        if (health <= 0) Explode();
    }
    
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        TakeDamage(DamageData.Simple(amount, hitPoint, hitNormal));
    }
    
    // ... other interface methods
}
```

### Dealing Damage from Code

```csharp
// Using DamageDealer component (recommended)
DamageDealer dealer = GetComponent<DamageDealer>();
dealer.DealDamageTo(targetObject, hitPoint);

// Or manually
IDamageable target = collision.GetComponent<IDamageable>();
if (target != null)
{
    DamageData damage = new DamageData(
        10f,                    // amount
        transform.position,     // hit point
        Vector3.forward,        // hit normal
        gameObject,             // attacker
        DamageType.Melee,       // type
        5f                      // knockback
    );
    target.TakeDamage(damage);
}
```

### Listening to Damage Events

```csharp
void OnEnable()
{
    GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
}

void OnDisable()
{
    GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
}

void HandlePlayerDamaged(DamageData damage)
{
    // Flash screen red, play sound, etc.
    Debug.Log($"Player took {damage.Amount} {damage.Type} damage!");
}
```

---

## Integration with Player

The `PlayerController` implements `IDamageable` and handles:
- Invulnerability frames
- Knockback via `AddVelocity()`
- Death/respawn via `CheckpointManager`

---

## DDA Integration

The `DifficultyManager` adjusts `EnemyBase.GlobalDamageMultiplier` based on player performance. All enemy damage is automatically scaled:

```csharp
float finalDamage = baseDamage * EnemyBase.GlobalDamageMultiplier;
```
