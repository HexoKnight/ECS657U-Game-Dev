using UnityEngine;

/// <summary>
/// Sensor component for detecting the player.
/// Uses Physics.OverlapSphere with layer filtering for efficiency.
/// </summary>
public class PlayerDetector : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Detection Settings")]
    [Tooltip("Layer mask for detecting the player")]
    [SerializeField] private LayerMask playerLayer;
    
    [Tooltip("If true, require line of sight (raycast) in addition to range")]
    [SerializeField] private bool requireLineOfSight = false;
    
    [Tooltip("Layer mask for obstacles that block line of sight")]
    [SerializeField] private LayerMask obstacleLayer;
    
    [Tooltip("Offset from transform position for LOS raycast origin")]
    [SerializeField] private Vector3 eyeOffset = Vector3.up * 0.5f;
    
    #endregion

    #region Private State
    
    private Transform cachedPlayer;
    private bool playerInRange;
    private float lastCheckTime;
    private const float CHECK_INTERVAL = 0.1f; // Check every 100ms for performance
    
    #endregion

    #region Properties
    
    /// <summary>Reference to player transform (cached)</summary>
    public Transform Player => cachedPlayer;
    
    /// <summary>True if player was detected in last check</summary>
    public bool IsPlayerDetected => playerInRange && cachedPlayer != null;
    
    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        FindPlayer();
    }
    
    #endregion

    #region Public API
    
    /// <summary>
    /// Check if player is within specified range.
    /// </summary>
    public bool IsPlayerInRange(float range)
    {
        // Throttle checks for performance
        if (Time.time - lastCheckTime < CHECK_INTERVAL)
        {
            return playerInRange;
        }
        lastCheckTime = Time.time;
        
        // Ensure we have player reference
        if (cachedPlayer == null)
        {
            FindPlayer();
            if (cachedPlayer == null)
            {
                playerInRange = false;
                return false;
            }
        }
        
        // Distance check
        float distance = Vector3.Distance(transform.position, cachedPlayer.position);
        if (distance > range)
        {
            playerInRange = false;
            return false;
        }
        
        // Optional line of sight check
        if (requireLineOfSight)
        {
            playerInRange = HasLineOfSight(cachedPlayer.position);
            return playerInRange;
        }
        
        playerInRange = true;
        return true;
    }
    
    /// <summary>
    /// Get distance to player, or float.MaxValue if no player.
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (cachedPlayer == null)
        {
            FindPlayer();
        }
        
        return cachedPlayer != null ? 
            Vector3.Distance(transform.position, cachedPlayer.position) : 
            float.MaxValue;
    }
    
    /// <summary>
    /// Get direction to player (normalized).
    /// </summary>
    public Vector3 GetDirectionToPlayer()
    {
        if (cachedPlayer == null) return Vector3.zero;
        return (cachedPlayer.position - transform.position).normalized;
    }
    
    /// <summary>
    /// Check if we have clear line of sight to target position.
    /// </summary>
    public bool HasLineOfSight(Vector3 targetPosition)
    {
        Vector3 origin = transform.position + eyeOffset;
        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;
        
        return !Physics.Raycast(origin, direction.normalized, distance, obstacleLayer);
    }
    
    #endregion

    #region Private Methods
    
    private void FindPlayer()
    {
        // Try to find player by tag first (fastest)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            cachedPlayer = playerObj.transform;
            return;
        }
        
        // Fallback: find PlayerController
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            cachedPlayer = pc.transform;
        }
    }
    
    #endregion

    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Draw eye position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, 0.1f);
        
        // Draw line to player if in play mode
        if (Application.isPlaying && cachedPlayer != null)
        {
            Gizmos.color = playerInRange ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position + eyeOffset, cachedPlayer.position);
        }
    }
    
    #endregion
}
