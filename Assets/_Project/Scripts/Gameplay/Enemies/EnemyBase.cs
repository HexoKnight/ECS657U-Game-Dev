using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

using GUP.Core;
using GUP.Core.Config;
using GUP.Core.Debug;

/// <summary>
/// Base class for all enemies. Integrates with state machine and health system.
/// Extend this class to create specific enemy types.
/// </summary>
[RequireComponent(typeof(EnemyStateMachine))]
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    #region Configuration
    
    [Header("Configuration")]
    [Tooltip("Optional: Enemy config ScriptableObject. If not assigned, uses local serialized values.")]
    [SerializeField] protected EnemyConfig enemyConfig;
    
    #endregion

    #region Health Settings
    
    [Header("Health")]
    [Tooltip("Maximum health of the enemy")]
    [SerializeField] protected float maxHealth = 10f;
    
    [Tooltip("Time invulnerable after taking damage")]
    [SerializeField] protected float invulnerabilityTime = 0.2f;
    
    protected float currentHealth;
    protected bool isInvulnerable;
    protected float invulnerabilityTimer;
    protected bool isDead;
    
    #endregion

    #region Movement Settings
    
    [Header("Movement")]
    [Tooltip("Base movement speed during patrol")]
    [SerializeField] protected float moveSpeed = 3f;
    
    [Tooltip("Speed when chasing the player")]
    [SerializeField] protected float chaseSpeed = 5f;
    
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] protected float rotationSpeed = 5f;
    
    #endregion

    #region Combat Settings
    
    [Header("Combat")]
    [Tooltip("Damage dealt to the player on attack")]
    [SerializeField] protected float attackDamage = 1f;
    
    [Tooltip("Knockback force applied to player")]
    [SerializeField] protected float knockbackForce = 10f;
    
    [Tooltip("Range at which enemy starts attacking")]
    [SerializeField] protected float attackRange = 2f;
    
    [Tooltip("Time between attacks")]
    [SerializeField] protected float attackCooldown = 1.5f;
    
    [Tooltip("Windup time before attack lands")]
    [SerializeField] protected float attackWindup = 0.3f;
    
    #endregion

    #region Detection Settings
    
    [Header("Detection")]
    [Tooltip("Range at which enemy detects the player")]
    [SerializeField] protected float detectionRadius = 8f;
    
    [Tooltip("Range at which enemy gives up chasing")]
    [SerializeField] protected float chaseRadius = 12f;
    
    [Tooltip("Time before enemy gives up when player is out of sight")]
    [SerializeField] protected float losePlayerTime = 3f;
    
    #endregion

    #region Patrol Settings
    
    [Header("Patrol")]
    [Tooltip("Enable patrol behavior")]
    [SerializeField] protected bool usePatrol = true;
    
    [Tooltip("Waypoints for patrol (optional)")]
    [SerializeField] protected List<Transform> patrolWaypoints;
    
    [Tooltip("Patrol radius if no waypoints set")]
    [SerializeField] protected float patrolRadius = 5f;
    
    [Tooltip("Wait time at each waypoint")]
    [SerializeField] protected float patrolWaitTime = 1f;
    
    [Tooltip("Distance to consider waypoint reached")]
    [SerializeField] protected float waypointTolerance = 0.5f;
    
    #endregion

    #region State Machine Settings
    
    [Header("State Machine")]
    [Tooltip("Use alert state before chasing")]
    [SerializeField] protected bool useAlertState = true;
    
    [Tooltip("Duration of alert state")]
    [SerializeField] protected float alertDuration = 0.5f;
    
    [Tooltip("Duration of idle before patrolling")]
    [SerializeField] protected float idleDuration = 2f;
    
    [Tooltip("Time to wait after death before destroying")]
    [SerializeField] protected float deathDuration = 2f;
    
    [Tooltip("Enable debug logging for state changes")]
    [SerializeField] protected bool debugStates = false;
    
    #endregion

    #region Events
    
    [Header("Events")]
    public UnityEvent OnDeathEvent;
    public UnityEvent OnHitEvent;
    public UnityEvent OnAttackEvent;
    
    #endregion

    #region DDA Multipliers (Static)
    
    /// <summary>Global speed multiplier for DDA system</summary>
    public static float GlobalSpeedMultiplier = 1f;
    
    /// <summary>Global damage multiplier for DDA system</summary>
    public static float GlobalDamageMultiplier = 1f;
    
    #endregion

    #region Component References
    
    protected EnemyStateMachine stateMachine;
    protected PlayerDetector playerDetector;
    protected Animator animator;
    protected Vector3 patrolCenter;
    
    #endregion

    #region Properties (for State Access)
    
    // Movement - use config if assigned, otherwise fallback to serialized fields
    public float MoveSpeed => enemyConfig != null ? enemyConfig.moveSpeed : moveSpeed;
    public float ChaseSpeed => enemyConfig != null ? enemyConfig.chaseSpeed : chaseSpeed;
    public float RotationSpeed => enemyConfig != null ? enemyConfig.turnSpeed : rotationSpeed;
    
    // Combat - use config if assigned, otherwise fallback
    public float AttackDamage => enemyConfig != null ? enemyConfig.attackDamage : attackDamage;
    public float KnockbackForce => enemyConfig != null ? enemyConfig.knockbackForce : knockbackForce;
    public float AttackRange => enemyConfig != null ? enemyConfig.attackRange : attackRange;
    public float AttackCooldown => enemyConfig != null ? enemyConfig.attackCooldown : attackCooldown;
    public float AttackWindup => enemyConfig != null ? enemyConfig.attackWindup : attackWindup;
    
    // Detection - use config if assigned, otherwise fallback
    public float DetectionRadius => enemyConfig != null ? enemyConfig.detectionRange : detectionRadius;
    public float ChaseRadius => enemyConfig != null ? enemyConfig.loseTargetRange : chaseRadius;
    public float LosePlayerTime => enemyConfig != null ? enemyConfig.forgetTime : losePlayerTime;
    
    // Patrol - partially from config
    public bool UsePatrol => usePatrol;
    public List<Transform> PatrolWaypoints => patrolWaypoints;
    public float PatrolRadius => patrolRadius;
    public float PatrolWaitTime => enemyConfig != null ? enemyConfig.patrolWaitTime : patrolWaitTime;
    public float WaypointTolerance => enemyConfig != null ? enemyConfig.waypointTolerance : waypointTolerance;
    
    // State machine - use config if assigned, otherwise fallback
    public bool UseAlertState => useAlertState;
    public float AlertDuration => enemyConfig != null ? enemyConfig.alertDuration : alertDuration;
    public float IdleDuration => idleDuration;
    public float DeathDuration => enemyConfig != null ? enemyConfig.deathDuration : deathDuration;
    public bool DebugStates => debugStates;
    
    // References
    public Vector3 PatrolCenter => patrolCenter;
    public PlayerDetector PlayerDetector => playerDetector;
    /// <summary>Returns animator only if it has a valid RuntimeAnimatorController</summary>
    public Animator Animator => (animator != null && animator.runtimeAnimatorController != null) ? animator : null;
    public EnemyStateMachine StateMachine => stateMachine;
    
    // Health - use config if assigned, otherwise fallback
    protected float MaxHealth => enemyConfig != null ? enemyConfig.maxHealth : maxHealth;
    protected float InvulnerabilityTime => enemyConfig != null ? enemyConfig.invulnerabilityTime : invulnerabilityTime;
    
    #endregion

    #region Unity Lifecycle
    
    protected virtual void Awake()
    {
        // Cache components
        stateMachine = GetComponent<EnemyStateMachine>();
        playerDetector = GetComponent<PlayerDetector>();
        animator = GetComponent<Animator>();
        
        // Ensure state machine exists
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<EnemyStateMachine>();
        }
        
        // Ensure player detector exists
        if (playerDetector == null)
        {
            playerDetector = gameObject.AddComponent<PlayerDetector>();
        }
    }
    
    protected virtual void Start()
    {
        // Initialize health using config-aware property
        currentHealth = MaxHealth;
        isDead = false;
        
        // Store patrol center
        patrolCenter = transform.position;
        
        // Initialize state machine
        stateMachine.Initialize(this);
        
        // Start in initial state
        EnemyState initialState = GetInitialState();
        stateMachine.ChangeState(initialState);
        
        OnSpawn();
    }
    
    protected virtual void Update()
    {
        // Update invulnerability
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }
    
    #endregion

    #region State Machine
    
    /// <summary>
    /// Override to specify initial state. Default is IdleState.
    /// </summary>
    protected virtual EnemyState GetInitialState()
    {
        if (usePatrol)
        {
            return new PatrolState(stateMachine, this);
        }
        return new IdleState(stateMachine, this);
    }
    
    #endregion

    #region Combat
    
    /// <summary>
    /// Called by AttackState when attack should land.
    /// Override for custom attack behavior.
    /// </summary>
    public virtual void PerformAttack()
    {
        if (playerDetector == null || playerDetector.Player == null) return;
        
        float distance = playerDetector.GetDistanceToPlayer();
        if (distance > attackRange * 1.2f) return;
        
        IDamageable target = playerDetector.Player.GetComponent<IDamageable>();
        if (target == null) return;
        
        // Calculate damage with DDA multiplier
        float finalDamage = attackDamage * GlobalDamageMultiplier;
        
        Vector3 hitNormal = playerDetector.GetDirectionToPlayer();
        DamageData damage = new DamageData(
            finalDamage,
            transform.position,
            hitNormal,
            gameObject,
            DamageType.Melee,
            knockbackForce
        );
        
        target.TakeDamage(damage);
        OnAttackEvent?.Invoke();
        
        // Log attack via GupDebug
        GupDebug.LogEnemyAttack(name, "Player", finalDamage);
    }
    
    #endregion

    #region Lifecycle Events
    
    /// <summary>Called when enemy is spawned/initialized</summary>
    public virtual void OnSpawn()
    {
        // Override in derived classes
    }
    
    /// <summary>Called when enemy dies</summary>
    protected virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeathEvent?.Invoke();
        GameEvents.RaiseEntityDied(gameObject);
        
        // Transition to dead state
        stateMachine.ChangeState(new DeadState(stateMachine, this));
    }
    
    /// <summary>Called by DeadState to cleanup and destroy</summary>
    public virtual void CleanupAndDestroy()
    {
        Destroy(gameObject);
    }
    
    #endregion

    #region IDamageable Implementation
    
    /// <summary>
    /// Entity type for cross-assembly identification.
    /// </summary>
    public EntityType EntityType => EntityType.Enemy;
    
    public void TakeDamage(DamageData damage)
    {
        if (isDead || damage.Amount <= 0f || isInvulnerable) return;
        
        currentHealth -= damage.Amount;
        
        // Events
        OnHitEvent?.Invoke();
        stateMachine.OnTakeDamage(damage);
        
        // Invulnerability - use config-aware property
        if (InvulnerabilityTime > 0f)
        {
            isInvulnerable = true;
            invulnerabilityTimer = InvulnerabilityTime;
        }
        
        // Log damage via GupDebug
        GupDebug.LogDamageTaken(name, damage.Amount, currentHealth, damage.Source?.name);
        
        // Check for death
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }
    
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        TakeDamage(DamageData.Simple(amount, hitPoint, hitNormal));
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    
    #endregion

    #region Collision Handling
    
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Contact damage on collision with player
        if (collision.gameObject.CompareTag("Player"))
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null && !isDead)
            {
                Vector3 hitPoint = collision.contactCount > 0 ? 
                    collision.GetContact(0).point : transform.position;
                Vector3 hitNormal = (collision.transform.position - transform.position).normalized;
                
                float contactDamage = attackDamage * 0.5f * GlobalDamageMultiplier; // Contact damage is half of attack damage
                
                DamageData damage = new DamageData(
                    contactDamage,
                    hitPoint,
                    hitNormal,
                    gameObject,
                    DamageType.Contact,
                    knockbackForce * 0.5f
                );
                
                damageable.TakeDamage(damage);
            }
        }
    }
    
    #endregion

    #region Debug
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Chase radius
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Patrol radius
        if (usePatrol && (patrolWaypoints == null || patrolWaypoints.Count == 0))
        {
            Gizmos.color = Color.cyan;
            Vector3 center = Application.isPlaying ? patrolCenter : transform.position;
            Gizmos.DrawWireSphere(center, patrolRadius);
        }
        
        // Patrol waypoints
        if (patrolWaypoints != null && patrolWaypoints.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolWaypoints.Count; i++)
            {
                if (patrolWaypoints[i] == null) continue;
                
                Gizmos.DrawSphere(patrolWaypoints[i].position, 0.3f);
                
                // Draw lines between waypoints
                int nextIndex = (i + 1) % patrolWaypoints.Count;
                if (patrolWaypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[nextIndex].position);
                }
            }
        }
    }
    
    #endregion
}
