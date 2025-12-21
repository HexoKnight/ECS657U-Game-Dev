using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

using GUP.Core;
using GUP.Core.Config;
using GUP.Core.Debug;
using GUP.Core.StateMachine;
using GUP.Gameplay.Player;
using GUP.Gameplay.Player.States;
using KinematicCharacterController;

// lots of the initial code adapted from ExampleCharacterController in:
// https://assetstore.unity.com/packages/tools/physics/kinematic-character-controller-99131

public delegate bool CalcForceField(Vector3 position, Vector3 velocity, out Vector3 force, float deltaTime);

[RequireComponent(typeof(KinematicCharacterMotor))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour, ICharacterController, IDamageable
{
    private StateMachineBase<PlayerContext> _stateMachine;
    private PlayerContext _playerContext;
    private AliveState _aliveState;

    [Header("Configuration")]
    [Tooltip("Optional: Movement config ScriptableObject. If not assigned, uses local serialized values.")]
    [SerializeField] private PlayerMovementConfig movementConfig;

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
    [Tooltip("Rotation speed of the camera")]
    public float rotationSpeed = 1f;

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

    [Header("Health")]
    [Tooltip("Maximum health of the player")]
    public float maxHealth = 3f;

    [Tooltip("Time in seconds during which the player is invulnerable after taking damage")]
    public float invulnerabilityTime = 0.5f;

    // current health
    private float _currentHealth;
    private bool _isInvulnerable;
    private float _invulnerabilityTimer;

    [Header("Internal")]
    // cinemachine
    private float _cinemachineTargetPitch;

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

    // external velocity used for knockback / pushes
    private Vector3 _externalVelocityAdd = Vector3.zero;

    private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    // ------------- PUBLIC API (STATIC SPLINE) -------------

    public void StartStaticSplineStart(SplineContainer splineContainer, float speed, System.Action finishCallback)
    {
        _currentPath = splineContainer;
        _pathFinishCallback = finishCallback;
        _pathSpeed = speed;
        _distanceAlongPath = 0f;
        
        _aliveState.TransitionToPathFollow();
    }

    public void ResetNormalState()
    {
        // NOTE: Do NOT reset jump timers here - they should persist across state changes
        // to allow buffered jumps during transitions (e.g., magnet detach → free control)
        _aliveState.TransitionToFreeControl();
    }

    public void EnterForceFieldRange(CalcForceField forceField)
    {
        _forceFields.Add(forceField);
    }

    public void ExitForceFieldRange(CalcForceField forceField)
    {
        _forceFields.Remove(forceField);
    }

// ------------- HEALTH / DAMAGE API -------------
/// <summary>
/// Entity type for cross-assembly identification.
/// </summary>
public EntityType EntityType => EntityType.Player;

/// <summary>
/// Implementation of IDamageable. Enemies/hazards should call this to damage the player.
/// </summary>
public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
{
    if (amount <= 0f)
        return;

    if (_isInvulnerable)
        return;

    _currentHealth -= amount;
    
    // Debug log damage
    GupDebug.LogDamageTaken(gameObject.name, amount, _currentHealth);
    
    // Broadcast damage event
    GameEvents.RaiseEntityDamaged(gameObject, DamageData.Simple(amount, hitPoint, hitNormal));

    if (_currentHealth <= 0f)
    {
        // Debug log death
        GupDebug.LogDeath(gameObject.name, "HP depleted");
        
        // On death, respawn or reload scene.
        GameEvents.RaiseEntityDied(gameObject);
        RespawnPlayer();
    }
    else
    {
        // Temporary invulnerability window
        _isInvulnerable = true;
        _invulnerabilityTimer = invulnerabilityTime;

        // Simple knockback away from the hit point / normal
        Vector3 knockbackDir = (transform.position - hitPoint).normalized;
        if (knockbackDir.sqrMagnitude < 0.01f)
        {
            // fallback if hitPoint == player position
            knockbackDir = -hitNormal.normalized;
        }

        float knockbackStrength = 8f; // puedes tunearlo o exponerlo como public
        AddVelocity(knockbackDir * knockbackStrength);
    }
}

/// <summary>
/// Implementation of IDamageable with DamageData struct.
/// </summary>
public void TakeDamage(DamageData damage)
{
    TakeDamage(damage.Amount, damage.HitPoint, damage.HitNormal);
    
    // Apply knockback from DamageData if specified
    if (damage.KnockbackForce > 0f)
    {
        Vector3 knockbackDir = (transform.position - damage.HitPoint).normalized;
        if (knockbackDir.sqrMagnitude < 0.01f)
        {
            knockbackDir = -damage.HitNormal.normalized;
        }
        AddVelocity(knockbackDir * damage.KnockbackForce);
    }
}

/// <summary>
/// Get current health value.
/// </summary>
public float GetCurrentHealth() => _currentHealth;

/// <summary>
/// Get maximum health value.
/// </summary>
public float GetMaxHealth() => maxHealth;

/// <summary>
/// Check if player is dead (health <= 0).
/// </summary>
public bool IsDead() => _currentHealth <= 0f;

/// <summary>
/// Heal the player by the specified amount.
/// </summary>
public void Heal(float amount)
{
    if (amount <= 0f) return;
    _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
}

    /// <summary>
    /// Allows external systems (enemies, currents, etc.) to add an instantaneous velocity,
    /// e.g. for knockback after damage.
    /// </summary>
    public void AddVelocity(Vector3 velocity)
    {
        _externalVelocityAdd += velocity;
    }

    /// <summary>
    /// Respawn or restart the level.
    /// For now, this simply reloads the current scene.
    /// Later this can be wired to CheckpointManager if needed.
    /// </summary>
    private void RespawnPlayer()
    {
        // Reset health first to prevent re-triggering death
        _currentHealth = maxHealth;
        _isInvulnerable = true; // Temporary invulnerability after respawn
        _invulnerabilityTimer = 2f; // 2 seconds of invulnerability
        
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            
            // Reset velocity
            if (_motor != null)
            {
                _motor.BaseVelocity = Vector3.zero;
            }
            _externalVelocityAdd = Vector3.zero;
            
            // Debug log respawn
            GupDebug.LogRespawn(gameObject.name, transform.position, "Checkpoint");
            
            // Raise respawn event
            GameEvents.RaisePlayerRespawned(transform.position);
        }
        else
        {
            GupDebug.Log(LogCategory.Respawn, "No CheckpointManager, reloading scene", LogLevel.Warn);
            // Simple behaviour: reload scene
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }

    // ------------- UNITY / CHARACTER CONTROLLER LIFECYCLE -------------

    private void Awake()
    {
        _motor = GetComponent<KinematicCharacterMotor>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();

        _motor.CharacterController = this;

        // initialise health
        _currentHealth = maxHealth;
        _isInvulnerable = false;
        _invulnerabilityTimer = 0f;
        
        // Init HFSM
        _playerContext = new PlayerContext(
            this,
            _motor,
            _input,
            _playerInput,
            movementConfig, 
            cinemachineCameraTarget
        );
        
        _stateMachine = new StateMachineBase<PlayerContext>(_playerContext);
        _aliveState = new AliveState(_stateMachine, _playerContext);
        
        // Set initial state
        _stateMachine.SetStateImmediate(_aliveState);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _aliveState.Enter();
    }

    private void Update()
    {
        // update invulnerability timer
        if (_isInvulnerable)
        {
            _invulnerabilityTimer -= Time.deltaTime;
            if (_invulnerabilityTimer <= 0f)
            {
                _isInvulnerable = false;
                _invulnerabilityTimer = 0f;
            }
        }
        
        // HFSM Update (Transition Checks + Execute)
        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        DeadZoneCheck();
        
        // HFSM FixedUpdate
        _stateMachine.FixedUpdate();
    }

    private void LateUpdate()
    {
        _stateMachine.LateUpdate();
    }

    private void DeadZoneCheck()
    {
        if (transform.position.y < deadZone)
        {
            RespawnPlayer();
        }
    }


    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    /// this function is the only place where rotation can be changed
    /// this function is the only place where rotation can be changed
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_stateMachine.CurrentState is PlayerState playerState)
        {
            playerState.OnUpdateRotation(ref currentRotation, deltaTime);
        }
    }

    /// this function is the only place where velocity can be changed
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_stateMachine.CurrentState is PlayerState playerState)
        {
            playerState.OnUpdateVelocity(ref currentVelocity, deltaTime);
        }
    }

    // ------------- HFSM KERNELS -------------
    
    /// <summary>Kernel for free movement (was Normal state logic)</summary>
    public void UpdateFreeMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        bool anyForceFieldActive = false;
        Vector3 totalForce = Vector3.zero;
        foreach (CalcForceField calcForceField in _forceFields)
        {
            bool forceFieldActive = calcForceField(_motor.InitialSimulationPosition,
                currentVelocity,
                out Vector3 force,
                deltaTime);
            if (forceFieldActive)
            {
                anyForceFieldActive = true;
                totalForce += force;
            }
        }

        // assume a mass of 1 (ie. force == acceleration)
        currentVelocity += totalForce * deltaTime;

        if (_motor.GroundingStatus.IsStableOnGround &&
            Vector3.Dot(totalForce, _motor.GroundingStatus.GroundNormal) > 0.1f)
            _motor.ForceUnground();

        Vector3 moveInputVector =
            transform.TransformVector(new Vector3(_input.move.x, 0, _input.move.y));
        if (!_input.analogMovement) moveInputVector.Normalize();

        float currentMaxGroundSpeed = moveInputVector.magnitude * maxGroundMoveSpeed;
        if (_input.sprint) currentMaxGroundSpeed *= sprintSpeedMultiplier;

        // Ground movement
        if (_motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;

            Vector3 effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;

            // Reorient velocity on slope
            currentVelocity =
                _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                currentVelocityMagnitude;

            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(moveInputVector, _motor.CharacterUp);
            Vector3 reorientedForward =
                Vector3.Cross(effectiveGroundNormal, inputRight).normalized;
            Vector3 targetMovementVelocity = reorientedForward * currentMaxGroundSpeed;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity,
                targetMovementVelocity,
                1f - Mathf.Exp(-groundAcceleration * deltaTime));
        }
        // Air movement
        else
        {
            // Add move input
            if (moveInputVector.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = moveInputVector * airAcceleration * deltaTime;

                Vector3 currentVelocityOnInputsPlane =
                    Vector3.ProjectOnPlane(currentVelocity, _motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < maxAirMoveSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal =
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity,
                            maxAirMoveSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity,
                            currentVelocityOnInputsPlane.normalized);
                    }
                }

                // Prevent air-climbing sloped walls
                if (_motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal =
                            Vector3.Cross(
                                    Vector3.Cross(_motor.CharacterUp,
                                        _motor.GroundingStatus.GroundNormal),
                                    _motor.CharacterUp)
                                .normalized;
                        addedVelocity =
                            Vector3.ProjectOnPlane(addedVelocity,
                                perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }

            // Gravity
            // only use gravity if there are no active force fields
            if (useGravity && !anyForceFieldActive)
                currentVelocity += _motor.CharacterUp * gravity * deltaTime;

            // Drag
            currentVelocity *= 1f / (1f + (drag * deltaTime));
        }

        // Handle jumping (using context timers for persistence across state changes)
        _playerContext.TimeSinceJumpRequested += deltaTime;
        if (_input.jump)
        {
            // See if we actually are allowed to jump
            if (!_playerContext.JumpConsumed &&
                ((allowJumpingWhenSliding
                      ? _motor.GroundingStatus.FoundAnyGround
                      : _motor.GroundingStatus.IsStableOnGround) ||
                 _playerContext.TimeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
            {
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = _motor.CharacterUp;
                if (_motor.GroundingStatus.FoundAnyGround &&
                    !_motor.GroundingStatus.IsStableOnGround)
                {
                    jumpDirection = _motor.GroundingStatus.GroundNormal;
                }

                // Makes the character skip ground probing/snapping on its next update.
                _motor.ForceUnground();

                // Add to the return velocity and reset jump state
                currentVelocity += (jumpDirection * jumpForce) -
                                   Vector3.Project(currentVelocity, _motor.CharacterUp);
                currentVelocity += moveInputVector.normalized * JumpBoostSpeed *
                                   currentMaxGroundSpeed;
                _input.jump = false;
                _playerContext.JumpConsumed = true;
            }
        }

        // Apply external velocity (knockback, pushes, etc.)
        if (_externalVelocityAdd.sqrMagnitude > 0f)
        {
            currentVelocity += _externalVelocityAdd;
            _externalVelocityAdd = Vector3.zero;
        }
    }

    /// <summary>Kernel for free rotation (was Normal state logic)</summary>
    public void UpdateFreeRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 cameraEulerAngles = cinemachineCameraTarget.localRotation.eulerAngles;
        currentRotation *= Quaternion.Euler(0, cameraEulerAngles.y, 0);
        cameraEulerAngles.y = 0;
        cinemachineCameraTarget.localRotation = Quaternion.Euler(cameraEulerAngles);

        Vector3 currentUp = currentRotation * Vector3.up;
        Vector3 smoothedUp =
            Vector3.Slerp(currentUp, targetUp, 1 - Mathf.Exp(-alignUpSpeed * deltaTime));
        currentRotation = Quaternion.FromToRotation(currentUp, smoothedUp) * currentRotation;
    }

    /// <summary>Kernel for free camera (was Normal state logic)</summary>
    public void UpdateFreeCamera()
    {
        // Don't multiply mouse input by Time.deltaTime
        float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

        _cinemachineTargetPitch += _input.look.y * rotationSpeed * deltaTimeMultiplier;
        float yawVelocity = _input.look.x * rotationSpeed * deltaTimeMultiplier;

        // clamp our pitch rotation
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

        // Update Cinemachine camera target pitch
        cinemachineCameraTarget.localRotation =
            Quaternion.Euler(_cinemachineTargetPitch,
                cinemachineCameraTarget.localRotation.eulerAngles.y + yawVelocity,
                0.0f);
    }
    
    /// <summary>Kernel for path following (StaticSpline logic)</summary>
    /// <returns>True if path finished this frame</returns>
    public bool UpdatePathFollowing(float deltaTime)
    {
        if (_currentPath == null) return true; // No path, consider finished
        
        float pathLength = _currentPath.CalculateLength();
        if (pathLength <= 0f) return true; // Invalid path
        
        _distanceAlongPath += _pathSpeed * deltaTime;
        
        // Clamp and check for end-of-path
        float ratio = Mathf.Clamp01(_distanceAlongPath / pathLength);
        const float endEpsilon = 0.001f;
        
        // End-of-path: finalize BEFORE evaluating last frame to avoid stuck state
        if (ratio >= 1f - endEpsilon)
        {
            GupDebug.LogPathComplete(gameObject.name);
            _pathFinishCallback?.Invoke();
            _pathFinishCallback = null;
            _currentPath = null;
            
            // Transition to FreeControl immediately
            _aliveState.TransitionToFreeControl();
            return true;
        }
        
        // Evaluate spline
        _currentPath.Evaluate(ratio, out var position, out var tangent, out var upVector);
        
        // Guard against zero tangent (LookRotation would error)
        Vector3 safeForward = tangent;
        if (safeForward.sqrMagnitude < 1e-6f)
        {
            safeForward = _motor.CharacterForward; // Fallback to current forward
        }
        else
        {
            safeForward = safeForward.normalized;
        }
        
        // Guard against zero up vector
        Vector3 safeUp = upVector;
        if (safeUp.sqrMagnitude < 1e-6f)
        {
            safeUp = _motor.CharacterUp; // Fallback to current up
        }
        else
        {
            safeUp = safeUp.normalized;
        }
        
        // Ensure forward and up aren't parallel
        if (Mathf.Abs(Vector3.Dot(safeForward, safeUp)) > 0.999f)
        {
            safeUp = Vector3.up; // Fallback to world up
        }
        
        Quaternion rotation = Quaternion.LookRotation(safeForward, safeUp);
        _motor.SetPositionAndRotation(position, rotation, false);
        
        return false;
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Update coyote timer regardless of state (persists across state changes)
        bool canJumpFromGround = allowJumpingWhenSliding
            ? _motor.GroundingStatus.FoundAnyGround
            : _motor.GroundingStatus.IsStableOnGround;

        if (canJumpFromGround)
        {
            // On ground: reset coyote timer and allow new jump
            _playerContext.TimeSinceLastAbleToJump = 0f;
            
            // Only reset JumpConsumed if grounded (allows re-jump after landing)
            // Note: ForceUnground is called when jumping, so this won't fire same frame
            _playerContext.JumpConsumed = false;
        }
        else
        {
            // Not grounded: accumulate coyote time
            _playerContext.TimeSinceLastAbleToJump += deltaTime;
        }

        // Delegate to active state for state-specific post-update logic
        if (_stateMachine.CurrentState is PlayerState playerState)
        {
            playerState.OnAfterCharacterUpdate(deltaTime);
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider,
        Vector3 hitNormal,
        Vector3 hitPoint,
        Vector3 atCharacterPosition,
        Quaternion atCharacterRotation,
        ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
}
