using UnityEngine;

/// <summary>
/// Interface for all entities that can receive damage.
/// Implemented by: Player, Enemies, Destructible objects.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// The type of entity for event routing and cross-assembly identification.
    /// </summary>
    EntityType EntityType { get; }
    
    /// <summary>
    /// Apply damage using structured DamageData.
    /// Preferred method for new code.
    /// </summary>
    void TakeDamage(DamageData damage);
    
    /// <summary>
    /// Apply damage with basic parameters.
    /// Kept for backward compatibility with existing code.
    /// </summary>
    void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal);
    
    /// <summary>Returns current health value</summary>
    float GetCurrentHealth();
    
    /// <summary>Returns maximum health value</summary>
    float GetMaxHealth();
    
    /// <summary>Returns true if entity is dead (health <= 0)</summary>
    bool IsDead();
    
    /// <summary>Restore health by the specified amount</summary>
    void Heal(float amount);
}
