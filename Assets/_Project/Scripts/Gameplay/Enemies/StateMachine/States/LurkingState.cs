using UnityEngine;

/// <summary>
/// Lurking state - anglerfish hides and uses lure to attract player.
/// </summary>
public class LurkingState : EnemyState
{
    public override string StateName => "Lurking";
    
    private AnglerFishEnemy anglerFish;
    private float lurkTimer;
    
    public LurkingState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy)
    {
        anglerFish = enemy as AnglerFishEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        lurkTimer = 0f;
        
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetBool("IsLurking", true);
        
        anglerFish?.EnableLure(true);
    }

    public override void Execute()
    {
        lurkTimer += Time.deltaTime;
        
        // Animate the lure
        anglerFish?.UpdateLure(lurkTimer);
    }

    public override void CheckTransitions()
    {
        // Wait for cooldown before ambushing again
        if (lurkTimer < anglerFish.AmbushCooldown) return;
        
        // Check if player is in ambush range
        if (enemy.PlayerDetector != null && 
            enemy.PlayerDetector.IsPlayerInRange(anglerFish.AmbushRange))
        {
            stateMachine.ChangeState(new AmbushState(stateMachine, enemy));
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemy.Animator?.SetBool("IsLurking", false);
        anglerFish?.EnableLure(false);
    }
}
