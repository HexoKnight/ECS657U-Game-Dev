namespace GUP.Core.StateMachine
{
    /// <summary>
    /// Interface for hierarchical finite state machines.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>Currently active state</summary>
        IState CurrentState { get; }
        
        /// <summary>Name of current state for debugging</summary>
        string CurrentStateName { get; }
        
        /// <summary>
        /// Transition to a new state, calling Exit on current and Enter on new.
        /// </summary>
        void ChangeState(IState newState);
        
        /// <summary>
        /// Set state without calling Exit/Enter. Use for initialization only.
        /// </summary>
        void SetStateImmediate(IState newState);
    }
}
