using UnityEngine;

/// <summary>
/// Ambush state - anglerfish lunges at player.
/// </summary>
public class AmbushState : EnemyState
{
    public override string StateName => "Ambush";
    
    private AnglerFishEnemy anglerFish;
    private float ambushTimer;
    private Vector3 lungeTarget;
    private Vector3 startPosition;
    private bool hasLunged;
    
    public AmbushState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy)
    {
        anglerFish = enemy as AnglerFishEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        ambushTimer = 0f;
        hasLunged = false;
        startPosition = transform.position;
        
        if (enemy.PlayerDetector != null && enemy.PlayerDetector.Player != null)
        {
            lungeTarget = enemy.PlayerDetector.Player.position;
        }
        else
        {
            lungeTarget = transform.position + transform.forward * 5f;
        }
        
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetTrigger("Ambush");
        
        anglerFish?.StartAmbushVisuals();
    }

    public override void Execute()
    {
        ambushTimer += Time.deltaTime;
        
        // Lunge towards target
        if (!hasLunged)
        {
            float lungeProgress = ambushTimer / anglerFish.LungeDuration;
            if (lungeProgress >= 1f)
            {
                lungeProgress = 1f;
                hasLunged = true;
                anglerFish?.PerformBite();
            }
            
            // Ease out for impact feel
            float easedProgress = 1f - Mathf.Pow(1f - lungeProgress, 3f);
            transform.position = Vector3.Lerp(startPosition, lungeTarget, easedProgress);
        }
    }

    public override void CheckTransitions()
    {
        // Return to lurking after ambush
        if (hasLunged && ambushTimer >= anglerFish.LungeDuration + 0.5f)
        {
            anglerFish?.EndAmbushVisuals();
            stateMachine.ChangeState(new LurkingState(stateMachine, enemy));
        }
    }

    public override void Exit()
    {
        base.Exit();
        anglerFish?.EndAmbushVisuals();
    }
}
