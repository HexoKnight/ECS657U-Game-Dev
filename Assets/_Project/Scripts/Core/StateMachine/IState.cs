namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Interface for all state machine states.
    /// </summary>
    public interface IState
    {
        /// <summary>Name of the state for debugging</summary>
        string StateName { get; }
        
        /// <summary>Called when entering this state</summary>
        void Enter();
        
        /// <summary>Called every frame while in this state</summary>
        void Execute();
        
        /// <summary>Called during FixedUpdate for physics operations</summary>
        void FixedExecute();
        
        /// <summary>Called when exiting this state</summary>
        void Exit();
        
        /// <summary>Check and handle state transitions</summary>
        void CheckTransitions();
    }
}
