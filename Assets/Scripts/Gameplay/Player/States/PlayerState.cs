using UnityEngine;
using GUP.Core.Debug;
using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Base class for all player states. Uses PlayerContext for type-safe access.
    /// </summary>
    public abstract class PlayerState : IState
    {
        /// <summary>Reference to the state machine</summary>
        protected IStateMachine StateMachine { get; }
        
        /// <summary>Context containing all player references</summary>
        protected PlayerContext Ctx { get; }
        
        /// <summary>Time spent in this state</summary>
        protected float StateTime { get; private set; }
        
        /// <inheritdoc/>
        public abstract string StateName { get; }
        
        protected PlayerState(IStateMachine stateMachine, PlayerContext context)
        {
            StateMachine = stateMachine;
            Ctx = context;
        }
        
        /// <inheritdoc/>
        public virtual void Enter()
        {
            StateTime = 0f;
            GupDebug.LogStateTransition("[none]", StateName, "Enter");
        }
        
        /// <inheritdoc/>
        public virtual void Execute()
        {
            StateTime += Time.deltaTime;
        }
        
        /// <inheritdoc/>
        public virtual void FixedExecute() { }
        
        /// <inheritdoc/>
        public virtual void LateExecute() { }
        
        /// <inheritdoc/>
        public virtual void Exit()
        {
            GupDebug.LogStateTransition(StateName, "[none]", "Exit");
        }
        
        /// <inheritdoc/>
        public virtual void CheckTransitions() { }
        
        // KCC Callbacks
        
        public virtual void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }
        
        public virtual void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
        
        public virtual void OnAfterCharacterUpdate(float deltaTime) { }
    }
}
