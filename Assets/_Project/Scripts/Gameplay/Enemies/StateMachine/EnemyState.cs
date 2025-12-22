using UnityEngine;

using GUP.Core;
using GUP.Core.Debug;

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
    
    /// <summary>Previous state name for logging transitions</summary>
    protected string previousStateName;

    public EnemyState(EnemyStateMachine stateMachine, EnemyBase enemy)
    {
        this.stateMachine = stateMachine;
        this.enemy = enemy;
        this.transform = enemy.transform;
    }

    /// <summary>Called when entering this state</summary>
    public virtual void Enter() 
    {
        // Log state transition via GupDebug
        string fromState = stateMachine.CurrentState?.StateName ?? "None";
        GupDebug.LogEnemyStateChange(enemy.name, fromState, StateName);
    }
    
    /// <summary>Called every frame while in this state</summary>
    public abstract void Execute();
    
    /// <summary>Called when exiting this state</summary>
    public virtual void Exit() 
    {
        // Exit logging handled by Enter of new state
    }
    
    /// <summary>Check and perform state transitions</summary>
    public abstract void CheckTransitions();
    
    /// <summary>Called during FixedUpdate for physics-based movement</summary>
    public virtual void FixedExecute() { }
    
    /// <summary>Optional: respond to taking damage</summary>
    public virtual void OnTakeDamage(DamageData damage) { }
}
