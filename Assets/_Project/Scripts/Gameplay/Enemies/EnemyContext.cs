using UnityEngine;

using GUP.Core.Config;

/// <summary>
/// Plain C# object (POCO) holding shared references and runtime state for enemy states.
/// Persists across state transitions, unlike state-local fields.
/// </summary>
public class EnemyContext
{
    #region References (Immutable after construction)
    
    /// <summary>The enemy's transform.</summary>
    public Transform Transform { get; }
    
    /// <summary>The EnemyBase component.</summary>
    public EnemyBase Enemy { get; }
    
    /// <summary>The EnemyConfig asset (may be null, use Enemy properties for fallback).</summary>
    public EnemyConfig Config { get; }
    
    /// <summary>The player detector component.</summary>
    public PlayerDetector Detector { get; }
    
    /// <summary>The state machine managing this enemy.</summary>
    public EnemyStateMachine StateMachine { get; }
    
    #endregion

    #region Runtime State (Persists across state changes)
    
    /// <summary>Last known position of the player when detected.</summary>
    public Vector3 LastKnownPlayerPosition { get; set; }
    
    /// <summary>Time since the player was last within detection range.</summary>
    public float TimeSincePlayerSeen { get; set; }
    
    /// <summary>Whether the player is currently detected.</summary>
    public bool IsPlayerDetected { get; set; }
    
    /// <summary>Current patrol waypoint index.</summary>
    public int CurrentWaypointIndex { get; set; }
    
    #endregion

    #region Constructor
    
    public EnemyContext(
        EnemyBase enemy,
        EnemyStateMachine stateMachine,
        PlayerDetector detector,
        EnemyConfig config)
    {
        Enemy = enemy;
        Transform = enemy.transform;
        StateMachine = stateMachine;
        Detector = detector;
        Config = config;
        
        // Initialize runtime state
        LastKnownPlayerPosition = Vector3.zero;
        TimeSincePlayerSeen = float.MaxValue;
        IsPlayerDetected = false;
        CurrentWaypointIndex = 0;
    }
    
    #endregion

    #region Helper Methods
    
    /// <summary>
    /// Updates player tracking state. Call every frame from state machine.
    /// </summary>
    public void UpdatePlayerTracking()
    {
        if (Detector == null || Detector.Player == null)
        {
            TimeSincePlayerSeen += Time.deltaTime;
            IsPlayerDetected = false;
            return;
        }
        
        float distance = Detector.GetDistanceToPlayer();
        bool wasDetected = IsPlayerDetected;
        IsPlayerDetected = distance <= Enemy.DetectionRadius;
        
        if (IsPlayerDetected)
        {
            LastKnownPlayerPosition = Detector.Player.position;
            TimeSincePlayerSeen = 0f;
        }
        else
        {
            TimeSincePlayerSeen += Time.deltaTime;
        }
        
        // Log detection changes
        if (IsPlayerDetected != wasDetected)
        {
            GUP.Core.Debug.GupDebug.LogEnemyDetection(Enemy.name, IsPlayerDetected, distance);
        }
    }
    
    #endregion
}
