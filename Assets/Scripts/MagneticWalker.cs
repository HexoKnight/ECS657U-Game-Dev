using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class MagnetWalker : MonoBehaviour
{
    [Header("Activation")]
    public LayerMask magneticLayers;
    public string magneticTag = "";
    public bool useToggle = true;

    [Header("Robust surface detection")]
    public float detectRadius = 2.5f;
    public float probeRadius = 0.35f;
    public bool queriesHitBackfaces = true;
    [Range(0, 90)] public float minSurfaceTiltFromUp = 25f;
    [Range(0, 90)] public float maxAttachAngle = 80f;

    [Header("Detach / stability")]
    [Tooltip("How long we can lose the wall before detaching (seconds).")]
    public float lostWallGrace = 0.25f;
    [Range(0, 90)] public float maxNormalChangeToDetach = 55f;

    [Header("Debug")]
    public bool debugLogging = true;
    public bool drawGizmos = true;

    // ---- internals ----
    private PlayerInputs _inputs;
    private PlayerController _playerController;
    private Transform _cameraTarget;

    private bool _magnetActive;

    private float _lostTimer;

    void Awake()
    {
        _inputs = GetComponent<PlayerInputs>();

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
        if (useToggle)
        {
            if (_inputs.magnetise)
            {
                if (_magnetActive) Detach();
                else TryAttach();
            }
        }
        else
        {
            if (_inputs.magnetise && !_magnetActive) TryAttach();
            if (_magnetActive) Detach();
        }

        // reset magnetise regardless of what happened
        _inputs.magnetise = false;
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

            _playerController.targetUp = hit.normal;

            _lostTimer = 0f;
            _magnetActive = true;

            if (debugLogging) Debug.Log("[MagnetWalker] ATTACHED");
        }
        else
        {
            if (debugLogging) Debug.Log("[MagnetWalker] No magnetic surface found nearby.");
        }
    }

    private void Detach()
    {
        _playerController.targetUp = Vector3.up;

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
                _playerController.targetUp = reHit.normal;
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

        _playerController.targetUp = hit.normal;
    }

    // --- primary lock while attached: cast from chest along -currentNormal ---
    private bool TryLockUsingCurrentNormal(out RaycastHit hit)
    {
        hit = new RaycastHit();

        bool prevBackfaces = Physics.queriesHitBackfaces;
        if (queriesHitBackfaces) Physics.queriesHitBackfaces = true;

        try
        {
            Vector3 origin = transform.position;

            Vector3 dir = -_playerController.targetUp; // straight back into the wall we’re stuck to
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
            Vector3 origin = transform.position;

            Collider[] cols = Physics.OverlapSphere(origin, detectRadius, magneticLayers, QueryTriggerInteraction.Ignore);
            if (cols.Length == 0) return false;

            foreach (var col in cols)
            {
                if (!string.IsNullOrEmpty(magneticTag) && !col.CompareTag(magneticTag)) continue;

                Vector3 closest = col.ClosestPoint(origin);
                Vector3 dir = closest - origin;
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

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Vector3 origin = transform.position;
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(origin, detectRadius);
        Gizmos.color = Color.yellow; if (_playerController != null) Gizmos.DrawRay(origin, -_playerController.targetUp * 1.0f);
    }
}
