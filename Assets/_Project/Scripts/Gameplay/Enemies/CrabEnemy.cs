using UnityEngine;

using GUP.Core.Debug;

/// <summary>
/// Crab enemy with patrol, alert, chase, and melee attack behaviors.
/// Uses state machine from EnemyBase.
/// </summary>
public class CrabEnemy : EnemyBase
{
    #region Crab-Specific Settings
    
    [Header("Crab Settings")]
    [Tooltip("Primary patrol point A")]
    [SerializeField] private Transform pointA;
    
    [Tooltip("Primary patrol point B")]
    [SerializeField] private Transform pointB;
    
    [Tooltip("Snapping animation when attacking")]
    [SerializeField] private bool useSnapAnimation = true;
    
    #endregion

    #region Lifecycle
    
    protected override void Awake()
    {
        base.Awake();
        
        // If patrol points are set, add them to waypoints
        if (patrolWaypoints == null)
        {
            patrolWaypoints = new System.Collections.Generic.List<Transform>();
        }
        
        if (pointA != null && !patrolWaypoints.Contains(pointA))
        {
            patrolWaypoints.Insert(0, pointA);
        }
        if (pointB != null && !patrolWaypoints.Contains(pointB))
        {
            patrolWaypoints.Add(pointB);
        }
    }

    protected override void Start()
    {
        // Detach points so they don't move with the crab
        if (pointA != null) pointA.SetParent(null);
        if (pointB != null) pointB.SetParent(null);
        
        base.Start();
    }
    
    public override void OnSpawn()
    {
        base.OnSpawn();
        
        GupDebug.Log(LogCategory.Enemy, $"{name} spawned with {patrolWaypoints.Count} patrol points");
    }
    
    #endregion

    #region Attack Override
    
    public override void PerformAttack()
    {
        // Crab-specific attack: snap claws
        if (useSnapAnimation && Animator != null)
        {
            Animator.SetTrigger("Snap");
        }
        
        // Call base attack logic
        base.PerformAttack();
        
        GupDebug.Log(LogCategory.Enemy, $"{name} snapped claws!");
    }
    
    #endregion
}
