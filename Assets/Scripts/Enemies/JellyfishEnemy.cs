using UnityEngine;

/// <summary>
/// Jellyfish enemy that creates an electric shock zone around itself.
/// Drifts slowly and pulses damage when player is nearby.
/// </summary>
public class JellyfishEnemy : EnemyBase
{
    #region Electric Shock Settings
    
    [Header("Electric Shock")]
    [Tooltip("Radius of the electric shock zone")]
    [SerializeField] private float shockRadius = 4f;
    
    [Tooltip("Damage per shock pulse")]
    [SerializeField] private float shockDamage = 0.5f;
    
    [Tooltip("Time between shock pulses")]
    [SerializeField] private float shockInterval = 0.5f;
    
    [Tooltip("Duration of each shock (affects visuals)")]
    [SerializeField] private float shockDuration = 0.2f;
    
    [Header("Drift Movement")]
    [Tooltip("Drift speed (slow, ambient movement)")]
    [SerializeField] private float driftSpeed = 1f;
    
    [Tooltip("How often to change drift direction")]
    [SerializeField] private float driftChangeInterval = 3f;
    
    [Tooltip("Maximum drift distance from spawn point")]
    [SerializeField] private float driftRadius = 5f;
    
    [Header("Visuals")]
    [Tooltip("Color when shocking")]
    [SerializeField] private Color shockColor = new Color(0.5f, 0.5f, 1f, 1f);
    
    [Tooltip("Particle system for shock effect")]
    [SerializeField] private ParticleSystem shockParticles;
    
    #endregion

    #region Private State
    
    private Vector3 driftDirection;
    private float driftTimer;
    private float shockTimer;
    private float shockPulseTimer;
    private bool isShocking;
    private Renderer rend;
    private Color originalColor;
    private Vector3 spawnPosition;
    
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
        
        // Jellyfish doesn't chase or use normal states
        usePatrol = false;
        useAlertState = false;
    }
    
    protected override void Start()
    {
        base.Start();
        spawnPosition = transform.position;
        ChangeDirection();
    }
    
    protected override EnemyState GetInitialState()
    {
        // Jellyfish uses custom behavior, starts in idle (no state transitions)
        return new IdleState(stateMachine, this);
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (isDead) return;
        
        // Handle drifting
        UpdateDrift();
        
        // Handle electric shock
        UpdateShock();
    }
    
    #endregion

    #region Drift Behavior
    
    private void UpdateDrift()
    {
        driftTimer += Time.deltaTime;
        
        // Change direction periodically
        if (driftTimer >= driftChangeInterval)
        {
            ChangeDirection();
            driftTimer = 0f;
        }
        
        // Move in drift direction
        Vector3 nextPosition = transform.position + driftDirection * driftSpeed * Time.deltaTime;
        
        // Stay within drift radius
        float distanceFromSpawn = Vector3.Distance(nextPosition, spawnPosition);
        if (distanceFromSpawn > driftRadius)
        {
            // Turn back towards spawn
            driftDirection = (spawnPosition - transform.position).normalized;
        }
        
        transform.position = nextPosition;
        
        // Gentle bobbing motion
        float bobOffset = Mathf.Sin(Time.time * 2f) * 0.1f;
        transform.position += Vector3.up * bobOffset * Time.deltaTime;
    }
    
    private void ChangeDirection()
    {
        // Random horizontal direction with slight vertical variation
        driftDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.2f, 0.2f),
            Random.Range(-1f, 1f)
        ).normalized;
    }
    
    #endregion

    #region Electric Shock
    
    private void UpdateShock()
    {
        // Check if player is in shock radius
        if (playerDetector != null && playerDetector.IsPlayerInRange(shockRadius))
        {
            shockTimer += Time.deltaTime;
            
            // Pulse damage at interval
            if (shockTimer >= shockInterval)
            {
                PerformShock();
                shockTimer = 0f;
            }
        }
        else
        {
            shockTimer = 0f;
        }
        
        // Handle shock visual pulse
        if (isShocking)
        {
            shockPulseTimer += Time.deltaTime;
            if (shockPulseTimer >= shockDuration)
            {
                EndShockVisuals();
            }
        }
    }
    
    private void PerformShock()
    {
        StartShockVisuals();
        
        // Find all damageable entities in range
        Collider[] hits = Physics.OverlapSphere(transform.position, shockRadius);
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
                Vector3 hitNormal = (hit.transform.position - transform.position).normalized;
                
                DamageData damage = new DamageData(
                    shockDamage * GlobalDamageMultiplier,
                    transform.position,
                    hitNormal,
                    gameObject,
                    DamageType.Electric,
                    knockbackForce * 0.3f // Light knockback for electric
                );
                
                damageable.TakeDamage(damage);
            }
        }
        
        if (debugStates)
        {
            Debug.Log($"[JellyfishEnemy] {name} pulsed electric shock!");
        }
    }
    
    private void StartShockVisuals()
    {
        isShocking = true;
        shockPulseTimer = 0f;
        
        if (rend != null)
        {
            rend.material.color = shockColor;
        }
        
        if (shockParticles != null)
        {
            shockParticles.Play();
        }
        
        animator?.SetTrigger("Shock");
    }
    
    private void EndShockVisuals()
    {
        isShocking = false;
        
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
    }
    
    #endregion

    #region Overrides
    
    public override void PerformAttack()
    {
        // Jellyfish doesn't do direct attacks, only shock pulses
    }
    
    #endregion

    #region Gizmos
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Shock radius
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, shockRadius);
        
        // Drift radius
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, driftRadius);
    }
    
    #endregion
}
