using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MagneticSurface : MonoBehaviour
{
    [Range(0f, 50f)] public float attractionStrength = 20f;
    public float activationDistance = 2.0f;
    [Range(0f, 90f)] public float maxAttachAngle = 80f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var c = GetComponent<Collider>();
        if (c != null)
        {
            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
        }
    }
}
