using UnityEngine;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IEnemy, IDamageable
{
    [Header("Stats")]
    [Tooltip("Maximum health of the enemy")]
    public float maxHealth = 10f;
    [Tooltip("Current health of the enemy")]
    [SerializeField] protected float currentHealth;

    [Header("Movement")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 3f;
    
    [Header("Combat")]
    [Tooltip("Damage dealt to the player on contact")]
    public float contactDamage = 1f;
    [Tooltip("Force applied to the player on contact")]
    public float knockbackForce = 10f;

    [Header("Events")]
    public UnityEvent OnDeathEvent;
    public UnityEvent OnHitEvent;

    // DDA Multipliers (static so Manager can tweak them globally)
    public static float GlobalSpeedMultiplier = 1f;
    public static float GlobalDamageMultiplier = 1f;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        OnSpawn();
    }

    public virtual void OnSpawn()
    {
        // Initialization logic
    }

    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        currentHealth -= amount;
        OnHitEvent?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    public virtual void OnDeath()
    {
        OnDeathEvent?.Invoke();
        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Logic to damage player will go here
            // PlayerController will need a TakeDamage method
            Debug.Log($"{name} hit Player!");
        }
    }
}
