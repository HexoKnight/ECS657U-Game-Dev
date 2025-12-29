using UnityEngine;
using GUP.Core.Debug;
using GUP.Core.StateMachine;

namespace GUP.Gameplay.Player.States
{
    /// <summary>
    /// State acting as the "Normal" mode (Free Control).
    /// Wraps the standard movement, rotation, and camera logic exposed by PlayerController.
    /// Handles both ground walking, swimming, and magnetic wall walking (unified kernel).
    /// </summary>
    public class FreeControlState : PlayerState
    {
        public override string StateName => "FreeControl";

        public FreeControlState(IStateMachine stateMachine, PlayerContext context) 
            : base(stateMachine, context)
        {
        }

        public override void Execute()
        {
            base.Execute();
        }

        public override void LateExecute()
        {
            base.LateExecute();
            // Update camera every frame (LateUpdate loop for smoothness)
            Ctx.Controller.UpdateFreeCamera();
        }

        // Note: Movement and Rotation are handled via direct callbacks from PlayerController
        // because KCC requires specific update phases (UpdateRotation, UpdateVelocity).
        // The StateMachine.Update/FixedUpdate are not enough because KCC drives the loop.
        
        // We will expose specific methods for the Controller to call:
        
        public override void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            Ctx.Controller.UpdateFreeRotation(ref currentRotation, deltaTime);
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Ctx.Controller.UpdateFreeMovement(ref currentVelocity, deltaTime);
        }

        public override void OnAfterCharacterUpdate(float deltaTime)
        {
            // Handle jump input buffer expiry (pre-ground grace period)
            // If jump was requested too long ago, clear the request
            if (Ctx.Input.jump && Ctx.TimeSinceJumpRequested > Ctx.Controller.JumpPreGroundingGraceTime)
            {
                Ctx.Input.jump = false;
            }
        }
    }
}
