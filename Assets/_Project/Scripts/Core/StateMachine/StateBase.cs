using UnityEngine;

namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Base class for states with common functionality.
    /// </summary>
    /// <typeparam name="TContext">The context type (e.g., PlayerController, EnemyBase)</typeparam>
    public abstract class StateBase<TContext> : IState where TContext : MonoBehaviour
    {
        /// <summary>Reference to the state machine</summary>
        protected IStateMachine StateMachine { get; }
        
        /// <summary>Reference to the context (owner)</summary>
        protected TContext Context { get; }
        
        /// <summary>Convenient access to context transform</summary>
        protected Transform Transform => Context.transform;
        
        /// <inheritdoc/>
        public abstract string StateName { get; }
        
        /// <summary>Time spent in this state</summary>
        protected float StateTime { get; private set; }
        
        protected StateBase(IStateMachine stateMachine, TContext context)
        {
            StateMachine = stateMachine;
            Context = context;
        }
        
        /// <inheritdoc/>
        public virtual void Enter()
        {
            StateTime = 0f;
            #if UNITY_EDITOR
            Debug.Log($"[{Context.name}] → {StateName}");
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
            Debug.Log($"[{Context.name}] ← {StateName}");
            #endif
        }
        
        /// <inheritdoc/>
        public virtual void CheckTransitions() { }
    }
}
