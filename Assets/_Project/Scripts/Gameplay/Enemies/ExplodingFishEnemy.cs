using UnityEngine;

using GUP.Core;
/// <summary>
/// Exploding fish enemy that chases player and detonates when close.
/// Uses state machine with custom PrimedToExplode state.
/// </summary>
public class ExplodingFishEnemy : EnemyBase
{
    #region Explosion Settings
    
    [Header("Explosion Settings")]
    [Tooltip("Radius of explosion detection and trigger")]
    [SerializeField] private float explosionTriggerRadius = 2f;
    
    [Tooltip("Radius of explosion damage")]
    [SerializeField] private float explosionDamageRadius = 5f;
    
    [Tooltip("Delay before explosion after being primed")]
    [SerializeField] private float explosionDelay = 1.5f;
    
    [Tooltip("Damage multiplier for explosion (base is attackDamage)")]
    [SerializeField] private float explosionDamageMultiplier = 5f;
    
    [Header("Explosion Visuals")]
    [Tooltip("Prefab to spawn on explosion")]
    [SerializeField] private GameObject explosionEffectPrefab;
    
    [Tooltip("Color to flash when primed")]
    [SerializeField] private Color warningColor = Color.red;
    
    [Tooltip("Flash speed when primed")]
    [SerializeField] private float flashSpeed = 10f;
    
    #endregion

    #region Private State
    
    private Renderer rend;
    private Color originalColor;
    private bool isPrimed = false;
    
    #endregion

    #region Properties
    
    public float ExplosionDelay => explosionDelay;
    public bool IsPrimed => isPrimed;
    
    #endregion

    #region Lifecycle
    
    protected override void Awake()
    {
        base.Awake();
        
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
        
        // Disable patrol for exploding fish - they just chase
        usePatrol = false;
    }
    
    protected override EnemyState GetInitialState()
    {
        // Start in idle, waiting for player
        return new IdleState(stateMachine, this);
    }
    
    #endregion

    #region State Machine Overrides
    
    protected override void Update()
    {
        base.Update();
        
        // Check for priming condition during chase
        if (!isPrimed && !isDead && playerDetector != null)
        {
            float distance = playerDetector.GetDistanceToPlayer();
            if (distance <= explosionTriggerRadius)
            {
                StartPriming();
            }
        }
    }
    
    private void StartPriming()
    {
        if (isPrimed) return;
        
        isPrimed = true;
        stateMachine.ChangeState(new PrimedToExplodeState(stateMachine, this));
    }
    
    #endregion

    #region Explosion
    
    /// <summary>
    /// Called by PrimedToExplodeState to start visual feedback.
    /// </summary>
    public void StartPrimedVisuals()
    {
        // Animation trigger if available
        animator?.SetBool("IsPrimed", true);
    }
    
    /// <summary>
    /// Called every frame during primed state.
    /// </summary>
    public void UpdatePrimedVisuals(float elapsedTime)
    {
        if (rend != null)
        {
            float t = Mathf.PingPong(elapsedTime * flashSpeed, 1f);
            rend.material.color = Color.Lerp(originalColor, warningColor, t);
        }
    }
    
    /// <summary>
    /// Execute the explosion - deals area damage and destroys self.
    /// </summary>
    public void Explode()
    {
        if (isDead) return;
        
        // Deal area damage
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionDamageRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null)
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float falloff = 1f - (distance / explosionDamageRadius);
                float damage = attackDamage * explosionDamageMultiplier * falloff * GlobalDamageMultiplier;
                
                Vector3 hitNormal = (hit.transform.position - transform.position).normalized;
                
                DamageData damageData = new DamageData(
                    damage,
                    transform.position,
                    hitNormal,
                    gameObject,
                    DamageType.Explosion,
                    knockbackForce * 2f // Extra knockback for explosion
                );
                
                damageable.TakeDamage(damageData);
            }
        }
        
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (debugStates)
        {
            Debug.Log($"[ExplodingFishEnemy] {name} exploded!");
        }
        
        // Destroy self (bypass death state since we're exploding)
        OnDeathEvent?.Invoke();
        GameEvents.RaiseEntityDied(gameObject);
        Destroy(gameObject);
    }
    
    #endregion

    #region Gizmos
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Explosion trigger radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionTriggerRadius);
        
        // Explosion damage radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, explosionDamageRadius);
    }
    
    #endregion
}
