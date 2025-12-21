using UnityEngine;

namespace GUP.Core
{
    /// <summary>
    /// Structured data for damage events. Contains all information needed
    /// for damage processing, knockback, and visual/audio feedback.
    /// </summary>
    [System.Serializable]
    public struct DamageData
    {
        /// <summary>Amount of damage to deal</summary>
        public float Amount;
        
        /// <summary>World position where the damage occurred</summary>
        public Vector3 HitPoint;
        
        /// <summary>Normal direction of the hit surface</summary>
        public Vector3 HitNormal;
        
        /// <summary>The GameObject that caused the damage (can be null)</summary>
        public GameObject Attacker;
        
        /// <summary>Type of damage for visual/audio feedback</summary>
        public DamageType Type;
        
        /// <summary>Knockback force to apply to the target</summary>
        public float KnockbackForce;

        /// <summary>
        /// Creates a new DamageData with the specified parameters.
        /// </summary>
        public DamageData(float amount, Vector3 hitPoint, Vector3 hitNormal, 
            GameObject attacker = null, DamageType type = DamageType.Contact, 
            float knockbackForce = 8f)
        {
            Amount = amount;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Attacker = attacker;
            Type = type;
            KnockbackForce = knockbackForce;
        }

        /// <summary>
        /// Creates a simple DamageData with minimal parameters (legacy compatibility).
        /// </summary>
        public static DamageData Simple(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            return new DamageData(amount, hitPoint, hitNormal);
        }
    }
}
