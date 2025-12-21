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
    }
}
