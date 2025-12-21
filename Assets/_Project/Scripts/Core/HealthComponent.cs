using UnityEngine;
using UnityEngine.Events;

namespace GUP.Core
{
    /// <summary>
    /// Reusable health component that implements IDamageable.
    /// Can be attached to any GameObject that needs health management.
    /// Provides events for UI, audio, and visual feedback systems.
    /// </summary>
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        #region Inspector Fields
        
        [Header("Entity Settings")]
        [Tooltip("Type of entity for event routing")]
        [SerializeField] private EntityType entityType = EntityType.Unknown;
        
        [Header("Health Settings")]
        [Tooltip("Maximum health of this entity")]
        [SerializeField] private float maxHealth = 100f;
        
        [Tooltip("Time in seconds the entity is invulnerable after taking damage")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;
        
        [Tooltip("If true, entity starts with full health on Awake")]
        [SerializeField] private bool initializeOnAwake = true;

        [Header("Events")]
        [Tooltip("Invoked when health changes. Parameter: normalized health (0-1)")]
        public UnityEvent<float> OnHealthChanged;
        
        [Tooltip("Invoked when entity takes damage. Parameter: DamageData")]
        public UnityEvent<DamageData> OnDamaged;
        
        [Tooltip("Invoked when entity dies")]
        public UnityEvent OnDeath;
        
        [Tooltip("Invoked when entity is healed. Parameter: heal amount")]
        public UnityEvent<float> OnHealed;
        
        #endregion

        #region C# Events (for code subscriptions)
        
        /// <summary>Invoked when health changes. Parameter: normalized health (0-1)</summary>
        public event System.Action<float> HealthChanged;
        
        /// <summary>Invoked when damage is taken. Parameter: DamageData</summary>
        public event System.Action<DamageData> Damaged;
        
        /// <summary>Invoked when entity dies</summary>
        public event System.Action Died;
        
        /// <summary>Invoked when healed. Parameter: heal amount</summary>
        public event System.Action<float> Healed;
        
        #endregion

        #region Private State
        
        private float currentHealth;
        private bool isInvulnerable;
        private float invulnerabilityTimer;
        private bool isDead;
        
        #endregion

        #region Properties
        
        /// <summary>True if currently in invulnerability frames</summary>
        public bool IsInvulnerable => isInvulnerable;
        
        /// <summary>Normalized health value (0-1)</summary>
        public float HealthNormalized => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
        }
        
        private void Update()
        {
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

        #region Public API
        
        /// <summary>
        /// Initialize health to maximum. Call this when spawning/respawning.
        /// </summary>
        public void Initialize()
        {
            currentHealth = maxHealth;
            isDead = false;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
            
            OnHealthChanged?.Invoke(HealthNormalized);
            HealthChanged?.Invoke(HealthNormalized);
        }
        
        /// <summary>
        /// Set max health and optionally reset current health.
        /// Useful for difficulty adjustments.
        /// </summary>
        /// <param name="newMaxHealth">The new maximum health value.</param>
        /// <param name="resetCurrent">If true, resets current health to new max.</param>
        public void SetMaxHealth(float newMaxHealth, bool resetCurrent = false)
        {
            maxHealth = Mathf.Max(1f, newMaxHealth);
            if (resetCurrent)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            
            OnHealthChanged?.Invoke(HealthNormalized);
            HealthChanged?.Invoke(HealthNormalized);
        }
        
        /// <summary>
        /// Temporarily make invulnerable (useful for respawn protection).
        /// </summary>
        /// <param name="duration">Duration in seconds.</param>
        public void SetTemporaryInvulnerability(float duration)
        {
            isInvulnerable = true;
            invulnerabilityTimer = duration;
        }
        
        #endregion

        #region IDamageable Implementation
        
        /// <inheritdoc/>
        public EntityType EntityType => entityType;
        
        /// <inheritdoc/>
        public void TakeDamage(DamageData damage)
        {
            if (isDead || damage.Amount <= 0f || isInvulnerable)
                return;
            
            currentHealth -= damage.Amount;
            
            // Trigger events
            OnDamaged?.Invoke(damage);
            Damaged?.Invoke(damage);
            OnHealthChanged?.Invoke(HealthNormalized);
            HealthChanged?.Invoke(HealthNormalized);
            
            // Broadcast to GameEvents
            GameEvents.RaiseEntityDamaged(gameObject, damage);
            
            // Start invulnerability
            if (invulnerabilityDuration > 0f)
            {
                isInvulnerable = true;
                invulnerabilityTimer = invulnerabilityDuration;
            }
            
            // Check for death
            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                Die();
            }
        }
        
        /// <inheritdoc/>
        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            TakeDamage(DamageData.Simple(amount, hitPoint, hitNormal));
        }
        
        /// <inheritdoc/>
        public void Heal(float amount)
        {
            if (isDead || amount <= 0f)
                return;
            
            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            float actualHeal = currentHealth - previousHealth;
            
            if (actualHeal > 0f)
            {
                OnHealed?.Invoke(actualHeal);
                Healed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(HealthNormalized);
                HealthChanged?.Invoke(HealthNormalized);
            }
        }
        
        /// <inheritdoc/>
        public float GetCurrentHealth() => currentHealth;
        
        /// <inheritdoc/>
        public float GetMaxHealth() => maxHealth;
        
        /// <inheritdoc/>
        public bool IsDead() => isDead;
        
        #endregion

        #region Private Methods
        
        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            isInvulnerable = false;
            
            OnDeath?.Invoke();
            Died?.Invoke();
            
            // Broadcast to GameEvents
            GameEvents.RaiseEntityDied(gameObject);
        }
        
        #endregion
    }
}
