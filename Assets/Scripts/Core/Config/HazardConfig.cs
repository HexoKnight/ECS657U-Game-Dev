using UnityEngine;

namespace GUP.Core.Config
{
    /// <summary>
    /// Configuration asset for hazard parameters.
    /// Create instances via Assets > Create > GUP/Config/Hazard.
    /// </summary>
    [CreateAssetMenu(fileName = "HazardConfig", menuName = "GUP/Config/Hazard")]
    public class HazardConfig : ScriptableObject
    {
        [Header("Damage")]
        [Tooltip("Damage dealt per hit/tick")]
        public float damage = 10f;
        
        [Tooltip("Type of damage for feedback systems")]
        public DamageType damageType = DamageType.Environmental;
        
        [Tooltip("Knockback force applied")]
        public float knockbackForce = 5f;
        
        [Header("Timing")]
        [Tooltip("Cooldown between damage applications")]
        public float damageCooldown = 1f;
        
        [Tooltip("If true, deals damage continuously while in contact")]
        public bool continuousDamage = false;
        
        [Header("Push Effect (BubbleStream)")]
        [Tooltip("Upward force applied to player")]
        public float pushForce = 15f;
        
        [Tooltip("How quickly the force builds up")]
        public float forceRampSpeed = 3f;
        
        [Tooltip("Maximum additional velocity boost")]
        public float maxVelocityBoost = 10f;
        
        [Header("Slow Effect (StickyTrash)")]
        [Tooltip("How much to slow the player (0-1, where 1 is stopped)")]
        [Range(0f, 0.9f)]
        public float slowFactor = 0.5f;
        
        [Tooltip("How quickly the slow effect applies")]
        public float slowTransitionSpeed = 3f;
        
        [Tooltip("Enable vision impairment effect")]
        public bool impairVision = true;
    }
}
