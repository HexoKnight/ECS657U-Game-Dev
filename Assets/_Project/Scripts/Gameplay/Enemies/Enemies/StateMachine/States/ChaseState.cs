using UnityEngine;

/// <summary>
/// Chase state - enemy actively pursues the player.
/// Transitions to Attack when in range, or back to patrol/idle if player escapes.
/// </summary>
public class ChaseState : EnemyState
{
    public override string StateName => "Chase";
    
    private float lostPlayerTimer;
    
    public ChaseState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        lostPlayerTimer = 0f;
        
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", true);
            enemy.Animator.SetBool("IsChasing", true);
        }
    }

    public override void Execute()
    {
        if (enemy.PlayerDetector == null || enemy.PlayerDetector.Player == null)
        {
            lostPlayerTimer += Time.deltaTime;
            return;
        }
        
        Transform player = enemy.PlayerDetector.Player;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is in detection range
        if (!enemy.PlayerDetector.IsPlayerInRange(enemy.ChaseRadius))
        {
            lostPlayerTimer += Time.deltaTime;
        }
        else
        {
            lostPlayerTimer = 0f;
        }
        
        // Move towards player (only if not in attack range)
        if (distanceToPlayer > enemy.AttackRange)
        {
            float speed = enemy.ChaseSpeed * EnemyBase.GlobalSpeedMultiplier;
            transform.position += directionToPlayer * speed * Time.deltaTime;
        }
        
        // Rotate towards player
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * enemy.RotationSpeed * 1.5f
            );
        }
    }

    public override void CheckTransitions()
    {
        // If player lost for too long, return to patrol/idle
        if (lostPlayerTimer >= enemy.LosePlayerTime)
        {
            if (enemy.UsePatrol)
            {
                stateMachine.ChangeState(new PatrolState(stateMachine, enemy));
            }
            else
            {
                stateMachine.ChangeState(new IdleState(stateMachine, enemy));
            }
            return;
        }
        
        // Transition to attack if in range
        if (enemy.PlayerDetector != null && enemy.PlayerDetector.Player != null)
        {
            float distanceToPlayer = enemy.PlayerDetector.GetDistanceToPlayer();
            if (distanceToPlayer <= enemy.AttackRange)
            {
                stateMachine.ChangeState(new AttackState(stateMachine, enemy));
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (enemy.Animator != null) enemy.Animator.SetBool("IsChasing", false);
    }
}
