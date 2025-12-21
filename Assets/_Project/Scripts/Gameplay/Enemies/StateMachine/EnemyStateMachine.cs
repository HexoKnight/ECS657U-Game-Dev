using UnityEngine;

/// <summary>
/// Manages enemy state transitions and executes current state logic.
/// Attach to enemy GameObjects alongside EnemyBase-derived scripts.
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    #region Private State
    
    private EnemyState currentState;
    private EnemyBase enemy;
    
    #endregion

    #region Properties
    
    /// <summary>Currently active state</summary>
    public EnemyState CurrentState => currentState;
    
    /// <summary>Name of current state for debugging</summary>
    public string CurrentStateName => currentState?.StateName ?? "None";
    
    #endregion

    #region Initialization
    
    /// <summary>
    /// Initialize the state machine with reference to enemy.
    /// Must be called before using the state machine.
    /// </summary>
    public void Initialize(EnemyBase enemyBase)
    {
        enemy = enemyBase;
    }
    
    #endregion

    #region State Management
    
    /// <summary>
    /// Transition to a new state.
    /// </summary>
    public void ChangeState(EnemyState newState)
    {
        if (newState == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Attempted to change to null state!");
            return;
        }
        
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
    
    /// <summary>
    /// Force immediate state change without exit/enter calls.
    /// Use sparingly, mainly for initialization.
    /// </summary>
    public void SetStateImmediate(EnemyState newState)
    {
        currentState = newState;
    }
    
    #endregion

    #region Unity Lifecycle
    
    private void Update()
    {
        if (currentState == null || enemy == null) return;
        if (enemy.IsDead()) return;
        
        currentState.CheckTransitions();
        currentState.Execute();
    }
    
    private void FixedUpdate()
    {
        if (currentState == null || enemy == null) return;
        if (enemy.IsDead()) return;
        
        currentState.FixedExecute();
    }
    
    #endregion

    #region Event Forwarding
    
    /// <summary>Forward damage events to current state</summary>
    public void OnTakeDamage(DamageData damage)
    {
        currentState?.OnTakeDamage(damage);
    }
    
    #endregion
}
