using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private Checkpoint currentCheckpoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find start point if none selected
        if (currentCheckpoint == null)
        {
            Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            foreach (var cp in allCheckpoints)
            {
                if (cp.isStartPoint)
                {
                    currentCheckpoint = cp;
                    break;
                }
            }
        }
    }

    public void SetCheckpoint(Checkpoint cp)
    {
        if (currentCheckpoint != cp)
        {
            currentCheckpoint = cp;
            Debug.Log($"Checkpoint set to: {cp.name}");
        }
    }

    public void RespawnPlayer(GameObject player)
    {
        if (currentCheckpoint != null)
        {
            // CharacterController/KinematicCharacterMotor often needs special handling for teleportation
            // We will attempt to set position directly, but might need to access the Motor if it overrides it.
            
            // Assuming KinematicCharacterMotor is used:
            var motor = player.GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
            if (motor != null)
            {
                motor.SetPositionAndRotation(currentCheckpoint.GetSpawnPosition(), currentCheckpoint.GetSpawnRotation());
            }
            else
            {
                player.transform.position = currentCheckpoint.GetSpawnPosition();
                player.transform.rotation = currentCheckpoint.GetSpawnRotation();
            }
        }
        else
        {
            Debug.LogWarning("No checkpoint set! Reloading scene might be better fallback.");
            // Fallback to scene reload or 0,0,0
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
