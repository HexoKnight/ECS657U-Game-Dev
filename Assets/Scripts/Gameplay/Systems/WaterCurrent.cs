using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class WaterCurrent : MonoBehaviour
{
    [Tooltip("Radius of the current")]
    public float radius = 4f;
    [Tooltip("Force the current applies forwards")]
    public float forwardForce = 10f;
    [Tooltip("Force the current applies inwards")]
    public float constantInForce = 0f;
    [Tooltip("Force the current applies inwards proportional to the square of the distance from the centre")]
    public float squareDistanceInForce = 5f;
    [Tooltip("Whether to apply a centripetal force to prevent being flung out on corners")]
    public bool applyCentripetalForce = true;

    private ForceField _forceField;

    // components
    private SplineContainer _splineContainer;

    // gizmos
    private Vector3 _lastCalcedPosition = Vector3.zero;
    private Vector3 _lastCalcedForce = Vector3.zero;

    private Bounds RangeBounds
    {
        get
        {
            Bounds bounds = _splineContainer.Spline.GetBounds(_splineContainer.transform.localToWorldMatrix);
            bounds.Expand(2 * radius);
            return bounds;
        }
    }

    private void Awake()
    {
        _splineContainer = GetComponent<SplineContainer>();

        _forceField = new ForceField
        {
            calc = delegate (PlayerController pc, Vector3 position, Vector3 velocity, out Vector3 force, float deltaTime)
            {
                Vector3 localPosition = _splineContainer.transform.InverseTransformPoint(position);
                Vector3 localVelocity = _splineContainer.transform.InverseTransformVector(velocity);

                float distance = SplineUtility.GetNearestPoint(
                    _splineContainer.Spline,
                    localPosition,
                    out Unity.Mathematics.float3 nearest,
                    out float t,
                    // each is double the default value
                    resolution: 8,
                    iterations: 4
                );

                // actual 'collider' is a essentially a curvy cylinder
                // ignore the last 0.01 because the evaluations become inaccurate
                if (distance > radius || t <= 0 || t >= 0.99f)
                {
                    force = Vector3.zero;
                    return false;
                }

                // push forward
                Vector3 forwardDirection = ((Vector3)_splineContainer.Spline.EvaluateTangent(t)).normalized;
                float forwardMagnitude = forwardForce;

                // keep inside current
                Vector3 inwardsDirection = ((Vector3)nearest - localPosition).normalized;
                float inwardsMagnitude = constantInForce + squareDistanceInForce * distance * distance;

                // help prevent being flung out on sharp corners
                Vector3 centripetalDirection = Vector3.zero;
                float centripetalForce = 0;
                if (applyCentripetalForce)
                {
                    Vector3 curvatureCentre = _splineContainer.Spline.EvaluateCurvatureCenter(t);
                    Vector3 curvatureVector = curvatureCentre - localPosition;
                    centripetalDirection = curvatureVector.normalized;
                    centripetalForce = System.MathF.Pow(Vector3.Dot(localVelocity, forwardDirection), 2) / curvatureVector.magnitude;
                }

                Vector3 localForce = forwardDirection * forwardMagnitude + inwardsDirection * inwardsMagnitude + centripetalDirection * centripetalForce;

                force = _splineContainer.transform.TransformVector(localForce);

                // for gizmos
                _lastCalcedPosition = position;
                _lastCalcedForce = force;

                return true;
            },

            onActive = delegate (PlayerController pc)
            {
            },
            onInactive = delegate (PlayerController pc)
            {
            },
        };
    }

    private void FixedUpdate()
    {
        // idk there could be multiple players? and it makes this implementation simpler
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (RangeBounds.Contains(player.transform.position))
                player.EnterForceFieldRange(_forceField);
            else
                player.ExitForceFieldRange(_forceField);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_splineContainer == null) _splineContainer = GetComponent<SplineContainer>();

        Bounds bounds = RangeBounds;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(bounds.extents.x * 2, 0, 0));
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(0, bounds.extents.y * 2, 0));
        Gizmos.DrawLine(bounds.min, bounds.min + new Vector3(0, 0, bounds.extents.z * 2));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_lastCalcedPosition, _lastCalcedPosition + _lastCalcedForce);
    }
}
