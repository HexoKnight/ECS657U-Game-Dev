using UnityEngine;

/// <summary>
/// Attack state - enemy attacks the player.
/// Handles attack cooldowns and transitions back to chase if player escapes.
/// </summary>
public class AttackState : EnemyState
{
    public override string StateName => "Attack";
    
    private float attackTimer;
    private bool hasAttackedThisCycle;
    
    public AttackState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        attackTimer = 0f;
        hasAttackedThisCycle = false;
        
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", false);
            enemy.Animator.SetBool("IsAttacking", true);
        }
    }

    public override void Execute()
    {
        attackTimer += Time.deltaTime;
        
        // Look at player while attacking
        if (enemy.PlayerDetector != null && enemy.PlayerDetector.Player != null)
        {
            Vector3 directionToPlayer = enemy.PlayerDetector.GetDirectionToPlayer();
            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * enemy.RotationSpeed
                );
            }
        }
        
        // Perform attack when cooldown is ready
        if (!hasAttackedThisCycle && attackTimer >= enemy.AttackWindup)
        {
            PerformAttack();
            hasAttackedThisCycle = true;
        }
        
        // Reset attack cycle
        if (attackTimer >= enemy.AttackCooldown)
        {
            attackTimer = 0f;
            hasAttackedThisCycle = false;
            if (enemy.Animator != null) enemy.Animator.SetTrigger("Attack");
        }
    }

    public override void CheckTransitions()
    {
        if (enemy.PlayerDetector == null)
        {
            stateMachine.ChangeState(new IdleState(stateMachine, enemy));
            return;
        }
        
        float distanceToPlayer = enemy.PlayerDetector.GetDistanceToPlayer();
        
        // If player escaped attack range, chase
        if (distanceToPlayer > enemy.AttackRange * 1.5f)
        {
            stateMachine.ChangeState(new ChaseState(stateMachine, enemy));
            return;
        }
        
        // If player escaped detection range entirely, return to patrol/idle
        if (distanceToPlayer > enemy.ChaseRadius)
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
        if (enemy.Animator != null) enemy.Animator.SetBool("IsAttacking", false);
    }

    private void PerformAttack()
    {
        // Call the enemy's attack method
        enemy.PerformAttack();
    }
}
