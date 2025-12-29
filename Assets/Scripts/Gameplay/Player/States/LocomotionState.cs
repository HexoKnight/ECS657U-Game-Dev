using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// Composite state managing locomotion substates.
    /// Contains GroundWalk and MagneticTraversal as substates.
    /// Note: MagnetWalker component handles attach/detach externally.
    /// </summary>
    public class LocomotionState : PlayerCompositeState
    {
        private readonly GroundWalkState groundWalkState;
        private readonly MagneticTraversalState magneticTraversalState;
        
        public override string StateName => "Locomotion";
        
        /// <summary>Access to GroundWalkState for external transitions</summary>
        public GroundWalkState GroundWalk => groundWalkState;
        
        /// <summary>Access to MagneticTraversalState for external transitions</summary>
        public MagneticTraversalState MagneticTraversal => magneticTraversalState;
        
        public LocomotionState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
            groundWalkState = new GroundWalkState(stateMachine, context, this);
            magneticTraversalState = new MagneticTraversalState(stateMachine, context, this);
        }
        
        protected override IState GetInitialSubstate()
        {
            // Start in ground walk (player walks underwater)
            return groundWalkState;
        }
        
        /// <summary>Change to ground walk substate</summary>
        public void TransitionToGroundWalk()
        {
            ChangeSubstate(groundWalkState);
        }
        
        /// <summary>Change to magnetic traversal substate</summary>
        public void TransitionToMagneticTraversal()
        {
            ChangeSubstate(magneticTraversalState);
        }
    }
}
