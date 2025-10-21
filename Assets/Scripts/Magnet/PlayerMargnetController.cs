using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMagnetController : MonoBehaviour
{
    public LayerMask magneticLayer;
    public float senseRadius = 2.5f;
    public float stickForce = 30f;
    public KeyCode stickKey = KeyCode.E;
    public KeyCode detachKey = KeyCode.Space;

    PlayerController pc;
    bool isStuck = false;
    Vector3 surfaceNormal;

    void Start() { pc = GetComponent<PlayerController>(); }

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
        }
    }

    void TryAttach()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, senseRadius, magneticLayer))
        {
            transform.position = hit.point + hit.normal * 0.5f;
            surfaceNormal = hit.normal;
            pc.useGravity = false;
            isStuck = true;
        }
    }

    void Detach()
    {
        isStuck = false;
        pc.useGravity = true;
    }
}
