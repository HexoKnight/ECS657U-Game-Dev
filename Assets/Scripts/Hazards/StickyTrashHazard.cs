using UnityEngine;

public class StickyTrashHazard : MonoBehaviour
{
    [Tooltip("How much to slow the player (0-1, where 1 is stopped)")]
    [Range(0f, 0.9f)]
    public float slowFactor = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // We need a way to modify player speed.
                // For now, we can modify the public maxGroundMoveSpeed directly, 
                // but a better way would be a "SpeedModifier" system.
                // I will assume I can modify the public fields for the prototype.
                player.maxGroundMoveSpeed *= (1f - slowFactor);
                player.maxAirMoveSpeed *= (1f - slowFactor);
                Debug.Log("Player slowed by sticky trash!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Restore speed (approximate inverse)
                player.maxGroundMoveSpeed /= (1f - slowFactor);
                player.maxAirMoveSpeed /= (1f - slowFactor);
                Debug.Log("Player left sticky trash.");
            }
        }
    }
}
