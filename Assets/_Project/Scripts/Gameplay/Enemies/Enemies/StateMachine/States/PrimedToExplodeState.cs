using UnityEngine;

/// <summary>
/// Primed to explode state - visual warning before explosion.
/// Specific to ExplodingFishEnemy.
/// </summary>
public class PrimedToExplodeState : EnemyState
{
    public override string StateName => "PrimedToExplode";
    
    private ExplodingFishEnemy explodingFish;
    private float primedTimer;
    
    public PrimedToExplodeState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy)
    {
        explodingFish = enemy as ExplodingFishEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        primedTimer = 0f;
        
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetTrigger("Prime");
        
        // Start visual feedback
        explodingFish?.StartPrimedVisuals();
    }

    public override void Execute()
    {
        primedTimer += Time.deltaTime;
        
        // Update visual feedback
        explodingFish?.UpdatePrimedVisuals(primedTimer);
    }

    public override void CheckTransitions()
    {
        // Explode when timer reaches delay
        if (primedTimer >= explodingFish.ExplosionDelay)
        {
            explodingFish.Explode();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
