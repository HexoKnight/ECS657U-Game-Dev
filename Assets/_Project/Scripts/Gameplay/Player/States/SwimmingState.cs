using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State for swimming movement. Handles 3D underwater locomotion.
    /// </summary>
    public class SwimmingState : PlayerState
    {
        private readonly LocomotionState parentLocomotion;
        
        public override string StateName => "Swimming";
        
        public SwimmingState(IStateMachine stateMachine, PlayerContext context, LocomotionState parent) 
            : base(stateMachine, context)
        {
            parentLocomotion = parent;
        }
        
        public override void Enter()
        {
            base.Enter();
            // Swimming-specific initialization
        }
        
        public override void Execute()
        {
            base.Execute();
            // TODO: Swimming input/movement logic will be migrated here
            // For now, old PlayerController code still handles this
        }
        
        public override void FixedExecute()
        {
            // TODO: Swimming physics will be migrated here
        }
        
        public override void CheckTransitions()
        {
            // TODO: Check for magnetic surface attachment
            // If attached to magnetic surface:
            // parentLocomotion.TransitionToMagneticTraversal();
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}
