using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Support manta ray that follows a spline path and acts as a moving platform.
/// Player can ride on top of the manta.
/// </summary>
public class SupportManta : MonoBehaviour
{
    #region Path Settings
    
    [Header("Path Following")]
    [Tooltip("The spline path to follow")]
    [SerializeField] private SplineContainer path;
    
    [Tooltip("Movement speed along the path")]
    [SerializeField] private float speed = 5f;
    
    [Tooltip("If true, loop back to start when reaching end")]
    [SerializeField] private bool loop = true;
    
    [Tooltip("If true, ping-pong instead of loop")]
    [SerializeField] private bool pingPong = false;
    
    #endregion

    #region Platform Settings
    
    [Header("Platform Behavior")]
    [Tooltip("If true, players riding will be parented (move with manta)")]
    [SerializeField] private bool parentRiders = false;
    
    [Tooltip("Offset applied to rider position")]
    [SerializeField] private Vector3 riderOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("How quickly riders sync to manta movement")]
    #pragma warning disable 0414
    [SerializeField] private float riderSyncSpeed = 10f;
    #pragma warning restore 0414
    
    #endregion

    #region Animation
    
    [Header("Animation")]
    [Tooltip("Bobbing amplitude")]
    [SerializeField] private float bobAmplitude = 0.2f;
    
    [Tooltip("Bobbing speed")]
    [SerializeField] private float bobSpeed = 2f;
    
    [Tooltip("Wing flap animator parameter")]
    [SerializeField] private string flapAnimParam = "IsFlapping";
    
    #endregion

    #region Private State
    
    private float distance;
    private float length;
    private bool movingForward = true;
    private Vector3 previousPosition;
    private Vector3 velocity;
    private Animator animator;
    
    // Track riders
    private System.Collections.Generic.HashSet<Transform> riders = new();
    private System.Collections.Generic.Dictionary<Transform, Transform> riderOriginalParents = new();
    
    #endregion

    #region Properties
    
    /// <summary>Current velocity of the manta (for rider movement)</summary>
    public Vector3 Velocity => velocity;
    
    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        
        if (path != null)
        {
            length = path.CalculateLength();
        }
        
        previousPosition = transform.position;
        
        if (animator != null && !string.IsNullOrEmpty(flapAnimParam))
        {
            animator.SetBool(flapAnimParam, true);
        }
    }
    
    private void Update()
    {
        if (path == null) return;
        
        // Track velocity
        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
        
        // Move along path
        UpdatePosition();
        
        // Update riders
        UpdateRiders();
    }
    
    private void FixedUpdate()
    {
        // For KinematicCharacterController compatibility, the manta needs to
        // be treated as a kinematic rigidbody. The KCC automatically handles
        // moving platform detection if the manta has a Rigidbody set to kinematic.
    }
    
    #endregion

    #region Path Following
    
    private void UpdatePosition()
    {
        float delta = speed * Time.deltaTime * (movingForward ? 1f : -1f);
        distance += delta;
        
        // Handle end of path
        if (distance > length)
        {
            if (loop)
            {
                distance -= length;
            }
            else if (pingPong)
            {
                distance = length - (distance - length);
                movingForward = false;
            }
            else
            {
                distance = length;
            }
        }
        else if (distance < 0)
        {
            if (pingPong)
            {
                distance = -distance;
                movingForward = true;
            }
            else if (loop)
            {
                distance += length;
            }
            else
            {
                distance = 0;
            }
        }
        
        // Evaluate position and rotation
        float t = length > 0 ? distance / length : 0;
        Vector3 pos = path.EvaluatePosition(t);
        Vector3 forward = path.EvaluateTangent(t);
        Vector3 up = path.EvaluateUpVector(t);
        
        // Add bobbing
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        pos += up * bobOffset;
        
        // Apply transform
        transform.position = pos;
        
        if (forward.sqrMagnitude > 0.01f)
        {
            // Flip direction if moving backward
            if (!movingForward)
            {
                forward = -forward;
            }
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
    
    #endregion

    #region Rider Management
    
    private void UpdateRiders()
    {
        if (!parentRiders) return;
        
        // Sync rider positions (if not parented, apply velocity)
        foreach (var rider in riders)
        {
            if (rider == null) continue;
            
            // The rider follows the manta's movement
            // This is handled by parenting, but we can add offset
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Check if player landed on top
        if (!collision.gameObject.CompareTag("Player")) return;
        
        // Verify player is above the manta (landed on top, not bumped from side)
        Vector3 contactNormal = Vector3.zero;
        if (collision.contactCount > 0)
        {
            contactNormal = collision.GetContact(0).normal;
        }
        
        // If collision normal points mostly down, player is on top
        if (Vector3.Dot(contactNormal, Vector3.down) > 0.5f)
        {
            AddRider(collision.transform);
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RemoveRider(collision.transform);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Alternative: use trigger zone on top of manta
        if (other.CompareTag("Player"))
        {
            AddRider(other.transform);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RemoveRider(other.transform);
        }
    }
    
    private void AddRider(Transform rider)
    {
        if (riders.Contains(rider)) return;
        
        riders.Add(rider);
        
        if (parentRiders)
        {
            riderOriginalParents[rider] = rider.parent;
            rider.SetParent(transform);
        }
        
        Debug.Log($"[SupportManta] {rider.name} is now riding the manta");
    }
    
    private void RemoveRider(Transform rider)
    {
        if (!riders.Contains(rider)) return;
        
        riders.Remove(rider);
        
        if (parentRiders && riderOriginalParents.ContainsKey(rider))
        {
            rider.SetParent(riderOriginalParents[rider]);
            riderOriginalParents.Remove(rider);
        }
        
        Debug.Log($"[SupportManta] {rider.name} stopped riding the manta");
    }
    
    #endregion

    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        // Draw path
        if (path != null)
        {
            Gizmos.color = Color.cyan;
            
            int steps = 50;
            for (int i = 0; i < steps; i++)
            {
                float t1 = (float)i / steps;
                float t2 = (float)(i + 1) / steps;
                
                Vector3 p1 = path.EvaluatePosition(t1);
                Vector3 p2 = path.EvaluatePosition(t2);
                
                Gizmos.DrawLine(p1, p2);
            }
        }
        
        // Draw rider offset
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.TransformDirection(riderOffset), 0.3f);
    }
    
    #endregion
}
