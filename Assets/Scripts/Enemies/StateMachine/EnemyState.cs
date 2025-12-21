using UnityEngine;

/// <summary>
/// Abstract base class for all enemy states.
/// Each state defines behavior for Enter, Execute, Exit, and CheckTransitions.
/// </summary>
public abstract class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected EnemyBase enemy;
    protected Transform transform;
    
    /// <summary>State name for debugging</summary>
    public abstract string StateName { get; }

    public EnemyState(EnemyStateMachine stateMachine, EnemyBase enemy)
    {
        this.stateMachine = stateMachine;
        this.enemy = enemy;
        this.transform = enemy.transform;
    }

    /// <summary>Called when entering this state</summary>
    public virtual void Enter() 
    {
        if (enemy.DebugStates)
        {
            Debug.Log($"[{enemy.name}] Entering state: {StateName}");
        }
    }
    
    /// <summary>Called every frame while in this state</summary>
    public abstract void Execute();
    
    /// <summary>Called when exiting this state</summary>
    public virtual void Exit() 
    {
        if (enemy.DebugStates)
        {
            Debug.Log($"[{enemy.name}] Exiting state: {StateName}");
        }
    }
    
    /// <summary>Check and perform state transitions</summary>
    public abstract void CheckTransitions();
    
    /// <summary>Called during FixedUpdate for physics-based movement</summary>
    public virtual void FixedExecute() { }
    
    /// <summary>Optional: respond to taking damage</summary>
    public virtual void OnTakeDamage(DamageData damage) { }
}
