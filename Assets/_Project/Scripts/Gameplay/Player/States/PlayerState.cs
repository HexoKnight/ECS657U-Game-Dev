using UnityEngine;

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
            #if UNITY_EDITOR
            Debug.Log($"[Player] → {StateName}");
            #endif
        }
        
        /// <inheritdoc/>
        public virtual void Execute()
        {
            StateTime += Time.deltaTime;
        }
        
        /// <inheritdoc/>
        public virtual void FixedExecute() { }
        
        /// <inheritdoc/>
        public virtual void Exit()
        {
            #if UNITY_EDITOR
            Debug.Log($"[Player] ← {StateName}");
            #endif
        }
        
        /// <inheritdoc/>
        public virtual void CheckTransitions() { }
    }
}
