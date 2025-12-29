using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Is this the starting checkpoint?")]
    public bool isStartPoint = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(this);
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    public Quaternion GetSpawnRotation()
    {
        return transform.rotation;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}
