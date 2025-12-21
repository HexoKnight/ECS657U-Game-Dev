using UnityEngine;

/// <summary>
/// Alert state - enemy has noticed player but hasn't started chasing yet.
/// Visual cue state: enemy pauses, looks at player, maybe raises weapons.
/// </summary>
public class AlertState : EnemyState
{
    public override string StateName => "Alert";
    
    private float alertTimer;
    
    public AlertState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        alertTimer = 0f;
        
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetTrigger("Alert");
    }

    public override void Execute()
    {
        alertTimer += Time.deltaTime;
        
        // Look at player
        if (enemy.PlayerDetector != null && enemy.PlayerDetector.Player != null)
        {
            Vector3 directionToPlayer = enemy.PlayerDetector.GetDirectionToPlayer();
            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * enemy.RotationSpeed * 2f // Faster rotation when alerted
                );
            }
        }
    }

    public override void CheckTransitions()
    {
        // After alert duration, transition to chase
        if (alertTimer >= enemy.AlertDuration)
        {
            stateMachine.ChangeState(new ChaseState(stateMachine, enemy));
            return;
        }
        
        // If player escapes detection range, go back to previous state
        if (enemy.PlayerDetector == null || 
            !enemy.PlayerDetector.IsPlayerInRange(enemy.DetectionRadius * 1.2f)) // Slightly larger range for hysteresis
        {
            if (enemy.UsePatrol)
            {
                stateMachine.ChangeState(new PatrolState(stateMachine, enemy));
            }
            else
            {
                stateMachine.ChangeState(new IdleState(stateMachine, enemy));
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
