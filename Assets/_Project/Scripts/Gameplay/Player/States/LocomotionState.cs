using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Composite state managing locomotion substates.
    /// Contains Swimming and MagneticTraversal as substates.
    /// </summary>
    public class LocomotionState : PlayerCompositeState
    {
        private readonly SwimmingState swimmingState;
        private readonly MagneticTraversalState magneticTraversalState;
        
        public override string StateName => "Locomotion";
        
        /// <summary>Access to SwimmingState for external transitions</summary>
        public SwimmingState Swimming => swimmingState;
        
        /// <summary>Access to MagneticTraversalState for external transitions</summary>
        public MagneticTraversalState MagneticTraversal => magneticTraversalState;
        
        public LocomotionState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
            swimmingState = new SwimmingState(stateMachine, context, this);
            magneticTraversalState = new MagneticTraversalState(stateMachine, context, this);
        }
        
        protected override IState GetInitialSubstate()
        {
            // Start in swimming by default (underwater game)
            return swimmingState;
        }
        
        /// <summary>Change to swimming substate</summary>
        public void TransitionToSwimming()
        {
            ChangeSubstate(swimmingState);
        }
        
        /// <summary>Change to magnetic traversal substate</summary>
        public void TransitionToMagneticTraversal()
        {
            ChangeSubstate(magneticTraversalState);
        }
    }
}
