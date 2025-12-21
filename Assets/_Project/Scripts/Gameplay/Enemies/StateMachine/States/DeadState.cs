using UnityEngine;

using GUP.Core;
/// <summary>
/// Dead state - enemy death sequence.
/// Plays death animation and cleans up.
/// </summary>
public class DeadState : EnemyState
{
    public override string StateName => "Dead";
    
    private float deathTimer;
    
    public DeadState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy) { }

    public override void Enter()
    {
        base.Enter();
        deathTimer = 0f;
        
        // Stop all movement animations
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetBool("IsChasing", false);
        enemy.Animator?.SetBool("IsAttacking", false);
        
        // Trigger death animation
        enemy.Animator?.SetTrigger("Die");
        
        // Disable colliders to prevent further interactions
        Collider[] colliders = enemy.GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable any DamageDealer components
        DamageDealer[] dealers = enemy.GetComponents<DamageDealer>();
        foreach (var dealer in dealers)
        {
            dealer.enabled = false;
        }
    }

    public override void Execute()
    {
        deathTimer += Time.deltaTime;
        
        // Destroy after death animation duration
        if (deathTimer >= enemy.DeathDuration)
        {
            enemy.CleanupAndDestroy();
        }
    }

    public override void CheckTransitions()
    {
        // No transitions from dead state
    }

    public override void Exit()
    {
        base.Exit();
    }
}
