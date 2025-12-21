using UnityEngine;

/// <summary>
/// Puffed up state - SpikyFish inflates and shoots spines.
/// </summary>
public class PuffedUpState : EnemyState
{
    public override string StateName => "PuffedUp";
    
    private SpikyFishEnemy spikyFish;
    private float puffTimer;
    private float shootTimer;
    private int spinesShot;
    
    public PuffedUpState(EnemyStateMachine stateMachine, EnemyBase enemy) 
        : base(stateMachine, enemy)
    {
        spikyFish = enemy as SpikyFishEnemy;
    }

    public override void Enter()
    {
        base.Enter();
        puffTimer = 0f;
        shootTimer = 0f;
        spinesShot = 0;
        
        enemy.Animator?.SetBool("IsMoving", false);
        enemy.Animator?.SetTrigger("PuffUp");
        
        spikyFish?.StartPuffVisuals();
    }

    public override void Execute()
    {
        puffTimer += Time.deltaTime;
        shootTimer += Time.deltaTime;
        
        // Update puff visuals
        spikyFish?.UpdatePuffVisuals(puffTimer);
        
        // Look at player while puffed
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
        
        // Shoot spines
        if (shootTimer >= spikyFish.SpineShootInterval && spinesShot < spikyFish.SpinesPerBurst)
        {
            spikyFish?.ShootSpine();
            spinesShot++;
            shootTimer = 0f;
        }
    }

    public override void CheckTransitions()
    {
        // Return to patrol after puff duration
        if (puffTimer >= spikyFish.PuffDuration)
        {
            spikyFish?.EndPuffVisuals();
            
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
        
        // Stay puffed if player still in range
        // (no early exit from puff state)
    }

    public override void Exit()
    {
        base.Exit();
        spikyFish?.EndPuffVisuals();
    }
}
