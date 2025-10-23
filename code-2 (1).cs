using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMagnetController : MonoBehaviour
{
    [Header("Magnet Settings")]
    public LayerMask magneticLayer;       // Layer for magnetic surfaces
    public float senseRadius = 3f;        // Radius for detecting surfaces
    public float stickForce = 50f;        // Strength of the magnetic pull
    public float rotationSpeed = 8f;      // Speed at which the player rotates to align with the surface

    [Header("Controls")]
    public KeyCode stickKey = KeyCode.E;  // Key to attach/detach
    public KeyCode detachKey = KeyCode.Space; // Key to quickly detach

    private Rigidbody rb;
    private bool isStuck = false;
    private Vector3 surfaceNormal;
    private Transform playerCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // Ensure gravity is on by default
        playerCamera = Camera.main.transform; // Assign the main camera
    }

    void Update()
    {
        // Try to attach or detach with the stick key
        if (Input.GetKeyDown(stickKey))
        {
            if (!isStuck)
                TryAttach(); // Try to attach to a surface
            else
                Detach();   // Detach from the surface
        }

        // Detach if the detach key is pressed
        if (isStuck && Input.GetKeyDown(detachKey))
        {
            Detach();  // Detach the player from the surface
        }

        // Smoothly rotate to align with the surface while stuck
        if (isStuck)
        {
            // Rotate player to align with the wall's surface
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, surfaceNormal), surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Move along the wall, forward and backward, based on input
            float horizontalInput = Input.GetAxis("Horizontal");  // Left/Right movement along the wall
            Vector3 moveDirection = Vector3.Cross(surfaceNormal, Vector3.up).normalized; // Determine movement direction
            rb.AddForce(moveDirection * horizontalInput * stickForce, ForceMode.Force);  // Apply movement force
        }
    }

    void FixedUpdate()
    {
        if (isStuck)
        {
            // Apply constant force to keep the player stuck to the wall
            rb.AddForce(-surfaceNormal * stickForce, ForceMode.Acceleration);
        }
    }

    void TryAttach()
    {
        // Cast a ray to detect a nearby magnetic surface in front of the player
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, senseRadius, magneticLayer))
        {
            surfaceNormal = hit.normal;
            rb.useGravity = false; // Disable gravity while stuck
            rb.velocity = Vector3.zero; // Stop any current movement
            isStuck = true;

            // Position the player slightly away from the surface to avoid overlap
            transform.position = hit.point + hit.normal * 0.5f;

            // Rotate the camera to follow the player as they stick to the wall
            playerCamera.RotateAround(transform.position, Vector3.up, 90); // Rotate camera to follow player
        }
    }

    void Detach()
    {
        isStuck = false;
        rb.useGravity = true; // Re-enable gravity
        playerCamera.RotateAround(transform.position, Vector3.up, -90); // Rotate camera back
    }
}
