using UnityEngine;

/// <summary>
/// Generic component that deals damage to IDamageable entities on contact.
/// Attach to projectiles, melee hitboxes, hazards, etc.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Damage Settings")]
    [Tooltip("Amount of damage to deal")]
    [SerializeField] private float damageAmount = 10f;
    
    [Tooltip("Type of damage for visual/audio feedback")]
    [SerializeField] private DamageType damageType = DamageType.Contact;
    
    [Tooltip("Knockback force applied to the target")]
    [SerializeField] private float knockbackForce = 8f;
    
    [Header("Targeting")]
    [Tooltip("Tags that can be damaged by this dealer")]
    [SerializeField] private string[] targetTags = { "Player" };
    
    [Tooltip("If true, deal damage on trigger enter. If false, use OnCollisionEnter")]
    [SerializeField] private bool useTrigger = true;
    
    [Header("Behavior")]
    [Tooltip("If true, destroy this GameObject after dealing damage")]
    [SerializeField] private bool destroyOnHit = false;
    
    [Tooltip("Cooldown between damage applications to the same target")]
    [SerializeField] private float damageCooldown = 0.5f;
    
    [Tooltip("If true, can damage the same target multiple times")]
    [SerializeField] private bool allowMultipleHits = true;
    
    #endregion

    #region Private State
    
    // Track cooldowns per target
    private readonly System.Collections.Generic.Dictionary<GameObject, float> cooldowns = new();
    
    #endregion

    #region Properties
    
    /// <summary>Current damage amount (can be modified by DDA)</summary>
    public float DamageAmount
    {
        get => damageAmount;
        set => damageAmount = value;
    }
    
    /// <summary>Current knockback force</summary>
    public float KnockbackForce
    {
        get => knockbackForce;
        set => knockbackForce = value;
    }
    
    #endregion

    #region Unity Callbacks
    
    private void Update()
    {
        // Update cooldowns
        var keysToRemove = new System.Collections.Generic.List<GameObject>();
        var keys = new System.Collections.Generic.List<GameObject>(cooldowns.Keys);
        
        foreach (var key in keys)
        {
            cooldowns[key] -= Time.deltaTime;
            if (cooldowns[key] <= 0f)
            {
                keysToRemove.Add(key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            cooldowns.Remove(key);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (useTrigger)
        {
            TryDealDamage(other.gameObject, other.ClosestPoint(transform.position));
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (useTrigger && allowMultipleHits)
        {
            TryDealDamage(other.gameObject, other.ClosestPoint(transform.position));
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!useTrigger)
        {
            Vector3 hitPoint = collision.contactCount > 0 ? 
                collision.GetContact(0).point : collision.transform.position;
            TryDealDamage(collision.gameObject, hitPoint);
        }
    }
    
    #endregion

    #region Public API
    
    /// <summary>
    /// Manually deal damage to a target. Useful for area damage or raycast hits.
    /// </summary>
    public bool DealDamageTo(GameObject target, Vector3 hitPoint)
    {
        return TryDealDamage(target, hitPoint);
    }
    
    /// <summary>
    /// Deal area damage to all IDamageable in radius.
    /// </summary>
    public void DealAreaDamage(Vector3 center, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        
        foreach (var hit in hits)
        {
            if (IsValidTarget(hit.gameObject))
            {
                Vector3 direction = (hit.transform.position - center).normalized;
                TryDealDamage(hit.gameObject, center + direction * 0.1f);
            }
        }
    }
    
    #endregion

    #region Private Methods
    
    private bool TryDealDamage(GameObject target, Vector3 hitPoint)
    {
        if (!IsValidTarget(target))
            return false;
        
        // Check cooldown
        if (cooldowns.ContainsKey(target))
            return false;
        
        // Find IDamageable (check self and parent)
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }
        
        if (damageable == null || damageable.IsDead())
            return false;
        
        // Calculate hit normal (direction from this to target)
        Vector3 hitNormal = (target.transform.position - transform.position).normalized;
        if (hitNormal.sqrMagnitude < 0.01f)
        {
            hitNormal = transform.forward;
        }
        
        // Use base damage amount (DDA multipliers should be applied at the Gameplay layer)
        float finalDamage = damageAmount;
        
        // Create damage data
        DamageData damage = new DamageData(
            finalDamage,
            hitPoint,
            hitNormal,
            gameObject,
            damageType,
            knockbackForce
        );
        
        // Deal damage
        damageable.TakeDamage(damage);
        
        // Set cooldown
        if (damageCooldown > 0f)
        {
            cooldowns[target] = damageCooldown;
        }
        
        // Destroy if configured
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
        
        return true;
    }
    
    private bool IsValidTarget(GameObject target)
    {
        if (target == null) return false;
        
        foreach (string tag in targetTags)
        {
            if (target.CompareTag(tag))
                return true;
        }
        
        return false;
    }
    
    #endregion

    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Draw damage range if this is an area dealer
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    #endregion
}
