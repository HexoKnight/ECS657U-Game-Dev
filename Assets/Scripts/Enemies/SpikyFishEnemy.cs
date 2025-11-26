using UnityEngine;
using System.Collections.Generic;

public class SpikyFishEnemy : EnemyBase
{
    [Header("Waypoints")]
    public List<Transform> waypoints;
    public float waypointTolerance = 0.5f;

    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        
        // Move
        transform.position += direction * moveSpeed * GlobalSpeedMultiplier * Time.deltaTime;
        
        // Rotate
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 2f);
        }

        // Check arrival
        if (Vector3.Distance(transform.position, target.position) < waypointTolerance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
    }
}
