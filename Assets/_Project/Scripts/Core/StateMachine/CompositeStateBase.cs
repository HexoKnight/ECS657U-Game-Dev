using UnityEngine;

namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Base class for composite (hierarchical) states that contain substates.
    /// Parent state owns a current substate and delegates execution to it.
    /// </summary>
    /// <typeparam name="TContext">The context type (e.g., PlayerController)</typeparam>
    public abstract class CompositeStateBase<TContext> : StateBase<TContext> where TContext : MonoBehaviour
    {
        private IState currentSubstate;
        
        /// <summary>Current active substate within this composite state</summary>
        public IState CurrentSubstate => currentSubstate;
        
        /// <summary>Name including current substate path</summary>
        public string FullStatePath => currentSubstate != null 
            ? $"{StateName}/{currentSubstate.StateName}" 
            : StateName;
        
        protected CompositeStateBase(IStateMachine stateMachine, TContext context) 
            : base(stateMachine, context)
        {
        }
        
        /// <summary>
        /// Override to return the initial substate for this composite.
        /// Called during Enter().
        /// </summary>
        /// <returns>The initial substate, or null if no substates</returns>
        protected abstract IState GetInitialSubstate();
        
        /// <inheritdoc/>
        public override void Enter()
        {
            base.Enter();
            
            // Enter initial substate
            var initialSubstate = GetInitialSubstate();
            if (initialSubstate != null)
            {
                ChangeSubstate(initialSubstate);
            }
        }
        
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();
            
            // Delegate to substate
            if (currentSubstate != null)
            {
                currentSubstate.CheckTransitions();
                currentSubstate.Execute();
            }
        }
        
        /// <inheritdoc/>
        public override void FixedExecute()
        {
            base.FixedExecute();
            
            // Delegate to substate
            currentSubstate?.FixedExecute();
        }
        
        /// <inheritdoc/>
        public override void Exit()
        {
            // Exit current substate first
            currentSubstate?.Exit();
            currentSubstate = null;
            
            base.Exit();
        }
        
        /// <summary>
        /// Change the current substate within this composite.
        /// </summary>
        /// <param name="newSubstate">The new substate to transition to</param>
        protected void ChangeSubstate(IState newSubstate)
        {
            if (newSubstate == null) return;
            
            currentSubstate?.Exit();
            currentSubstate = newSubstate;
            currentSubstate.Enter();
        }
        
        /// <summary>
        /// Request a transition to a sibling state through the parent state machine.
        /// Use this when a substate needs to trigger a parent-level transition.
        /// </summary>
        /// <param name="newState">The new parent-level state to transition to</param>
        protected void RequestParentTransition(IState newState)
        {
            StateMachine.ChangeState(newState);
        }
    }
}
