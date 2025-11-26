using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

using KinematicCharacterController;

// lots of the initial code adapted from ExampleCharacterController in:
// https://assetstore.unity.com/packages/tools/physics/kinematic-character-controller-99131

public delegate bool CalcForceField(Vector3 position, Vector3 velocity, out Vector3 force, float deltaTime);

[RequireComponent(typeof(KinematicCharacterMotor))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour, ICharacterController
{
    enum PlayerState
    {
        Normal,
        StaticSpline,
    }

    [Header("Player")]
    [Tooltip("Move speed of the character on the ground in m/s")]
    public float maxGroundMoveSpeed = 4f;
    [Tooltip("How much faster the character is when sprinting")]
    public float sprintSpeedMultiplier = 1.5f;
    [Tooltip("Move speed of the character in the air in m/s")]
    public float maxAirMoveSpeed = 4f;
    [Tooltip("Acceleration and deceleration when grounded")]
    public float groundAcceleration = 10f;
    [Tooltip("Acceleration and deceleration when in the air")]
    public float airAcceleration = 10f;
    [Tooltip("Drag experienced by the player")]
    public float drag = 1f;

    [Tooltip("Coyote time (time after leaving the ground during which you can still jump)")]
    public float JumpPostGroundingGraceTime = 0f;
    [Tooltip("Reverse Coyote time (time before landing on the ground during which you can queue a jump)")]
    public float JumpPreGroundingGraceTime = 0f;
    [Tooltip("The amount (relative to the current max movement speed) to boost the character in the direction of movement when they jump")]
    public float JumpBoostSpeed = 1f;

    [Space(10)]
    [Tooltip("The force with which the player jumps")]
    public float jumpForce = 9f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15f;
    [Tooltip("If the character uses gravity")]
    public bool useGravity = true;
    [Tooltip("The y-value, below which the scene will reload (ie. game restart)")]
    public float deadZone = -15;

    [Tooltip("If the character can jump while they are sliding down a steep slope")]
    public bool allowJumpingWhenSliding = true;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public Transform cinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    [Range(0, 90)]
    public float topClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [Range(-90, 0)]
    public float bottomClamp = -90.0f;

    [Header("Other")]
    [Tooltip("The world space up vector the player is currently trying to achieve")]
    public Vector3 targetUp = Vector3.up;
    [Tooltip("The speed at which the player aligns to the target up")]
    public float alignUpSpeed = 12f;

    [Header("Internal")]

    // cinemachine
    private float _cinemachineTargetPitch;

    // player state
    private PlayerState _playerState = PlayerState.Normal;

    // player state - normal
    private bool _jumpedThisFrame = false;
    private bool _jumpConsumed = false;
    private float _timeSinceJumpRequested = float.PositiveInfinity;
    private float _timeSinceLastAbleToJump = 0f;

    // player state - static spline
    private SplineContainer _currentPath;
    private System.Action _pathFinishCallback;
    private float _pathSpeed;
    private float _distanceAlongPath;

    private readonly HashSet<CalcForceField> _forceFields = new();

    // components
    private KinematicCharacterMotor _motor;
    private PlayerInput _playerInput;
    private PlayerInputs _input;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    public void StartStaticSplineStart(SplineContainer splineContainer, float speed, System.Action finishCallback)
    {
        _currentPath = splineContainer;
        _pathFinishCallback = finishCallback;
        _pathSpeed = speed;
        _distanceAlongPath = 0;

        _playerState = PlayerState.StaticSpline;
    }

    public void ResetNormalState()
    {
        _jumpedThisFrame = false;
        _jumpConsumed = false;
        _timeSinceJumpRequested = float.PositiveInfinity;
        _timeSinceLastAbleToJump = 0f;

        _playerState = PlayerState.Normal;
    }

    public void EnterForceFieldRange(CalcForceField forceField)
    {
        _forceFields.Add(forceField);
    }

    public void ExitForceFieldRange(CalcForceField forceField)
    {
        _forceFields.Remove(forceField);
    }

    // INTERNAL

    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();

        _motor.CharacterController = this;
    }

    private void FixedUpdate()
    {
        DeadZoneCheck();

        switch (_playerState)
        {
            case PlayerState.StaticSpline:
                float pathLength = _currentPath.CalculateLength();

                _distanceAlongPath = System.Math.Min(pathLength, _distanceAlongPath + _pathSpeed * Time.deltaTime);

                float ratio = _distanceAlongPath / pathLength;

                _currentPath.Evaluate(ratio, out var position, out var tangent, out var upVector);

                Quaternion rotation = Quaternion.LookRotation(tangent, upVector);

                _motor.SetPositionAndRotation(position, rotation, false);

                if (ratio == 1f)
                {
                    _pathFinishCallback();
                    ResetNormalState();
                }

                break;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void DeadZoneCheck()
    {
        if (transform.position.y < deadZone)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void CameraRotation()
    {
        switch (_playerState)
        {
            case PlayerState.Normal:
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * Options.mouseSensitivity * deltaTimeMultiplier;
                float yawVelocity = _input.look.x * Options.mouseSensitivity * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

                // Update Cinemachine camera target pitch
                cinemachineCameraTarget.localRotation = Quaternion.Euler(_cinemachineTargetPitch, cinemachineCameraTarget.localRotation.eulerAngles.y + yawVelocity, 0.0f);

                break;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    /// this function is the only place where rotation can be changed
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        switch (_playerState)
        {
            case PlayerState.Normal:
                Vector3 cameraEulerAngles = cinemachineCameraTarget.localRotation.eulerAngles;
                currentRotation *= Quaternion.Euler(0, cameraEulerAngles.y, 0);
                cameraEulerAngles.y = 0;
                cinemachineCameraTarget.localRotation = Quaternion.Euler(cameraEulerAngles);

                Vector3 currentUp = currentRotation * Vector3.up;
                Vector3 smoothedUp = Vector3.Slerp(currentUp, targetUp, 1 - Mathf.Exp(-alignUpSpeed * deltaTime));
                currentRotation = Quaternion.FromToRotation(currentUp, smoothedUp) * currentRotation;

                break;
        }
    }

    /// this function is the only place where velocity can be changed
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        switch (_playerState)
        {
            case PlayerState.Normal:
                bool anyForceFieldActive = false;
                Vector3 totalForce = Vector3.zero;
                foreach (CalcForceField calcForceField in _forceFields)
                {
                    bool forceFieldActive = calcForceField(_motor.InitialSimulationPosition, currentVelocity, out Vector3 force, deltaTime);
                    if (forceFieldActive)
                    {
                        anyForceFieldActive = true;
                        totalForce += force;
                    }
                }

                // assume a mass of 1 (ie. force == acceleration)
                currentVelocity += totalForce * deltaTime;

                if (_motor.GroundingStatus.IsStableOnGround && Vector3.Dot(totalForce, _motor.GroundingStatus.GroundNormal) > 0.1f) _motor.ForceUnground();

                Vector3 moveInputVector = transform.TransformVector(new Vector3(_input.move.x, 0, _input.move.y));
                if (!_input.analogMovement) moveInputVector.Normalize();

                float currentMaxGroundSpeed = moveInputVector.magnitude * maxGroundMoveSpeed;
                if (_input.sprint) currentMaxGroundSpeed *= sprintSpeedMultiplier;

                // Ground movement
                if (_motor.GroundingStatus.IsStableOnGround)
                {
                    float currentVelocityMagnitude = currentVelocity.magnitude;

                    Vector3 effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;

                    // Reorient velocity on slope
                    currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

                    // Calculate target velocity
                    Vector3 inputRight = Vector3.Cross(moveInputVector, _motor.CharacterUp);
                    Vector3 reorientedForward = Vector3.Cross(effectiveGroundNormal, inputRight).normalized;
                    Vector3 targetMovementVelocity = reorientedForward * currentMaxGroundSpeed;

                    // Smooth movement Velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-groundAcceleration * deltaTime));
                }
                // Air movement
                else
                {
                    // Add move input
                    if (moveInputVector.sqrMagnitude > 0f)
                    {
                        Vector3 addedVelocity = moveInputVector * airAcceleration * deltaTime;

                        Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp);

                        // Limit air velocity from inputs
                        if (currentVelocityOnInputsPlane.magnitude < maxAirMoveSpeed)
                        {
                            // clamp addedVel to make total vel not exceed max vel on inputs plane
                            Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, maxAirMoveSpeed);
                            addedVelocity = newTotal - currentVelocityOnInputsPlane;
                        }
                        else
                        {
                            // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                            if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                            {
                                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                            }
                        }

                        // Prevent air-climbing sloped walls
                        if (_motor.GroundingStatus.FoundAnyGround)
                        {
                            if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                            {
                                Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(_motor.CharacterUp, _motor.GroundingStatus.GroundNormal), _motor.CharacterUp).normalized;
                                addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                            }
                        }

                        // Apply added velocity
                        currentVelocity += addedVelocity;
                    }

                    // Gravity
                    // only use gravity if there are no active force fields
                    if (useGravity && !anyForceFieldActive) currentVelocity += _motor.CharacterUp * gravity * deltaTime;

                    // Drag
                    currentVelocity *= 1f / (1f + (drag * deltaTime));
                }

                // Handle jumping
                _jumpedThisFrame = false;
                _timeSinceJumpRequested += deltaTime;
                if (_input.jump)
                {
                    // See if we actually are allowed to jump
                    if (!_jumpConsumed && ((allowJumpingWhenSliding ? _motor.GroundingStatus.FoundAnyGround : _motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                    {
                        // Calculate jump direction before ungrounding
                        Vector3 jumpDirection = _motor.CharacterUp;
                        if (_motor.GroundingStatus.FoundAnyGround && !_motor.GroundingStatus.IsStableOnGround)
                        {
                            jumpDirection = _motor.GroundingStatus.GroundNormal;
                        }

                        // Makes the character skip ground probing/snapping on its next update.
                        // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                        _motor.ForceUnground();

                        // Add to the return velocity and reset jump state
                        currentVelocity += (jumpDirection * jumpForce) - Vector3.Project(currentVelocity, _motor.CharacterUp);
                        currentVelocity += moveInputVector.normalized * JumpBoostSpeed * currentMaxGroundSpeed;
                        _input.jump = false;
                        _jumpConsumed = true;
                        _jumpedThisFrame = true;
                    }
                }

                // // Take into account additive velocity
                // if (_internalVelocityAdd.sqrMagnitude > 0f)
                // {
                //     currentVelocity += _internalVelocityAdd;
                //     _internalVelocityAdd = Vector3.zero;
                // }

                break;
            case PlayerState.StaticSpline:
                currentVelocity = Vector3.zero;
                break;
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        switch (_playerState)
        {
            case PlayerState.Normal:
                // Handle jump-related values
                {
                    // Handle jumping pre-ground grace period
                    if (_input.jump && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                    {
                        _input.jump = false;
                    }

                    if (allowJumpingWhenSliding ? _motor.GroundingStatus.FoundAnyGround : _motor.GroundingStatus.IsStableOnGround)
                    {
                        // If we're on a ground surface, reset jumping values
                        if (!_jumpedThisFrame)
                        {
                            _jumpConsumed = false;
                        }
                        _timeSinceLastAbleToJump = 0f;
                    }
                    else
                    {
                        // Keep track of time since we were last able to jump (for grace period)
                        _timeSinceLastAbleToJump += deltaTime;
                    }
                }

                // Handle uncrouching
                // if (_isCrouching && !_shouldBeCrouching)
                // {
                //     // Do an overlap test with the character's standing height to see if there are any obstructions
                //     _motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                //     if (_motor.CharacterOverlap(
                //         _motor.TransientPosition,
                //         _motor.TransientRotation,
                //         _probedColliders,
                //         _motor.CollidableLayers,
                //         QueryTriggerInteraction.Ignore) > 0)
                //     {
                //         // If obstructions, just stick to crouching dimensions
                //         _motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                //     }
                //     else
                //     {
                //         // If no obstructions, uncrouch
                //         MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                //         _isCrouching = false;
                //     }
                // }

                break;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
}