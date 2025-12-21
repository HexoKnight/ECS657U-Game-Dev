using UnityEngine;
using GUP.Core.Debug;
using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State acting as the "StaticSpline" mode (Path Follow).
    /// Wraps the spline evaluation logic exposed by PlayerController.
    /// </summary>
    public class PathFollowState : PlayerState
    {
        public override string StateName => "PathFollow";

        public PathFollowState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
        }

        public override void Enter()
        {
            base.Enter();
            GupDebug.LogPathStart(Ctx.Controller.gameObject.name, "Spline");
        }

        public override void FixedExecute()
        {
            base.FixedExecute();
            // Path following happens in FixedUpdate (KCC lifecycle) or just Update depending on original code.
            // Original code had it in FixedUpdate loop for KCC? 
            // Wait, standard KCC uses UpdateVelocity/Rotation.
            // But StaticSpline case in `FixedUpdate` (line 345) explicitly does logic.
            // So we call the kernel here.
            
            Ctx.Controller.UpdatePathFollowing(Time.deltaTime);
        }
        
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // No rotation update from KCC system in this state (handled by FixedExecute/Path logic)
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Zero velocity in this state
            currentVelocity = Vector3.zero;
        }
    }
}
