using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMagnetController : MonoBehaviour
{
    public LayerMask magneticLayer;
    public float senseRadius = 2.5f;
    public float stickForce = 30f;
    public KeyCode stickKey = KeyCode.E;
    public KeyCode detachKey = KeyCode.Space;

    Rigidbody rb;
    bool isStuck = false;
    Vector3 surfaceNormal;

    void Start() { rb = GetComponent<Rigidbody>(); }

    void Update()
    {
        if (Input.GetKeyDown(stickKey))
        {
            if (!isStuck) TryAttach();
            else Detach();
        }
        if (isStuck && Input.GetKeyDown(detachKey))
        {
            Detach();
        }
    }

    void FixedUpdate()
    {
        if (isStuck)
        {
            rb.AddForce(-surfaceNormal * stickForce, ForceMode.Acceleration);
        }
    }

    void TryAttach()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, senseRadius, magneticLayer))
        {
            transform.position = hit.point + hit.normal * 0.5f;
            surfaceNormal = hit.normal;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            isStuck = true;
        }
    }

    void Detach()
    {
        isStuck = false;
        rb.useGravity = true;
    }
}
