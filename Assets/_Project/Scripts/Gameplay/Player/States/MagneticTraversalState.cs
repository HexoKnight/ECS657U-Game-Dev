using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State for magnetic surface traversal. Handles walking on magnetic surfaces.
    /// </summary>
    public class MagneticTraversalState : PlayerState
    {
        private readonly LocomotionState parentLocomotion;
        
        public override string StateName => "MagneticTraversal";
        
        public MagneticTraversalState(IStateMachine stateMachine, PlayerContext context, LocomotionState parent) 
            : base(stateMachine, context)
        {
            parentLocomotion = parent;
        }
        
        public override void Enter()
        {
            base.Enter();
            // Magnetic traversal initialization
        }
        
        public override void Execute()
        {
            base.Execute();
            // TODO: Magnetic walking input/movement logic will be migrated here
            // For now, old PlayerController code still handles this
        }
        
        public override void FixedExecute()
        {
            // TODO: Magnetic physics will be migrated here
        }
        
        public override void CheckTransitions()
        {
            // TODO: Check for detachment from magnetic surface
            // If detached:
            // parentLocomotion.TransitionToSwimming();
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}
