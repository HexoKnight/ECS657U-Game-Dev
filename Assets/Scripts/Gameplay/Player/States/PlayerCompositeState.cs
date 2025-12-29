using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Base class for composite player states that contain substates.
    /// Inherits from PlayerState to support KCC callbacks.
    /// </summary>
    public abstract class PlayerCompositeState : PlayerState
    {
        private IState currentSubstate;
        
        /// <summary>Current active substate</summary>
        public IState CurrentSubstate => currentSubstate;
        
        /// <summary>Full path including substates for debugging</summary>
        public string FullStatePath => currentSubstate != null 
            ? $"{StateName}/{currentSubstate.StateName}" 
            : StateName;
        
        protected PlayerCompositeState(IStateMachine stateMachine, PlayerContext context)
            : base(stateMachine, context)
        {
        }
        
        /// <summary>Override to return the initial substate</summary>
        protected abstract IState GetInitialSubstate();
        
        /// <inheritdoc/>
        public override void Enter()
        {
            base.Enter();
            
            var initial = GetInitialSubstate();
            if (initial != null)
            {
                ChangeSubstate(initial);
            }
        }
        
        /// <inheritdoc/>
        public override void Execute()
        {
            base.Execute();
            
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
            currentSubstate?.FixedExecute();
        }

        public override void LateExecute()
        {
            base.LateExecute();
            currentSubstate?.LateExecute();
        }
        
        /// <inheritdoc/>
        public override void Exit()
        {
            currentSubstate?.Exit();
            currentSubstate = null;
            base.Exit();
        }
        
        // KCC Delegations
        
        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (currentSubstate is PlayerState playerState)
            {
                playerState.OnUpdateVelocity(ref currentVelocity, deltaTime);
            }
        }
        
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (currentSubstate is PlayerState playerState)
            {
                playerState.OnUpdateRotation(ref currentRotation, deltaTime);
            }
        }

        public override void OnAfterCharacterUpdate(float deltaTime)
        {
            if (currentSubstate is PlayerState playerState)
            {
                playerState.OnAfterCharacterUpdate(deltaTime);
            }
        }
        
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
