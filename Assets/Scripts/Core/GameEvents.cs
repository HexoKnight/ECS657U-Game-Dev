using UnityEngine;

namespace GUP.Core
{
    /// <summary>
    /// Static event system for decoupled communication between game systems.
    /// Allows any system to broadcast and listen to game-wide events without direct references.
    /// </summary>
    /// <remarks>
    /// This legacy static event system will be replaced by ScriptableObject-based 
    /// event channels in Phase 2, Milestone 2.
    /// </remarks>
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
        
        /// <summary>
        /// Raises an entity damaged event with automatic player detection.
        /// </summary>
        public static void RaiseEntityDamaged(GameObject entity, DamageData damage)
        {
            OnEntityDamaged?.Invoke(entity, damage);
            
            // Use interface-based detection for cross-assembly safety
            var damageable = entity.GetComponent<IDamageable>();
            if (damageable != null && damageable.EntityType == EntityType.Player)
            {
                OnPlayerDamaged?.Invoke(damage);
            }
        }
        
        /// <summary>
        /// Raises an entity died event with automatic player/enemy detection.
        /// </summary>
        public static void RaiseEntityDied(GameObject entity)
        {
            OnEntityDied?.Invoke(entity);
            
            var damageable = entity.GetComponent<IDamageable>();
            if (damageable != null)
            {
                if (damageable.EntityType == EntityType.Player)
                {
                    OnPlayerDied?.Invoke();
                }
                else if (damageable.EntityType == EntityType.Enemy)
                {
                    OnEnemyKilled?.Invoke(entity);
                }
            }
        }
        
        /// <summary>Raises a checkpoint activated event.</summary>
        public static void RaiseCheckpointActivated(GameObject checkpoint)
        {
            OnCheckpointActivated?.Invoke(checkpoint);
        }
        
        /// <summary>Raises a player respawned event.</summary>
        public static void RaisePlayerRespawned(Vector3 position)
        {
            OnPlayerRespawned?.Invoke(position);
        }
        
        /// <summary>Raises a difficulty changed event (for DDA).</summary>
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
}
