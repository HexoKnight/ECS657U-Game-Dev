using UnityEngine;
using UnityEngine.Splines;

public class SupportManta : MonoBehaviour
{
    public SplineContainer path;
    public float speed = 5f;
    public bool loop = true;

    private float distance;
    private float length;

    private void Start()
    {
        if (path != null)
        {
            length = path.CalculateLength();
        }
    }

    private void Update()
    {
        if (path == null) return;

        distance += speed * Time.deltaTime;

        if (distance > length)
        {
            if (loop) distance -= length;
            else distance = length;
        }

        Vector3 pos = path.EvaluatePosition(distance / length);
        Vector3 forward = path.EvaluateTangent(distance / length);
        Vector3 up = path.EvaluateUpVector(distance / length);

        // Kinematic update
        transform.position = pos;
        if (forward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
    
    // Note: For the player to "stick" to this platform, the KinematicCharacterMotor 
    // usually handles moving platforms automatically if the platform has a collider 
    // and the KCC detects it as ground.
    // Ensure this object has a MeshCollider or BoxCollider and is on a layer the player collides with.
}
