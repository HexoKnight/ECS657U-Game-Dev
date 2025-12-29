using UnityEngine;
using UnityEngine.InputSystem;

using GUP.Core.Config;
using KinematicCharacterController;

namespace GUP.Gameplay.Player
{
    /// <summary>
    /// Context object containing all references needed by player state machine states.
    /// Provides access to components, configs, and runtime values without direct coupling.
    /// </summary>
    public class PlayerContext
    {
        #region Component References
        
        /// <summary>The PlayerController MonoBehaviour</summary>
        public PlayerController Controller { get; }
        
        /// <summary>Transform component (cached for performance)</summary>
        public Transform Transform { get; }
        
        /// <summary>Kinematic Character Controller motor</summary>
        public KinematicCharacterMotor Motor { get; }
        
        /// <summary>PlayerInputs wrapper for input access</summary>
        public PlayerInputs Input { get; }
        
        /// <summary>UnityEngine PlayerInput component</summary>
        public PlayerInput PlayerInput { get; }
        
        #endregion

        #region Configuration
        
        /// <summary>Optional movement config ScriptableObject</summary>
        public PlayerMovementConfig Config { get; }
        
        #endregion

        #region Runtime State (cached for cross-state sharing)
        
        /// <summary>Current velocity (updated by states)</summary>
        public Vector3 CurrentVelocity { get; set; }
        
        /// <summary>Whether player is currently grounded</summary>
        public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;
        
        /// <summary>Current up direction for orientation</summary>
        public Vector3 TargetUp { get; set; } = Vector3.up;
        
        /// <summary>Camera target for look rotation</summary>
        public Transform CinemachineTarget { get; }
        
        #endregion

        #region Jump Buffer State (persists across state changes)
        
        /// <summary>Time since jump was requested (for input buffering)</summary>
        public float TimeSinceJumpRequested { get; set; } = float.PositiveInfinity;
        
        /// <summary>Time since player was last able to jump (for coyote time)</summary>
        public float TimeSinceLastAbleToJump { get; set; } = 0f;
        
        /// <summary>Whether the current jump has been consumed</summary>
        public bool JumpConsumed { get; set; } = false;
        
        #endregion

        #region Constructor
        
        public PlayerContext(
            PlayerController controller,
            KinematicCharacterMotor motor,
            PlayerInputs input,
            PlayerInput playerInput,
            PlayerMovementConfig config,
            Transform cinemachineTarget)
        {
            Controller = controller;
            Transform = controller.transform;
            Motor = motor;
            Input = input;
            PlayerInput = playerInput;
            Config = config;
            CinemachineTarget = cinemachineTarget;
        }
        
        #endregion

        #region Helpers
        
        /// <summary>Check if current control scheme is keyboard/mouse</summary>
        public bool IsMouseKeyboard => PlayerInput.currentControlScheme == "KeyboardMouse";
        
        /// <summary>Main camera for look direction calculations</summary>
        public Camera MainCamera => Camera.main;
        
        #endregion
    }
}
