using System.Collections;
using UnityEngine;
using Cinemachine;
using StarterAssets; // Starter Assets First Person Controller / Inputs

/// <summary>
/// Monta al jugador en un CinemachineDollyCart al entrar en un SphereCollider (IsTrigger),
/// hace el cambio de cámaras, mueve el cart de 0→1 en tiempo exacto y aplica rotación aleatoria
/// al parent del jugador para evitar mareo rotando solo el parent. Al terminar, aplana rotaciones
/// (sin pitch/roll) para que los ejes del FPC queden correctos.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class DollyTrackMount : MonoBehaviour
{
    [Header("Referencias - Cinemachine")]
    [Tooltip("Dolly Cart a recorrer")]
    public CinemachineDollyCart dollyCart;

    [Tooltip("Punto de montaje (Empty) hijo de algo que siga al cart. Aquí se 'sienta' el jugador.")]
    public Transform cartMountParent;

    [Tooltip("Virtual Camera del jugador (p.ej. PlayerFollowCamera).")]
    public CinemachineVirtualCamera playerVCam;

    [Tooltip("Virtual Camera del cart.")]
    public CinemachineVirtualCamera cartVCam;

    [Header("Jugador")]
    [Tooltip("Raíz del jugador (el objeto que tiene CharacterController y FirstPersonController).")]
    public GameObject playerRoot;

    [Tooltip("Transform que se re-parentizará (p.ej. el mismo playerRoot o un child que contiene la cámara).")]
    public Transform playerMountTarget;

    [Tooltip("Punto opcional de salida al terminar el ride (si se asigna, se teletransporta ahí).")]
    public Transform exitPlacement;

    [Header("Trayecto")]
    [Tooltip("Duración del recorrido completo (0→1) en segundos.")]
    [Min(0.01f)] public float rideDuration = 6f;

    [Tooltip("Retraso (seg) antes de elevar la prioridad de la cámara del cart (para desenfocar/transicionar un pelín antes).")]
    [Min(0f)] public float cameraPreBlendLead = 0.15f;

    [Tooltip("Si es true, ignoramos m_Speed y avanzamos con Lerp de posición (exacto en tiempo).")]
    public bool driveByPositionLerp = true;

    [Tooltip("Curva opcional para el Lerp de 0→1 (dejar vacía para lineal).")]
    public AnimationCurve positionCurve;

    [Header("Unidades de la Ruta")]
    [Tooltip("Si tu ruta está en 'Normalized' 0..1, activa esto. Si no, calculamos con MaxUnit() del Path.")]
    public bool pathIsNormalized01 = true;

    [Header("Rotación aleatoria del parent")]
    public bool enableRandomRotation = true;

    [Tooltip("Amplitud máxima de rotación aleatoria (Euler grados).")]
    public Vector3 maxRandomEuler = new Vector3(12f, 18f, 8f);

    [Tooltip("Velocidad de interpolación hacia el objetivo aleatorio.")]
    [Min(0.1f)] public float randomRotLerpSpeed = 3f;

    [Tooltip("Intervalo mínimo entre cambios de objetivo aleatorio.")]
    [Min(0.1f)] public float randomRotIntervalMin = 1.2f;

    [Tooltip("Intervalo máximo entre cambios de objetivo aleatorio.")]
    [Min(0.2f)] public float randomRotIntervalMax = 3.5f;

    // Internos
    private FirstPersonController _fpc;
    private CharacterController _cc;
    private StarterAssetsInputs _inputs;

    private int _playerPriority;
    private int _cartPriority;
    private bool _isRiding;
    private Quaternion _targetRandomRot = Quaternion.identity;
    private Transform _originalParent;

    private float _pathMaxUnit = 1f;

    private void Reset()
    {
        var col = GetComponent<SphereCollider>();
        if (col) col.isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<SphereCollider>();
        if (col != null && !col.isTrigger) col.isTrigger = true;

        if (playerRoot != null)
        {
            _fpc = playerRoot.GetComponent<FirstPersonController>();
            _cc  = playerRoot.GetComponent<CharacterController>();
            _inputs = playerRoot.GetComponent<StarterAssetsInputs>();
        }
    }

    private void Start()
    {
        if (playerVCam) _playerPriority = playerVCam.Priority;
        if (cartVCam)   _cartPriority   = cartVCam.Priority;

        if (dollyCart != null && dollyCart.m_Path != null)
        {
            _pathMaxUnit = pathIsNormalized01
                ? 1f
                : dollyCart.m_Path.MaxUnit(dollyCart.m_PositionUnits);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isRiding) return;

        if (playerRoot != null && other.gameObject == playerRoot)
        {
            if (dollyCart == null || cartMountParent == null || playerVCam == null || cartVCam == null || playerMountTarget == null)
            {
                Debug.LogError("[DollyTrackMount] Faltan referencias en el inspector.");
                return;
            }
            StartCoroutine(RideSequence());
        }
    }

    private IEnumerator RideSequence()
    {
        _isRiding = true;

        // 1) Apagar control del jugador de forma limpia (Starter Assets).
        if (_inputs != null)
        {
            _inputs.MoveInput(Vector2.zero);
            _inputs.LookInput(Vector2.zero);
            _inputs.SprintInput(false);
            _inputs.JumpInput(false);
        }

        if (_fpc) _fpc.enabled = false;      // detiene Update/LateUpdate del FPC
        if (_cc)  _cc.enabled  = false;      // evita interferencias del CharacterController

        // 2) Preparar cámaras: blend suave
        if (playerVCam) playerVCam.Priority = _playerPriority;
        if (cartVCam)   cartVCam.Priority   = _playerPriority - 1; // aún por debajo

        // 3) Colocar jugador en el punto de montaje
        _originalParent = playerMountTarget.parent;
        playerMountTarget.SetParent(cartMountParent, worldPositionStays: false);
        playerMountTarget.localPosition = Vector3.zero;
        playerMountTarget.localRotation = Quaternion.identity;

        // 4) Preparar DollyCart para control manual por posición (tiempo exacto)
        float startUnit = 0f;
        float endUnit   = _pathMaxUnit;

        dollyCart.m_Speed = 0f; // lo moveremos por m_Position
        dollyCart.m_Position = startUnit;

        if (cameraPreBlendLead > 0f)
            yield return new WaitForSeconds(cameraPreBlendLead);

        if (cartVCam) cartVCam.Priority = _playerPriority + 10; // toma control

        // 5) Rotación aleatoria del parent
        Coroutine randomRotCo = null;
        if (enableRandomRotation) randomRotCo = StartCoroutine(RandomRotationRoutine());

        // 6) Recorrer EXACTO en rideDuration
        float t = 0f;
        while (t < rideDuration)
        {
            t += Time.deltaTime;
            float nt = Mathf.Clamp01(t / rideDuration);

            if (positionCurve != null && positionCurve.keys != null && positionCurve.length > 0)
                nt = Mathf.Clamp01(positionCurve.Evaluate(nt));

            float pos = Mathf.Lerp(startUnit, endUnit, nt);
            dollyCart.m_Position = pos;

            if (enableRandomRotation)
            {
                cartMountParent.localRotation = Quaternion.Slerp(
                    cartMountParent.localRotation,
                    _targetRandomRot,
                    Time.deltaTime * randomRotLerpSpeed
                );
            }

            yield return null;
        }

        // Asegurar final
        dollyCart.m_Position = endUnit;

        // 7) Limpiar y SALIR (con aplanado de rotaciones)
        if (randomRotCo != null) StopCoroutine(randomRotCo);
        cartMountParent.localRotation = Quaternion.identity;

        // --- Elegir yaw final (solo eje Y) ---
        float finalYaw;
        if (exitPlacement != null)
        {
            // Usa solo yaw del exit (ignoramos pitch/roll del exit si los tuviera)
            finalYaw = exitPlacement.eulerAngles.y;
        }
        else
        {
            // Usa forward del cart proyectado en XZ para no heredar inclinación
            Vector3 flatForward = Vector3.ProjectOnPlane(cartMountParent.forward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
            finalYaw = Quaternion.LookRotation(flatForward, Vector3.up).eulerAngles.y;
        }

        // Des-parentizar manteniendo en mundo
        playerMountTarget.SetParent(_originalParent, worldPositionStays: true);

        // Colocar posición de salida (si se indicó)
        if (exitPlacement != null)
            playerRoot.transform.position = exitPlacement.position;

        // Aplanar rotación del jugador: sin pitch/roll, solo yaw final
        playerRoot.transform.rotation = Quaternion.Euler(0f, finalYaw, 0f);

        // Asegurar cámara del jugador sin roll (conserva su pitch actual)
        if (_fpc != null && _fpc.CinemachineCameraTarget != null)
        {
            var camTr = _fpc.CinemachineCameraTarget.transform;
            var e = camTr.localEulerAngles; // e.x = pitch acumulado por el FPC
            camTr.localRotation = Quaternion.Euler(e.x, 0f, 0f); // quita roll/yaw locales
        }

        // Restaurar cámaras
        if (playerVCam) playerVCam.Priority = _playerPriority + 10; // vuelve el control al jugador
        if (cartVCam)   cartVCam.Priority   = _cartPriority;

        // Reactivar control (CharacterController primero)
        if (_cc)  _cc.enabled  = true;
        if (_fpc) _fpc.enabled = true;

        _isRiding = false;
    }

    private IEnumerator RandomRotationRoutine()
    {
        while (true)
        {
            Vector3 euler = new Vector3(
                Random.Range(-maxRandomEuler.x, maxRandomEuler.x),
                Random.Range(-maxRandomEuler.y, maxRandomEuler.y),
                Random.Range(-maxRandomEuler.z, maxRandomEuler.z)
            );
            _targetRandomRot = Quaternion.Euler(euler);
            yield return new WaitForSeconds(Random.Range(randomRotIntervalMin, randomRotIntervalMax));
        }
    }

    private void OnValidate()
    {
        if (randomRotIntervalMax < randomRotIntervalMin)
            randomRotIntervalMax = randomRotIntervalMin + 0.1f;
    }
}
