using UnityEngine;

/// <summary>
/// Projectile spine fired by SpikyFishEnemy.
/// Deals damage on contact and destroys itself.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SpineProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Speed of the projectile")]
    [SerializeField] private float speed = 15f;
    
    [Tooltip("Damage dealt on hit")]
    [SerializeField] private float damage = 1f;
    
    [Tooltip("Knockback force on hit")]
    [SerializeField] private float knockback = 5f;
    
    [Tooltip("Time before auto-destroy")]
    [SerializeField] private float lifetime = 5f;
    
    [Tooltip("Tags that can be hit")]
    [SerializeField] private string[] hitTags = { "Player" };
    
    private Rigidbody rb;
    private GameObject owner;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }
    
    /// <summary>
    /// Initialize the projectile with direction and owner.
    /// </summary>
    public void Initialize(Vector3 direction, GameObject owner, float damage, float knockback)
    {
        this.owner = owner;
        this.damage = damage;
        this.knockback = knockback;
        
        rb.velocity = direction.normalized * speed;
        transform.rotation = Quaternion.LookRotation(direction);
        
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Don't hit owner
        if (other.gameObject == owner) return;
        if (other.transform.IsChildOf(owner.transform)) return;
        
        // Check if valid target
        bool isValidTarget = false;
        foreach (string tag in hitTags)
        {
            if (other.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }
        
        if (!isValidTarget)
        {
            // Hit environment, just destroy
            Destroy(gameObject);
            return;
        }
        
        // Deal damage
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
        }
        
        if (damageable != null)
        {
            Vector3 hitNormal = (other.transform.position - transform.position).normalized;
            
            DamageData damageData = new DamageData(
                damage * EnemyBase.GlobalDamageMultiplier,
                transform.position,
                hitNormal,
                owner,
                DamageType.Projectile,
                knockback
            );
            
            damageable.TakeDamage(damageData);
        }
        
        Destroy(gameObject);
    }
}
