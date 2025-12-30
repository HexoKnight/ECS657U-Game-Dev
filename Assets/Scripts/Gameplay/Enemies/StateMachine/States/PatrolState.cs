using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Patrol state - enemy moves between waypoints.
/// Can use Transform waypoints or a patrol radius.
/// </summary>
public class PatrolState : EnemyState
{
    public override string StateName => "Patrol";
    
    private int currentWaypointIndex;
    private Vector3 currentTarget;
    private float waitTimer;
    private bool isWaiting;
    
    public PatrolState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        isWaiting = false;
        waitTimer = 0f;
        
        // Initialize waypoint
        if (enemy.PatrolWaypoints != null && enemy.PatrolWaypoints.Count > 0)
        {
            currentTarget = enemy.PatrolWaypoints[currentWaypointIndex].position;
        }
        else
        {
            // Random patrol within radius
            currentTarget = GetRandomPatrolPoint();
        }
        
        if (enemy.Animator != null) enemy.Animator.SetBool("IsMoving", true);
    }

    public override void Execute()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= enemy.PatrolWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
            return;
        }
        
        // Move towards current target
        MoveTowardsTarget();
        
        // Check if reached target
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        if (distanceToTarget < enemy.WaypointTolerance)
        {
            isWaiting = true;
            if (enemy.Animator != null) enemy.Animator.SetBool("IsMoving", false);
        }
    }

    public override void CheckTransitions()
    {
        // Check for player detection
        if (enemy.PlayerDetector != null && 
            enemy.PlayerDetector.IsPlayerInRange(enemy.DetectionRadius))
        {
            if (enemy.UseAlertState)
            {
                stateMachine.ChangeState(new AlertState(stateMachine, enemy));
            }
            else
            {
                stateMachine.ChangeState(new ChaseState(stateMachine, enemy));
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (enemy.Animator != null) enemy.Animator.SetBool("IsMoving", false);
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (currentTarget - transform.position).normalized;
        
        // Move
        float speed = enemy.MoveSpeed * EnemyBase.GlobalSpeedMultiplier;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate towards movement direction
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * enemy.RotationSpeed
            );
        }
    }

    private void MoveToNextWaypoint()
    {
        if (enemy.PatrolWaypoints != null && enemy.PatrolWaypoints.Count > 0)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % enemy.PatrolWaypoints.Count;
            currentTarget = enemy.PatrolWaypoints[currentWaypointIndex].position;
        }
        else
        {
            currentTarget = GetRandomPatrolPoint();
        }
        
        if (enemy.Animator != null) enemy.Animator.SetBool("IsMoving", true);
    }

    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * enemy.PatrolRadius;
        randomDirection.y = 0; // Keep on same Y level (for ground enemies)
        return enemy.PatrolCenter + randomDirection;
    }
}
