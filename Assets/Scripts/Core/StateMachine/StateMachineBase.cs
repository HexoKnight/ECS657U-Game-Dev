using UnityEngine;

namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Hierarchical Finite State Machine base implementation.
    /// Manages state transitions and executes current state logic.
    /// </summary>
    /// <typeparam name="TContext">The context type (e.g., PlayerController)</typeparam>
    public class StateMachineBase<TContext> : IStateMachine where TContext : class
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
                string ctxName = context?.GetType().Name ?? typeof(TContext).Name;
                UnityEngine.Debug.LogWarning($"[StateMachine] Attempted to change to null state on {ctxName}");
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

        /// <summary>
        /// Late update the state machine. Call from MonoBehaviour.LateUpdate.
        /// </summary>
        public void LateUpdate()
        {
            currentState?.LateExecute();
        }
    }
}
