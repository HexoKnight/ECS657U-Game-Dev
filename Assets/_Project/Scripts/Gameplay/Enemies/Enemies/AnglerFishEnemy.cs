using UnityEngine;

/// <summary>
/// Anglerfish enemy that lurks in shadows and ambushes with its lure.
/// Uses a glowing lure to attract the player, then lunges for high damage.
/// </summary>
public class AnglerFishEnemy : EnemyBase
{
    #region Ambush Settings
    
    [Header("Ambush Attack")]
    [Tooltip("Range at which anglerfish will ambush")]
    [SerializeField] private float ambushRange = 5f;
    
    [Tooltip("Duration of the lunge attack")]
    [SerializeField] private float lungeDuration = 0.3f;
    
    [Tooltip("Damage multiplier for ambush (base is attackDamage)")]
    [SerializeField] private float ambushDamageMultiplier = 3f;
    
    [Tooltip("Cooldown between ambushes")]
    [SerializeField] private float ambushCooldown = 4f;
    
    [Header("Lure Settings")]
    [Tooltip("Transform for the lure object")]
    [SerializeField] private Transform lureTransform;
    
    [Tooltip("Light component on the lure")]
    [SerializeField] private Light lureLight;
    
    [Tooltip("Base intensity of lure light")]
    [SerializeField] private float lureIntensity = 2f;
    
    [Tooltip("Lure pulse speed")]
    [SerializeField] private float lurePulseSpeed = 2f;
    
    [Tooltip("Lure swing amplitude")]
    [SerializeField] private float lureSwingAmplitude = 15f;
    
    [Header("Visuals")]
    [Tooltip("Color during ambush")]
    [SerializeField] private Color ambushColor = Color.red;
    
    #endregion

    #region Private State
    
    private Renderer rend;
    private Color originalColor;
    private Vector3 originalLurePosition;
    private Quaternion originalLureRotation;
    
    #endregion

    #region Properties
    
    public float AmbushRange => ambushRange;
    public float LungeDuration => lungeDuration;
    public float AmbushCooldown => ambushCooldown;
    
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
        
        if (lureTransform != null)
        {
            originalLurePosition = lureTransform.localPosition;
            originalLureRotation = lureTransform.localRotation;
        }
        
        // Anglerfish doesn't patrol normally
        usePatrol = false;
        useAlertState = false;
    }
    
    protected override EnemyState GetInitialState()
    {
        return new LurkingState(stateMachine, this);
    }
    
    #endregion

    #region Lure Control
    
    public void EnableLure(bool enabled)
    {
        if (lureLight != null)
        {
            lureLight.enabled = enabled;
        }
    }
    
    public void UpdateLure(float time)
    {
        if (lureLight != null)
        {
            // Pulse the light
            float pulse = 1f + Mathf.Sin(time * lurePulseSpeed * Mathf.PI * 2f) * 0.3f;
            lureLight.intensity = lureIntensity * pulse;
        }
        
        if (lureTransform != null)
        {
            // Swing the lure
            float swing = Mathf.Sin(time * lurePulseSpeed * 0.5f * Mathf.PI * 2f) * lureSwingAmplitude;
            lureTransform.localRotation = originalLureRotation * Quaternion.Euler(swing, 0, 0);
        }
    }
    
    #endregion

    #region Ambush
    
    public void StartAmbushVisuals()
    {
        // Increase lure brightness dramatically
        if (lureLight != null)
        {
            lureLight.intensity = lureIntensity * 3f;
        }
        
        if (rend != null)
        {
            rend.material.color = ambushColor;
        }
    }
    
    public void EndAmbushVisuals()
    {
        if (lureLight != null)
        {
            lureLight.intensity = lureIntensity;
        }
        
        if (rend != null)
        {
            rend.material.color = originalColor;
        }
        
        if (lureTransform != null)
        {
            lureTransform.localPosition = originalLurePosition;
            lureTransform.localRotation = originalLureRotation;
        }
    }
    
    public void PerformBite()
    {
        // Deal high damage to any damageable in front
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, 2f);
        
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
                float damage = attackDamage * ambushDamageMultiplier * GlobalDamageMultiplier;
                Vector3 hitNormal = (hit.transform.position - transform.position).normalized;
                
                DamageData damageData = new DamageData(
                    damage,
                    transform.position,
                    hitNormal,
                    gameObject,
                    DamageType.Melee,
                    knockbackForce * 2f // Heavy knockback
                );
                
                damageable.TakeDamage(damageData);
                
                if (debugStates)
                {
                    Debug.Log($"[AnglerFishEnemy] {name} bit for {damage} damage!");
                }
            }
        }
        
        animator?.SetTrigger("Bite");
    }
    
    #endregion

    #region Overrides
    
    public override void PerformAttack()
    {
        // Anglerfish only attacks via ambush
        PerformBite();
    }
    
    #endregion

    #region Gizmos
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Ambush range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ambushRange);
        
        // Bite range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1f, 2f);
    }
    
    #endregion
}
