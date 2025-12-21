# Sistema de Vida y Daño en Unity - FPS 3D Platformer Subacuático
## Investigación Académica + Prácticas de Industria

---

## 1. VISIÓN GENERAL DE ARQUITECTURA

Tu código actual implementa correctamente la interfaz `IDamageable` (excelente diseño). Lo que necesitas es expandir esto con:

- **Separación de responsabilidades**: Health, Damage Dealing, Enemy AI, UI Feedback
- **Pattern: Strategy + State Machine** para enemigos
- **Event System** para comunicación entre sistemas
- **Modularidad**: Permite diferentes tipos de daño (contacto, proyectil, área, etc.)

---

## 2. PATRONES RECOMENDADOS POR INDUSTRIA

### 2.1 **Interface-Based Damage System** (Recomendado: Reddit r/gamedev + Foros)

**Patrón Principal:**
```csharp
// Contrato único para todo lo dañable
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsDead();
}

// Implementado por: Player, Enemies, Destructibles, Environmental
```

**Ventajas:**
- ✅ Desacoplado: Atacantes no necesitan saber qué tipo es el objetivo
- ✅ Escalable: Agregar nuevos enemigos es trivial
- ✅ DRY (Don't Repeat Yourself): Una sola lógica de daño

### 2.2 **State Machine para Enemigos** (Académico: Game Programming Patterns, GDC Talks)

**Tres componentes:**
1. **Enum de Estados**: Idle → Patrol → Chase → Attack → Dead
2. **Base State Class**: Define interfaz común
3. **Concrete States**: Cada comportamiento especifico

```csharp
public abstract class EnemyState
{
    protected Enemy enemy;
    
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract void CheckTransitions();
}

public class IdleState : EnemyState { ... }
public class ChaseState : EnemyState { ... }
public class AttackState : EnemyState { ... }
```

**Por qué funciona para 3D Platformer Subacuático:**
- Enemies complejos: Crab (agresivo), Spiky Fish (evasivo), Exploding Fish (kamikaze)
- Cada uno puede tener comportamientos completamente diferentes
- Transiciones claras entre estados

---

## 3. ARQUITECTURA RECOMENDADA DE SCRIPTS

```
Assets/
├─ Scripts/
│  ├─ Core/
│  │  ├─ IDamageable.cs           # Interfaz base
│  │  ├─ DamageData.cs            # Estructura de datos
│  │  └─ HealthComponent.cs       # Lógica de salud reutilizable
│  │
│  ├─ Player/
│  │  ├─ PlayerController.cs      # Ya tienes (modificar ligeramente)
│  │  └─ PlayerHealthUI.cs        # UI feedback (nuevo)
│  │
│  ├─ Enemies/
│  │  ├─ BaseEnemy/
│  │  │  ├─ Enemy.cs              # Clase base con State Machine
│  │  │  └─ EnemyState.cs         # Base class de estados
│  │  │
│  │  ├─ States/
│  │  │  ├─ IdleState.cs
│  │  │  ├─ PatrolState.cs
│  │  │  ├─ ChaseState.cs
│  │  │  ├─ AttackState.cs
│  │  │  └─ DieState.cs
│  │  │
│  │  ├─ Enemies/
│  │  │  ├─ CrabEnemy.cs          # Comportamiento específico
│  │  │  ├─ SpikyFishEnemy.cs
│  │  │  └─ ExplodingFishEnemy.cs
│  │  │
│  │  └─ Sensors/
│  │     ├─ PlayerDetector.cs     # Rango de detección
│  │     └─ LineOfSightChecker.cs # LOS raycast
│  │
│  ├─ Damage/
│  │  ├─ DamageDealer.cs          # Aplica daño (genérico)
│  │  ├─ MeleeDamage.cs           # Melee attack
│  │  ├─ ProjectileDamage.cs      # Proyectiles
│  │  └─ AreaDamage.cs            # AOE (Exploding Fish)
│  │
│  ├─ Hazards/
│  │  └─ StickyTrashHazard.cs     # Ya existe
│  │
│  └─ Systems/
│     ├─ EventManager.cs          # Comunicación entre sistemas
│     ├─ Checkpoint.cs            # Ya existe
│     └─ CheckpointManager.cs     # Ya existe
```

---

## 4. CÓDIGO BASE - IMPLEMENTACIÓN PASO A PASO

### 4.1 **IDamageable.cs** (YA TIENES)

```csharp
public interface IDamageable
{
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsDead();
    
    // Opcional pero útil
    void Heal(float amount);
}
```

### 4.2 **DamageData.cs** (NUEVO - Datos estructurados)

```csharp
public struct DamageData
{
    public float amount;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
    public GameObject attacker;
    public DamageType damageType;
    public float knockbackForce;
}

public enum DamageType
{
    Melee,
    Projectile,
    Environmental,
    Explosion,
    Contact
}
```

### 4.3 **HealthComponent.cs** (NUEVO - REUTILIZABLE)

```csharp
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    private float currentHealth;
    private bool isInvulnerable;
    private float invulnerabilityTimer;
    
    // Events para UI, sounds, particles
    public event System.Action<float> OnHealthChanged;
    public event System.Action OnDeath;
    public event System.Action<Vector3> OnTakeDamage;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    private void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }
    }
    
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (amount <= 0f || isInvulnerable)
            return;
        
        currentHealth -= amount;
        OnTakeDamage?.Invoke(hitPoint);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
        
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityTime;
        
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }
    
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => currentHealth <= 0f;
    
    protected virtual void Die()
    {
        OnDeath?.Invoke();
    }
}
```

### 4.4 **Enemy.cs** (NUEVA - Base con State Machine)

```csharp
using UnityEngine;

public class Enemy : HealthComponent
{
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float moveSpeed = 3f;
    
    protected EnemyState currentState;
    protected Transform playerTransform;
    protected bool playerDetected;
    protected Animator animator;
    
    // Estados disponibles
    protected IdleState idleState;
    protected ChaseState chaseState;
    protected AttackState attackState;
    protected DieState dieState;
    
    protected override void Die()
    {
        SwitchState(dieState);
        base.Die();
    }
    
    public virtual void SwitchState(EnemyState newState)
    {
        if (currentState != null)
            currentState.Exit();
        
        currentState = newState;
        currentState.Enter();
    }
    
    protected virtual void Update()
    {
        if (!IsDead() && currentState != null)
        {
            currentState.CheckTransitions();
            currentState.Update();
        }
    }
    
    public Transform GetPlayer() => playerTransform;
    public bool IsPlayerDetected() => playerDetected;
    public float GetDetectionRange() => detectionRange;
    public float GetAttackRange() => attackRange;
    public float GetMoveSpeed() => moveSpeed;
}
```

### 4.5 **EnemyState.cs** (NUEVA - Base abstracta)

```csharp
using UnityEngine;

public abstract class EnemyState
{
    protected Enemy enemy;
    protected Animator animator;
    
    public EnemyState(Enemy enemy, Animator animator)
    {
        this.enemy = enemy;
        this.animator = animator;
    }
    
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
    public abstract void CheckTransitions();
}
```

### 4.6 **IdleState.cs** (NUEVA - Ejemplo de estado)

```csharp
using UnityEngine;

public class IdleState : EnemyState
{
    private float idleTimer;
    private float idleDuration = 2f;
    
    public IdleState(Enemy enemy, Animator animator) 
        : base(enemy, animator) { }
    
    public override void Enter()
    {
        animator?.SetBool("IsWalking", false);
        idleTimer = 0f;
    }
    
    public override void Update()
    {
        idleTimer += Time.deltaTime;
    }
    
    public override void Exit() { }
    
    public override void CheckTransitions()
    {
        // Si detecta player → Chase
        if (enemy.IsPlayerDetected())
        {
            enemy.SwitchState(new ChaseState(enemy, animator));
            return;
        }
        
        // Si timeout → Patrol (opcional)
        if (idleTimer > idleDuration)
        {
            // enemy.SwitchState(patrolState);
        }
    }
}
```

### 4.7 **ChaseState.cs** (NUEVA)

```csharp
using UnityEngine;

public class ChaseState : EnemyState
{
    private NavMeshAgent navMeshAgent;
    
    public ChaseState(Enemy enemy, Animator animator) 
        : base(enemy, animator)
    {
        navMeshAgent = enemy.GetComponent<NavMeshAgent>();
    }
    
    public override void Enter()
    {
        animator?.SetBool("IsWalking", true);
        if (navMeshAgent) navMeshAgent.enabled = true;
    }
    
    public override void Update()
    {
        Transform player = enemy.GetPlayer();
        if (player && navMeshAgent)
        {
            float distanceToPlayer = Vector3.Distance(
                enemy.transform.position, 
                player.position
            );
            
            navMeshAgent.SetDestination(player.position);
            
            // Transición a Attack si está lo suficientemente cerca
            if (distanceToPlayer <= enemy.GetAttackRange())
            {
                enemy.SwitchState(new AttackState(enemy, animator));
            }
        }
    }
    
    public override void Exit()
    {
        if (navMeshAgent) navMeshAgent.enabled = false;
    }
    
    public override void CheckTransitions()
    {
        // Si pierde al player → Idle
        if (!enemy.IsPlayerDetected())
        {
            enemy.SwitchState(new IdleState(enemy, animator));
        }
    }
}
```

### 4.8 **CrabEnemy.cs** (NUEVA - Implementación específica)

```csharp
using UnityEngine;
using UnityEngine.AI;

public class CrabEnemy : Enemy
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private PlayerDetector playerDetector;
    private float lastAttackTime;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerDetector = GetComponent<PlayerDetector>();
        
        // Inicializar estados
        idleState = new IdleState(this, animator);
        chaseState = new ChaseState(this, animator);
        attackState = new AttackState(this, animator);
        dieState = new DieState(this, animator);
        
        currentState = idleState;
    }
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")
            ?.transform;
        currentState.Enter();
        
        // Suscribirse a eventos de salud
        OnHealthChanged += UpdateHealthUI;
        OnDeath += PlayDeathAnimation;
    }
    
    private new void Update()
    {
        // Detectar player usando raycast/distancia
        if (playerDetector != null)
        {
            playerDetected = playerDetector.IsPlayerInRange(detectionRange);
        }
        
        base.Update();
    }
    
    public void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown && 
            playerTransform != null)
        {
            IDamageable targetHealth = 
                playerTransform.GetComponent<IDamageable>();
            
            if (targetHealth != null)
            {
                Vector3 hitNormal = (playerTransform.position - 
                    transform.position).normalized;
                
                targetHealth.TakeDamage(
                    attackDamage,
                    transform.position,
                    hitNormal
                );
                
                lastAttackTime = Time.time;
            }
        }
    }
    
    private void UpdateHealthUI(float healthPercent)
    {
        // Actualizar barra de vida visual si existe
    }
    
    private void PlayDeathAnimation()
    {
        animator?.SetTrigger("Die");
        Destroy(gameObject, 2f); // Después de animación
    }
}
```

### 4.9 **AttackState.cs** (NUEVA)

```csharp
using UnityEngine;

public class AttackState : EnemyState
{
    private CrabEnemy crabEnemy;
    private float attackCooldown = 1.5f;
    private float timeSinceLastAttack;
    
    public AttackState(Enemy enemy, Animator animator) 
        : base(enemy, animator)
    {
        crabEnemy = enemy as CrabEnemy;
    }
    
    public override void Enter()
    {
        animator?.SetBool("IsAttacking", true);
        timeSinceLastAttack = 0f;
    }
    
    public override void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        
        if (timeSinceLastAttack >= attackCooldown)
        {
            crabEnemy?.Attack();
            timeSinceLastAttack = 0f;
        }
        
        // Mirar al player mientras ataca
        Transform player = enemy.GetPlayer();
        if (player)
        {
            Vector3 lookDir = (player.position - enemy.transform.position)
                .normalized;
            enemy.transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }
    
    public override void Exit()
    {
        animator?.SetBool("IsAttacking", false);
    }
    
    public override void CheckTransitions()
    {
        float distToPlayer = Vector3.Distance(
            enemy.transform.position,
            enemy.GetPlayer().position
        );
        
        // Si sale del rango → Chase
        if (distToPlayer > enemy.GetAttackRange() * 1.5f)
        {
            enemy.SwitchState(new ChaseState(enemy, animator));
        }
    }
}
```

### 4.10 **DieState.cs** (NUEVA)

```csharp
using UnityEngine;

public class DieState : EnemyState
{
    public DieState(Enemy enemy, Animator animator) 
        : base(enemy, animator) { }
    
    public override void Enter()
    {
        animator?.SetTrigger("Die");
    }
    
    public override void Update() { }
    
    public override void Exit() { }
    
    public override void CheckTransitions() { }
}
```

### 4.11 **PlayerDetector.cs** (NUEVO - Sensor)

```csharp
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    
    public bool IsPlayerInRange(float range)
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position, 
            range, 
            playerLayer
        );
        
        return colliders.Length > 0;
    }
}
```

---

## 5. CÓMO INTEGRAR CON TU CÓDIGO ACTUAL

### 5.1 **Modifica PlayerController.cs**

```csharp
// Reemplaza tu sección de Health por esto:

public class PlayerController : MonoBehaviour, ICharacterController, IDamageable
{
    private HealthComponent healthComponent;
    
    private void Awake()
    {
        // ... código existente ...
        
        // Obtén el componente Health
        healthComponent = GetComponent<HealthComponent>();
        
        if (healthComponent == null)
        {
            healthComponent = gameObject.AddComponent<HealthComponent>();
        }
    }
    
    // Implementa IDamageable
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        healthComponent.TakeDamage(amount, hitPoint, hitNormal);
        
        // Knockback integrado
        Vector3 knockbackDir = (transform.position - hitPoint).normalized;
        if (knockbackDir.sqrMagnitude < 0.01f)
            knockbackDir = -hitNormal.normalized;
        
        AddVelocity(knockbackDir * 8f);
    }
    
    // ... resto del código ...
}
```

### 5.2 **Setup en Editor**

1. Agrega `HealthComponent` a tu Player GameObject
2. En cada Enemy prefab:
   - Agrega `PlayerDetector` script
   - Agrega `NavMeshAgent` component
   - Agrega `Animator` component (si no lo tiene)
   - Reemplaza script del Enemy heredando de `Enemy`

---

## 6. TIPOS DE DAÑO - ESCALABILIDAD

```csharp
// DamageDealer.cs - Sistema genérico
public class DamageDealer : MonoBehaviour
{
    [SerializeField] private DamageData damageData;
    
    public void DealDamage(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(
                damageData.amount,
                transform.position,
                transform.forward
            );
        }
    }
}

// Detecta diferentes enemigos automáticamente
```

**Para cada tipo de daño:**
- **Melee**: OnCollisionStay, radius check
- **Projectile**: Raycast, OnCollisionEnter
- **Area/Explosion**: Physics.OverlapSphere
- **Environmental**: Trigger zones

---

## 7. PRÁCTICAS ACADÉMICAS - REFERENCIAS

### 7.1 **Design Patterns Utilizados**

| Patrón | Aplicación | Referencia |
|--------|-----------|-----------|
| **Strategy Pattern** | Diferentes comportamientos de daño | GoF Design Patterns |
| **State Pattern** | Máquina de estados de enemigos | GoF + Game Programming Patterns (R. Nystrom) |
| **Observer Pattern** | Events de health/damage | C# Events |
| **Component Pattern** | HealthComponent reutilizable | ECS architecture |
| **Factory Pattern** | Spawning de enemigos | Standard game arch. |

### 7.2 **SOLID Principles**

- **S**ingle Responsibility: HealthComponent solo maneja salud
- **O**pen/Closed: Agregar nuevos enemigos sin modificar base
- **L**iskov Substitution: Todos los enemigos usan Enemy base
- **I**nterface Segregation: IDamageable es minimalist
- **D**ependency Inversion: Dependen de interfaces, no de clases concretas

### 7.3 **Fuentes Académicas**

```
[1] Nystrom, R. (2014). Game Programming Patterns. 
    Genever Benning. (Cap: State Machine, Observer)

[2] Bilas, S. (2002). "A Data-Driven Game Object System". 
    GDC Proceedings.

[3] Gregory, J. (2018). Game Engine Architecture (3rd ed).
    CRC Press. (Cap: Game Objects, Scripting)

[4] UFRGS Paper: "Behavioral AI for Enemy Design" (2023)
    - Hierarchical State Machines for complex behaviors
```

---

## 8. RECOMENDACIONES DE REDDIT/FOROS

### 8.1 **Errores Comunes a Evitar**

❌ **No hagas esto:**
```csharp
// Malo: Acoplamiento fuerte
if (hitObject is CrabEnemy)
    ((CrabEnemy)hitObject).TakeCrabDamage(amount);
else if (hitObject is FishEnemy)
    ((FishEnemy)hitObject).TakeFishDamage(amount);
```

✅ **Haz esto:**
```csharp
// Bueno: Desacoplado con interfaz
IDamageable damageable = hitObject.GetComponent<IDamageable>();
if (damageable != null)
    damageable.TakeDamage(amount, hitPoint, hitNormal);
```

### 8.2 **Tips de Rendimiento**

1. **Cache de componentes** en Awake, no Update
2. **Object Pooling** para enemigos frecuentes
3. **Octree/Spatial Partitioning** para búsqueda de enemigos
4. **NavMesh Baking** para niveles subacuáticos
5. **Events** en lugar de Update() polling

### 8.3 **Detalles del Juego Subacuático**

```csharp
// Consideraciones para 3D platformer subacuático:

// Flotabilidad afecta movimiento
public class WaterPhysics : MonoBehaviour
{
    [SerializeField] private float buoyancy = 0.5f;
    // Reduce gravedad efectiva
}

// Velocidad de nado variable
public class SwimmingEnemy : Enemy
{
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float boostMultiplier = 1.5f;
}

// Visibilidad reducida = Rango de detección menor
// Sonido viaja diferente en agua = Ajusta detección por sonido
```

---

## 9. CHECKLIST DE IMPLEMENTACIÓN

- [ ] Crear `IDamageable` interface
- [ ] Crear `HealthComponent` reutilizable
- [ ] Crear `DamageData` struct
- [ ] Crear clase base `Enemy` con State Machine
- [ ] Crear `EnemyState` base abstracta
- [ ] Implementar 4-5 estados (Idle, Patrol, Chase, Attack, Die)
- [ ] Crear `CrabEnemy`, `SpikyFishEnemy`, `ExplodingFishEnemy`
- [ ] Agregar `PlayerDetector` sensor
- [ ] Integrar con `PlayerController` existente
- [ ] Agregar UI de salud
- [ ] Agregar visual feedback (color flash, knockback)
- [ ] Playtesting y balance

---

## 10. EJEMPLO COMPLETO DE FLUJO

```
Player toma daño:
1. Enemy.Attack() → DealsDamage
2. Llama IDamageable.TakeDamage() en Player
3. HealthComponent.TakeDamage() reduce salud
4. Invoca evento OnHealthChanged
5. PlayerHealthUI escucha y actualiza barra
6. Aplica knockback automático
7. Si salud ≤ 0 → OnDeath → Respawn

Enemy toma daño:
1. Igual al anterior pero Enemy responde con estado
2. Si muere: SwitchState(DieState)
3. DieState reproduce animación
4. Se destruye después de 2s
```

---

## 11. RECURSOS EXTERNOS RECOMENDADOS

- **Video: LlamAcademy** - "Enemy State Machine AI" (YouTube)
- **Blog: GameDevGenius** - "IDamageable Interface in Unity"
- **Documentación: Unity** - Interfaces Tutorial
- **Repositorio: Game Programming Patterns** - github.com/gameprogrammingpatterns

---

## CONCLUSIÓN

Tu aproximación con `IDamageable` es excelente. Ahora:

1. **Expande** con State Machine para enemigos complejos
2. **Desacopla** todo mediante interfaces y events
3. **Modulariza** scripts en carpetas claras
4. **Documenta** comportamientos en cada Enemy
5. **Testea** balance de daño/salud

Esto es lo que recomienda:
- ✅ Industria AAA (Halo, Doom, Elden Ring)
- ✅ Game Engines (GDocs, GDC talks)
- ✅ Academia (Game Programming Patterns, Papers)
- ✅ Comunidad (Reddit r/gamedev, Unity forums)