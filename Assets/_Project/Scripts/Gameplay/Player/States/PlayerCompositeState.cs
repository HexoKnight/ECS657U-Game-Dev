using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Base class for composite player states that contain substates.
    /// </summary>
    public abstract class PlayerCompositeState : IState
    {
        private IState currentSubstate;
        
        /// <summary>Reference to the root state machine</summary>
        protected IStateMachine StateMachine { get; }
        
        /// <summary>Context containing all player references</summary>
        protected PlayerContext Ctx { get; }
        
        /// <summary>Time spent in this composite state</summary>
        protected float StateTime { get; private set; }
        
        /// <summary>Current active substate</summary>
        public IState CurrentSubstate => currentSubstate;
        
        /// <inheritdoc/>
        public abstract string StateName { get; }
        
        /// <summary>Full path including substates for debugging</summary>
        public string FullStatePath => currentSubstate != null 
            ? $"{StateName}/{currentSubstate.StateName}" 
            : StateName;
        
        protected PlayerCompositeState(IStateMachine stateMachine, PlayerContext context)
        {
            StateMachine = stateMachine;
            Ctx = context;
        }
        
        /// <summary>Override to return the initial substate</summary>
        protected abstract IState GetInitialSubstate();
        
        /// <inheritdoc/>
        public virtual void Enter()
        {
            StateTime = 0f;
            #if UNITY_EDITOR
            Debug.Log($"[Player] → {StateName}");
            #endif
            
            var initial = GetInitialSubstate();
            if (initial != null)
            {
                ChangeSubstate(initial);
            }
        }
        
        /// <inheritdoc/>
        public virtual void Execute()
        {
            StateTime += Time.deltaTime;
            
            if (currentSubstate != null)
            {
                currentSubstate.CheckTransitions();
                currentSubstate.Execute();
            }
        }
        
        /// <inheritdoc/>
        public virtual void FixedExecute()
        {
            currentSubstate?.FixedExecute();
        }
        
        /// <inheritdoc/>
        public virtual void Exit()
        {
            currentSubstate?.Exit();
            currentSubstate = null;
            
            #if UNITY_EDITOR
            Debug.Log($"[Player] ← {StateName}");
            #endif
        }
        
        /// <inheritdoc/>
        public virtual void CheckTransitions() { }
        
        /// <summary>Change to a new substate within this composite</summary>
        protected void ChangeSubstate(IState newSubstate)
        {
            if (newSubstate == null) return;
            
            currentSubstate?.Exit();
            currentSubstate = newSubstate;
            currentSubstate.Enter();
        }
        
        /// <summary>Request transition at the parent level</summary>
        protected void RequestParentTransition(IState newState)
        {
            StateMachine.ChangeState(newState);
        }
    }
}
