using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State for ground-based walking/running movement.
    /// Handles normal locomotion underwater (player walks on ground, not swims).
    /// </summary>
    public class GroundWalkState : PlayerState
    {
        private readonly LocomotionState parentLocomotion;
        
        public override string StateName => "GroundWalk";
        
        public GroundWalkState(IStateMachine stateMachine, PlayerContext context, LocomotionState parent) 
            : base(stateMachine, context)
        {
            parentLocomotion = parent;
        }
        
        public override void Enter()
        {
            base.Enter();
            // Ground walk initialization
        }
        
        public override void Execute()
        {
            base.Execute();
            // TODO M4: Ground walk movement will be migrated here
            // For now, PlayerController.UpdateVelocity still handles this
        }
        
        public override void FixedExecute()
        {
            // TODO M4: Physics updates will be migrated here
        }
        
        public override void CheckTransitions()
        {
            // Note: MagneticTraversal transition is handled by MagnetWalker component
            // which modifies PlayerController.targetUp directly.
            // The HFSM observes this but doesn't need to handle the transition itself.
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}
