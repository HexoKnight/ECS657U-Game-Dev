using UnityEngine;

/// <summary>
/// Static event system for decoupled communication between game systems.
/// Allows any system to broadcast and listen to game-wide events without direct references.
/// </summary>
public static class GameEvents
{
    #region Entity Events
    
    /// <summary>Raised when any entity takes damage</summary>
    public static event System.Action<GameObject, DamageData> OnEntityDamaged;
    
    /// <summary>Raised when any entity dies</summary>
    public static event System.Action<GameObject> OnEntityDied;
    
    /// <summary>Raised when player specifically takes damage</summary>
    public static event System.Action<DamageData> OnPlayerDamaged;
    
    /// <summary>Raised when player dies</summary>
    public static event System.Action OnPlayerDied;
    
    /// <summary>Raised when an enemy is killed</summary>
    public static event System.Action<GameObject> OnEnemyKilled;
    
    #endregion

    #region Checkpoint Events
    
    /// <summary>Raised when a checkpoint is activated</summary>
    public static event System.Action<GameObject> OnCheckpointActivated;
    
    /// <summary>Raised when player respawns</summary>
    public static event System.Action<Vector3> OnPlayerRespawned;
    
    #endregion

    #region Difficulty Events (for DDA)
    
    /// <summary>Raised when difficulty level changes</summary>
    public static event System.Action<float> OnDifficultyChanged;
    
    #endregion

    #region Raise Methods
    
    public static void RaiseEntityDamaged(GameObject entity, DamageData damage)
    {
        OnEntityDamaged?.Invoke(entity, damage);
        
        // Use component-based detection (safer than tags)
        if (entity.GetComponent<PlayerController>() != null)
        {
            OnPlayerDamaged?.Invoke(damage);
        }
    }
    
    public static void RaiseEntityDied(GameObject entity)
    {
        OnEntityDied?.Invoke(entity);
        
        // Use component-based detection (safer than tags)
        // This avoids the "Tag not defined" error entirely
        if (entity.GetComponent<PlayerController>() != null)
        {
            OnPlayerDied?.Invoke();
        }
        else if (entity.GetComponent<EnemyBase>() != null)
        {
            OnEnemyKilled?.Invoke(entity);
        }
    }
    
    public static void RaiseCheckpointActivated(GameObject checkpoint)
    {
        OnCheckpointActivated?.Invoke(checkpoint);
    }
    
    public static void RaisePlayerRespawned(Vector3 position)
    {
        OnPlayerRespawned?.Invoke(position);
    }
    
    public static void RaiseDifficultyChanged(float newDifficulty)
    {
        OnDifficultyChanged?.Invoke(newDifficulty);
    }
    
    #endregion

    #region Cleanup
    
    /// <summary>
    /// Clears all event subscriptions. Call when changing scenes if needed.
    /// </summary>
    public static void ClearAllListeners()
    {
        OnEntityDamaged = null;
        OnEntityDied = null;
        OnPlayerDamaged = null;
        OnPlayerDied = null;
        OnEnemyKilled = null;
        OnCheckpointActivated = null;
        OnPlayerRespawned = null;
        OnDifficultyChanged = null;
    }
    
    #endregion
}
