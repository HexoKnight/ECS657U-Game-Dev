using UnityEngine;

namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Hierarchical Finite State Machine base implementation.
    /// Manages state transitions and executes current state logic.
    /// </summary>
    /// <typeparam name="TContext">The context type (e.g., PlayerController)</typeparam>
    public class StateMachineBase<TContext> : IStateMachine where TContext : MonoBehaviour
    {
        private IState currentState;
        private readonly TContext context;
        private readonly bool debugMode;
        
        /// <inheritdoc/>
        public IState CurrentState => currentState;
        
        /// <inheritdoc/>
        public string CurrentStateName => currentState?.StateName ?? "None";
        
        public StateMachineBase(TContext context, bool debug = false)
        {
            this.context = context;
            this.debugMode = debug;
        }
        
        /// <inheritdoc/>
        public void ChangeState(IState newState)
        {
            if (newState == null)
            {
                Debug.LogWarning($"[StateMachine] Attempted to change to null state on {context.name}");
                return;
            }
            
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }
        
        /// <inheritdoc/>
        public void SetStateImmediate(IState newState)
        {
            currentState = newState;
        }
        
        /// <summary>
        /// Update the state machine. Call from MonoBehaviour.Update.
        /// </summary>
        public void Update()
        {
            if (currentState == null) return;
            
            currentState.CheckTransitions();
            currentState.Execute();
        }
        
        /// <summary>
        /// Fixed update the state machine. Call from MonoBehaviour.FixedUpdate.
        /// </summary>
        public void FixedUpdate()
        {
            currentState?.FixedExecute();
        }
    }
}
