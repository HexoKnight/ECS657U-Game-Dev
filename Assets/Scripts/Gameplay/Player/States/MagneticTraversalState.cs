using UnityEngine;

using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State for magnetic surface traversal. Handles walking on magnetic surfaces.
    /// Note: MagnetWalker component handles attach/detach and sets PlayerController.targetUp.
    /// This state observes the magnetic state but doesn't control attachment.
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
            // Note: targetUp is already set by MagnetWalker before entering this state
        }
        
        public override void Execute()
        {
            base.Execute();
            // TODO M5: Magnetic walking logic will be migrated here
            // Movement uses the modified targetUp from MagnetWalker
        }
        
        public override void FixedExecute()
        {
            // TODO M5: Physics updates for magnetic traversal
        }
        
        public override void CheckTransitions()
        {
            // Check if MagnetWalker has detached (targetUp returned to Vector3.up)
            // If so, transition back to GroundWalk
            // if (Ctx.TargetUp == Vector3.up)
            //     parentLocomotion.TransitionToGroundWalk();
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}
