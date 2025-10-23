using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MagneticSurface : MonoBehaviour
{
    [Header("Magnetic Surface Settings")]
    [Range(0f, 50f)] public float attractionStrength = 20f;   // Strength of attraction
    [Range(0f, 90f)] public float maxAttachAngle = 85f;       // Max angle for attachment

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = false;

        // Ensure this object is on the "MagneticSurface" layer
        int layer = LayerMask.NameToLayer("MagneticSurface");
        if (layer == -1)
        {
            Debug.LogWarning("Create a layer named 'MagneticSurface' and assign it to magnetic surfaces!");
        }
        else if (gameObject.layer == 0)
        {
            gameObject.layer = layer;
        }
    }

    private void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider>();
        if (!c) return;
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
    }

    // Optional: Method to return the attraction force based on distance or other parameters
    public Vector3 GetMagneticForce(Vector3 position, Vector3 surfaceNormal)
    {
        // Calculate attraction force based on distance and angle
        float distance = Vector3.Distance(position, transform.position);
        float angle = Vector3.Angle(surfaceNormal, transform.up);

        if (angle > maxAttachAngle)
            return Vector3.zero;  // Don't apply force if the angle is too steep

        // Use a simple inverse distance formula for attraction strength
        float strength = Mathf.Clamp(attractionStrength / (distance * distance), 0f, attractionStrength);
        return surfaceNormal * strength;
    }
}
