using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// based initially off `FirstPersonController.cs` from:
// https://assetstore.unity.com/packages/essentials/starter-assets-firstperson-updates-in-new-charactercontroller-pa-196525

// TODO:
// - magnetic platform integration
// - steps
// - slopes

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float moveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float sprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float rotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float speedChangeRate = 10.0f;
	[Tooltip("The relative acceleration and deceleration when not grounded")]
	public float airSpeedChangeRateRatio = 0.1f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float jumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float gravity = -15.0f;
	[Tooltip("The terminal velocity of the character in m/s")]
	public float terminalVelocity = 53.0f;
	[Tooltip("The y-value, below which the scene will reload (ie. game restart)")]
	public float deadZone = -15;
	[Tooltip("If the character uses gravity")]
	public bool useGravity = true;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float fallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool grounded = true;
	[Tooltip("Useful for rough ground")]
	public float groundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	[Min(0)]
	public float groundedRadius = 0.5f;
	[Tooltip("What layers the character uses as ground")]
	public LayerMask groundLayers;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public Transform cinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	[Range(0, 90)]
	public float topClamp = 90.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	[Range(-90, 0)]
	public float bottomClamp = -90.0f;

	[Header("Collision")]
	[Tooltip("The rigidbody used for collision")]
	public Rigidbody collisionRigidbody;
	[Tooltip("Tolerance for the collision checking")]
	[Min(0)]
	public float tolerance;

	[Header("Debug")]
	[Tooltip("Display collision gizmos for debugging")]
	public bool collisionGizmos;
	[Tooltip("The distance the collision gizmo projects")]
	[Min(0)]
	public float collisionGizmoDistance = 1;
	[Tooltip("Whether or not to use the current velocity for the projection gizmo")]
	public bool useVelocityForCollisionGizmo;

	[Header("Internal")]

	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _rotationVelocity;
	[SerializeField]
	private Vector3 _velocity;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;


	private PlayerInput _playerInput;
	private PlayerInputs _input;

	private const float _threshold = 0.01f;

	private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

	// use transform position as that updates instantly to parent transform changes
	// while the rigidbody position only updates after a physics step
	private Vector3 ColliderPosition => collisionRigidbody.transform.position;

	private void Start()
	{
		_input = GetComponent<PlayerInputs>();
		_playerInput = GetComponent<PlayerInput>();

		// reset our timeouts on start
		_jumpTimeoutDelta = jumpTimeout;
		_fallTimeoutDelta = fallTimeout;
	}

	private void FixedUpdate()
	{
		GroundedCheck();
		JumpAndGravity();
		Move();
		DeadZoneCheck();
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

	Vector3 GroundSpherePosition => ColliderPosition - collisionRigidbody.transform.up * groundedOffset;

	private void GroundedCheck()
	{
		// set sphere position, with offset
		grounded = Physics.CheckSphere(GroundSpherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
	}

	private void CameraRotation()
	{
		// if there is an input
		if (_input.look.sqrMagnitude >= _threshold)
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

			_cinemachineTargetPitch += _input.look.y * rotationSpeed * deltaTimeMultiplier;
			_rotationVelocity = _input.look.x * rotationSpeed * deltaTimeMultiplier;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

			// Update Cinemachine camera target pitch
			cinemachineCameraTarget.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

			// rotate the player left and right
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{

		float targetSpeed;
		Vector3 targetDirection;

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		if (_input.move == Vector2.zero)
		{
			targetSpeed = 0.0f;
			// use previous direction if there is no current input
			targetDirection = new Vector3(_velocity.x, 0, _velocity.z).normalized;
		}
		else
		{
			targetSpeed = _input.sprint ? sprintSpeed : moveSpeed;
			targetDirection = new Vector3(_input.move.x, 0, _input.move.y).normalized;
		}

		float currentHorizontalSpeed = new Vector2(_velocity.x, _velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			float finalSpeedChangeRate = speedChangeRate * (grounded ? 1 : airSpeedChangeRateRatio);
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * finalSpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			// snap to speed if within speedOffset
			_speed = targetSpeed;
		}

		// try move the player

		Vector3 moveVector = targetDirection * _speed + new Vector3(0.0f, _velocity.y, 0.0f);

		Vector3 actualMove = TryMove(moveVector * Time.deltaTime);

		transform.position += actualMove;

		_velocity = actualMove / Time.deltaTime;
	}

	private Vector3 TryMove(Vector3 vector)
		=> TryMoveRec(ColliderPosition, vector, vector, 0);

	private Vector3 TryMoveRec(Vector3 startPos, Vector3 vector, Vector3 originalVector, int depth)
	{
		if (depth > 20)
		{
			Debug.Log("too deep!!");
			return Vector3.zero;
		}

		Vector3 direction = vector.normalized;
		float maxDistance = vector.magnitude;

		if (!SweepFromPosWithTolerance(startPos, direction, maxDistance, tolerance, out RaycastHit hitInfo))
			return vector;

		Vector3 toHit = direction * hitInfo.distance;
		Vector3 leftover = vector - toHit;

		// project leftover to give slideVector
		Vector3 slideVector = Vector3.ProjectOnPlane(leftover, hitInfo.normal);

		// give slideVector same magnitude as leftover
		slideVector = slideVector.normalized * leftover.magnitude;

		// scale slideVector according to it's similarity to the original direction of movement
		// clamping at 0 (which occurs with tightly curved corners) to prevent backwards sliding
		slideVector *= System.Math.Max(0, Vector3.Dot(slideVector.normalized, originalVector.normalized));

		return toHit + TryMoveRec(toHit + startPos, slideVector, originalVector, depth + 1);
	}

	private bool SweepFromPosWithTolerance(Vector3 position, Vector3 direction, float maxDistance, float tolerance, out RaycastHit hitInfo)
	{
		Vector3 offset = direction * tolerance;

		Vector3 prevPos = collisionRigidbody.position;
		collisionRigidbody.position = position - offset;
		bool result = collisionRigidbody.SweepTest(direction, out RaycastHit rawHitInfo, maxDistance + tolerance, QueryTriggerInteraction.Ignore);
		collisionRigidbody.position = prevPos;

		rawHitInfo.distance -= tolerance;
		hitInfo = rawHitInfo;

		if (result)
		{
			if (hitInfo.distance > maxDistance) Debug.Log("SweepTest over swept!!: " + hitInfo.distance + " > " + maxDistance);
		}

		return result;
	}

	private void JumpAndGravity()
	{
		if (grounded)
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = fallTimeout;

			// stop our velocity dropping infinitely when grounded
			if (_velocity.y < 0.0f)
			{
				_velocity.y = 0.0f;
			}

			// Jump
			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				_velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = jumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}

			// if we are not grounded, do not jump
			_input.jump = false;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (useGravity && _velocity.y < terminalVelocity)
		{
			_velocity.y += gravity * Time.deltaTime;
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmos()
	{
		if (collisionGizmos)
		{
			Vector3 projectionDirection = useVelocityForCollisionGizmo ? _velocity : transform.forward;
			Vector3 tryMove = ColliderPosition + TryMove(projectionDirection * collisionGizmoDistance);

			MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
			Vector3 newMeshPosition = tryMove + meshFilter.transform.position - ColliderPosition;

			Gizmos.color = Color.red;
			Gizmos.DrawWireMesh(meshFilter.sharedMesh, 0, newMeshPosition, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new(1.0f, 0.0f, 0.0f, 0.35f);

		if (grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(GroundSpherePosition, groundedRadius);
	}
}