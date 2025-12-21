using UnityEngine;

/// <summary>
/// Idle state - enemy stands still, looking around.
/// Transitions to Alert or Chase when player is detected.
/// </summary>
public class IdleState : EnemyState
{
    public override string StateName => "Idle";
    
    private float idleTimer;
    private float lookTimer;
    private Quaternion targetRotation;
    
    public IdleState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        idleTimer = 0f;
        lookTimer = 0f;
        targetRotation = transform.rotation;
        
        enemy.Animator?.SetBool("IsMoving", false);
    }

    public override void Execute()
    {
        idleTimer += Time.deltaTime;
        lookTimer += Time.deltaTime;
        
        // Occasionally look around
        if (lookTimer > Random.Range(2f, 4f))
        {
            lookTimer = 0f;
            float randomAngle = Random.Range(-45f, 45f);
            targetRotation = transform.rotation * Quaternion.Euler(0f, randomAngle, 0f);
        }
        
        // Smooth rotation towards target
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            Time.deltaTime * 2f
        );
    }

    public override void CheckTransitions()
    {
        // Check for player detection
        if (enemy.PlayerDetector != null && 
            enemy.PlayerDetector.IsPlayerInRange(enemy.DetectionRadius))
        {
            // Transition to alert or chase based on enemy settings
            if (enemy.UseAlertState)
            {
                stateMachine.ChangeState(new AlertState(stateMachine, enemy));
            }
            else
            {
                stateMachine.ChangeState(new ChaseState(stateMachine, enemy));
            }
            return;
        }
        
        // Transition to patrol after idle duration
        if (enemy.UsePatrol && idleTimer >= enemy.IdleDuration)
        {
            stateMachine.ChangeState(new PatrolState(stateMachine, enemy));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
