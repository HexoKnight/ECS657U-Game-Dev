using UnityEngine;

namespace GUP.Core.Config
{
    /// <summary>
    /// Configuration asset for player movement parameters.
    /// Create instances via Assets > Create > GUP/Config/Player Movement.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "GUP/Config/Player Movement")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("Ground Movement")]
        [Tooltip("Move speed on ground in m/s")]
        public float maxGroundMoveSpeed = 4f;
        
        [Tooltip("Sprint speed multiplier")]
        public float sprintSpeedMultiplier = 1.5f;
        
        [Tooltip("Rate at which speed changes on ground")]
        public float groundSpeedChangeRate = 10f;
        
        [Header("Swimming")]
        [Tooltip("Maximum swim speed in m/s")]
        public float maxSwimSpeed = 6f;
        
        [Tooltip("Vertical swim speed multiplier")]
        public float verticalSwimMultiplier = 0.5f;
        
        [Tooltip("Rate at which speed changes while swimming")]
        public float swimSpeedChangeRate = 5f;
        
        [Tooltip("Drag applied while swimming")]
        public float swimDrag = 2f;
        
        [Header("Jumping")]
        [Tooltip("Jump height in meters")]
        public float jumpHeight = 1.2f;
        
        [Tooltip("Time required between jumps")]
        public float jumpCooldown = 0.1f;
        
        [Header("Gravity")]
        [Tooltip("Gravity multiplier when grounded (stick to ground)")]
        public float groundedGravity = -2f;
        
        [Tooltip("Gravity multiplier when falling")]
        public float fallingGravity = -15f;
        
        [Header("Camera")]
        [Tooltip("Rotation speed for looking around")]
        public float rotationSpeed = 1.0f;
        
        [Tooltip("Maximum pitch angle (looking up)")]
        public float topClamp = 89f;
        
        [Tooltip("Minimum pitch angle (looking down)")]
        public float bottomClamp = -89f;
        
        [Header("Magnetic Walking")]
        [Tooltip("Max angle the player can walk on magnetically")]
        public float maxMagneticAngle = 80f;
        
        [Tooltip("Distance to check for magnetic surfaces")]
        public float magneticProbeDistance = 0.5f;
    }
}
