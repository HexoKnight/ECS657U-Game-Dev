using UnityEngine;

namespace GUP.Core.Config
{
    /// <summary>
    /// Configuration asset for enemy parameters.
    /// Create instances via Assets > Create > GUP/Config/Enemy.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "GUP/Config/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Health")]
        [Tooltip("Maximum health of the enemy")]
        public float maxHealth = 100f;
        
        [Tooltip("Time invulnerable after taking damage")]
        public float invulnerabilityTime = 0.2f;
        
        [Header("Movement")]
        [Tooltip("Base movement speed during patrol")]
        public float moveSpeed = 3f;
        
        [Tooltip("Speed when chasing the player")]
        public float chaseSpeed = 5f;
        
        [Tooltip("Turn speed in degrees per second")]
        public float turnSpeed = 180f;
        
        [Tooltip("Acceleration rate")]
        public float acceleration = 10f;
        
        [Header("Detection")]
        [Tooltip("Range at which enemy detects player")]
        public float detectionRange = 10f;
        
        [Tooltip("Range at which enemy loses player")]
        public float loseTargetRange = 15f;
        
        [Tooltip("Time to forget player after losing line of sight")]
        public float forgetTime = 3f;
        
        [Header("Combat")]
        [Tooltip("Damage dealt per attack")]
        public float attackDamage = 10f;
        
        [Tooltip("Range at which enemy can attack")]
        public float attackRange = 2f;
        
        [Tooltip("Cooldown between attacks")]
        public float attackCooldown = 1f;
        
        [Tooltip("Knockback force applied on hit")]
        public float knockbackForce = 8f;
        
        [Tooltip("Windup time before attack lands")]
        public float attackWindup = 0.3f;
        
        [Header("Patrol")]
        [Tooltip("Wait time at each waypoint")]
        public float patrolWaitTime = 1f;
        
        [Tooltip("Distance to consider waypoint reached")]
        public float waypointTolerance = 0.5f;
        
        [Header("State Machine")]
        [Tooltip("Duration of alert state before chasing")]
        public float alertDuration = 0.5f;
        
        [Header("Death")]
        [Tooltip("Time before enemy is destroyed after death")]
        public float deathDuration = 2f;
    }
}
