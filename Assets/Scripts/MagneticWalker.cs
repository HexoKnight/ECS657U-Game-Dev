using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerController))]
public class MagnetWalker : MonoBehaviour
{
    [Header("Activation")]
    public LayerMask magneticLayers;
    public string magneticTag = "";
    public bool useToggle = true;
    public bool allowSprintAction = true;
    public KeyCode attachKey = KeyCode.E;

    [Header("Robust surface detection")]
    public float detectRadius = 2.5f;
    public float probeRadius = 0.35f;
    public bool queriesHitBackfaces = true;
    [Range(0, 90)] public float minSurfaceTiltFromUp = 25f;
    [Range(0, 90)] public float maxAttachAngle = 80f;

    [Header("Wall locomotion")]
    public float wallMoveSpeed = 4.5f;
    public float stickForce = 34f;
    public float alignUpSlerp = 12f;
    public float inputSmoothing = 12f;
    public float snapOffset = 0.12f;

    [Header("Detach / stability")]
    [Tooltip("How long we can lose the wall before detaching (seconds).")]
    public float lostWallGrace = 0.25f;
    [Range(0, 90)] public float maxNormalChangeToDetach = 55f;
    public bool alignToWorldUpOnDetach = true;

    [Header("Camera (look)")]
    public Vector2 lookSensitivity = new Vector2(1.2f, 1.0f);
    public Vector2 pitchLimits = new Vector2(-80f, 80f);

    [Header("References")]

    [Header("Debug")]
    public bool debugLogging = true;
    public bool drawGizmos = true;

    // ---- internals ----
    private PlayerInputs _inputs;
    private CharacterController _cc;
    private PlayerController _playerController;
    private Transform _cameraTarget;

    private bool _magnetActive, _toggleLatch;
    private Vector2 _smoothedMove;
    private float _yaw, _pitch;

    private Vector3 _currentNormal = Vector3.up, _lastNormal = Vector3.up;
    private Quaternion _savedCamLocalRot;
    private float _savedStepOffset, _savedSlopeLimit;
    private float _lostTimer;

    void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();
        _cc = GetComponent<CharacterController>();

        _playerController = GetComponent<PlayerController>();
        _cameraTarget = _playerController.cinemachineCameraTarget.transform;
    }

    void Update()
    {
        HandleActivation();
        if (_magnetActive) TickMagnet();
    }

    private void HandleActivation()
    {
        bool keyDown = Input.GetKeyDown(attachKey);
        bool sprintPressed = allowSprintAction && _inputs != null && _inputs.sprint;

        if (useToggle)
        {
            if ((keyDown || (sprintPressed && !_toggleLatch)))
            {
                _toggleLatch = true;
                if (_magnetActive) Detach();
                else TryAttach();
            }
            if (_inputs != null && !_inputs.sprint) _toggleLatch = false;
        }
        else
        {
            if ((keyDown || sprintPressed) && !_magnetActive) TryAttach();
            if (!sprintPressed && _magnetActive) Detach();
        }

        if (_magnetActive && _inputs != null && _inputs.jump)
        {
            Detach();
            _inputs.JumpInput(false);
        }
    }

    private void TryAttach()
    {
        if (FindBestSurface(out var hit))
        {
            float tiltFromUp = Vector3.Angle(hit.normal, Vector3.up);
            if (tiltFromUp < minSurfaceTiltFromUp) { if (debugLogging) Debug.Log("[MagnetWalker] Surface too flat."); return; }

            float aimAngle = Vector3.Angle(-GetAimForward(), hit.normal);
            if (aimAngle > maxAttachAngle) { if (debugLogging) Debug.Log($"[MagnetWalker] Not facing wall enough ({aimAngle:0.0} > {maxAttachAngle})."); return; }

            if (!string.IsNullOrEmpty(magneticTag) && !hit.collider.CompareTag(magneticTag))
            { if (debugLogging) Debug.Log("[MagnetWalker] Tag mismatch."); return; }

            _currentNormal = hit.normal;
            _lastNormal = hit.normal;

            if (_playerController != null) _playerController.enabled = false;

            if (_cc != null)
            {
                _savedStepOffset = _cc.stepOffset;
                _savedSlopeLimit = _cc.slopeLimit;
                _cc.stepOffset = 0f;
                _cc.slopeLimit = 90f;
            }

            if (_cameraTarget != null)
            {
                _savedCamLocalRot = _cameraTarget.localRotation;
                Vector3 fFlat = Vector3.ProjectOnPlane(GetAimForward(), _currentNormal).normalized;
                if (fFlat.sqrMagnitude < 1e-4f) fFlat = Vector3.forward;
                _yaw = SignedAngleOnPlane(transform.forward, fFlat, _currentNormal);
                _pitch = Mathf.Asin(Vector3.Dot(GetAimForward(), _currentNormal)) * Mathf.Rad2Deg;
            }

            _smoothedMove = Vector2.zero;
            _lostTimer = 0f;
            _magnetActive = true;

            // Snap slightly off the wall
            Vector3 snapPos = hit.point + hit.normal * snapOffset;
            _cc.Move(snapPos - transform.position);

            if (debugLogging) Debug.Log("[MagnetWalker] ATTACHED");
        }
        else
        {
            if (debugLogging) Debug.Log("[MagnetWalker] No magnetic surface found nearby.");
        }
    }

    private void Detach()
    {
        if (_cc != null)
        {
            _cc.stepOffset = _savedStepOffset;
            _cc.slopeLimit = _savedSlopeLimit;
        }

        if (_cameraTarget != null)
        {
            _cameraTarget.localRotation = _savedCamLocalRot;
            var e = _cameraTarget.localEulerAngles;
            _cameraTarget.localRotation = Quaternion.Euler(e.x, 0f, 0f);
        }

        if (alignToWorldUpOnDetach)
        {
            Vector3 flatFwd = Vector3.ProjectOnPlane(GetAimForward(), Vector3.up);
            if (flatFwd.sqrMagnitude < 1e-4f) flatFwd = transform.forward;
            float yaw = Quaternion.LookRotation(flatFwd, Vector3.up).eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        if (_playerController != null) _playerController.enabled = true;
        _magnetActive = false;
        if (debugLogging) Debug.Log("[MagnetWalker] DETACHED");
    }

    private void TickMagnet()
    {
        // 1) PRIMARY LOCK: cast back INTO the wall along the current normal
        if (!TryLockUsingCurrentNormal(out var hit))
        {
            _lostTimer += Time.deltaTime;

            // 2) FALLBACK: try wide search once while grace is running
            if (_lostTimer < lostWallGrace && FindBestSurface(out var reHit))
            {
                _currentNormal = reHit.normal;
                _lostTimer = 0f;
                hit = reHit; // recovered lock
            }
            else if (_lostTimer >= lostWallGrace)
            {
                if (debugLogging) Debug.Log("[MagnetWalker] Lost wall (grace elapsed) -> Detach");
                Detach();
                return;
            }
        }
        else
        {
            _lostTimer = 0f;
        }

        _currentNormal = hit.normal;

        float normalDelta = Vector3.Angle(_lastNormal, _currentNormal);
        if (normalDelta > maxNormalChangeToDetach)
        {
            if (debugLogging) Debug.Log("[MagnetWalker] Normal changed too much -> Detach");
            Detach();
            return;
        }
        _lastNormal = _currentNormal;

        // --- camera look on wall ---
        if (_inputs != null && _cameraTarget != null)
        {
            _yaw += _inputs.look.x * lookSensitivity.x * Time.deltaTime;
            _pitch -= _inputs.look.y * lookSensitivity.y * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, pitchLimits.x, pitchLimits.y);

            Vector3 fFlat = Quaternion.AngleAxis(_yaw, _currentNormal) *
                            ProjectOnPlane(transform.forward, _currentNormal).normalized;
            Vector3 right = Vector3.Cross(_currentNormal, fFlat).normalized;

            Quaternion camRot = Quaternion.LookRotation(
                Quaternion.AngleAxis(_pitch, right) * fFlat,
                _currentNormal
            );
            _cameraTarget.rotation = camRot;
        }

        // --- align player up to wall ---
        Quaternion targetUp = Quaternion.FromToRotation(transform.up, _currentNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetUp, Time.deltaTime * alignUpSlerp);

        // --- move on wall plane + stick ---
        Vector2 raw = _inputs != null ? _inputs.move : Vector2.zero;
        _smoothedMove = Vector2.Lerp(_smoothedMove, raw, 1f - Mathf.Exp(-inputSmoothing * Time.deltaTime));

        Vector3 camFwd = _cameraTarget ? _cameraTarget.forward : transform.forward;
        Vector3 camRight = _cameraTarget ? _cameraTarget.right : transform.right;

        Vector3 forwardWall = ProjectOnPlane(camFwd, _currentNormal).normalized;
        Vector3 rightWall = ProjectOnPlane(camRight, _currentNormal).normalized;

        Vector3 move = (forwardWall * _smoothedMove.y + rightWall * _smoothedMove.x) * wallMoveSpeed;
        Vector3 stick = -_currentNormal * stickForce;

        // keep a tiny offset from wall to avoid clipping
        Vector3 keepOff = hit.point + _currentNormal * snapOffset - transform.position;

        _cc.Move((move + stick + keepOff * 10f) * Time.deltaTime);
    }

    // --- primary lock while attached: cast from chest along -currentNormal ---
    private bool TryLockUsingCurrentNormal(out RaycastHit hit)
    {
        hit = new RaycastHit();

        bool prevBackfaces = Physics.queriesHitBackfaces;
        if (queriesHitBackfaces) Physics.queriesHitBackfaces = true;

        try
        {
            float mid = _cc ? Mathf.Clamp(_cc.height * 0.45f, 0.6f, 1.2f) : 0.8f;
            // IMPORTANT: use transform.up (your current local up), not world up
            Vector3 origin = transform.position + transform.up * mid;

            Vector3 dir = -_currentNormal; // straight back into the wall we’re stuck to
            float max = 1.25f;           // short lock distance is enough

            if (drawGizmos) Debug.DrawRay(origin, dir * max, Color.yellow);

            // spherecast = more forgiving than raycast near the surface
            if (Physics.SphereCast(origin, probeRadius, dir, out hit, max, magneticLayers, QueryTriggerInteraction.Ignore))
            {
                if (!string.IsNullOrEmpty(magneticTag) && !hit.collider.CompareTag(magneticTag))
                    return false;
                return true;
            }
            return false;
        }
        finally
        {
            if (queriesHitBackfaces) Physics.queriesHitBackfaces = prevBackfaces;
        }
    }

    // --- wide search for best surface (used on attach & fallback) ---
    private bool FindBestSurface(out RaycastHit bestHit)
    {
        bestHit = new RaycastHit();
        float bestDist = float.MaxValue;

        if (magneticLayers.value == 0)
        {
            if (debugLogging) Debug.LogWarning("[MagnetWalker] 'magneticLayers' is 0 — set your wall layer.");
            return false;
        }

        bool prevBackfaces = Physics.queriesHitBackfaces;
        if (queriesHitBackfaces) Physics.queriesHitBackfaces = true;

        try
        {
            float mid = _cc ? Mathf.Clamp(_cc.height * 0.45f, 0.6f, 1.2f) : 0.8f;
            // IMPORTANT: use transform.up here too
            Vector3 origin = transform.position + transform.up * mid;

            Collider[] cols = Physics.OverlapSphere(origin, detectRadius, magneticLayers, QueryTriggerInteraction.Ignore);
            if (cols.Length == 0) return false;

            foreach (var col in cols)
            {
                if (!string.IsNullOrEmpty(magneticTag) && !col.CompareTag(magneticTag)) continue;

                Vector3 closest = col.ClosestPoint(origin);
                Vector3 dir = (closest - origin);
                float dist = dir.magnitude;
                if (dist < 1e-4f) continue;
                dir /= dist;

                if (Physics.SphereCast(origin, probeRadius, dir, out RaycastHit hit, detectRadius + 0.5f, magneticLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.distance < bestDist)
                    {
                        bestDist = hit.distance;
                        bestHit = hit;
                    }
                }
            }
            return bestDist < float.MaxValue;
        }
        finally
        {
            if (queriesHitBackfaces) Physics.queriesHitBackfaces = prevBackfaces;
        }
    }

    // --- helpers ---
    private Vector3 GetAimForward() => _cameraTarget ? _cameraTarget.forward : transform.forward;
    private static Vector3 ProjectOnPlane(Vector3 v, Vector3 n) => v - Vector3.Dot(v, n) * n;
    private static float SignedAngleOnPlane(Vector3 from, Vector3 to, Vector3 n)
    {
        Vector3 f = ProjectOnPlane(from, n).normalized;
        Vector3 t = ProjectOnPlane(to, n).normalized;
        return Vector3.SignedAngle(f, t, n);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || _cc == null) return;
        float mid = Mathf.Clamp(_cc.height * 0.45f, 0.6f, 1.2f);
        Vector3 origin = transform.position + transform.up * mid;
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(origin, detectRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawRay(origin, -_currentNormal * 1.0f);
    }
}
