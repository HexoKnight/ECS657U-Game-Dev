using UnityEngine;

using GUP.Core.Config;
using GUP.Core.Debug;

/// <summary>
/// Bubble stream that pushes the player upward when inside.
/// Creates a vertical current effect for underwater platforming.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BubbleStream : MonoBehaviour
{
    #region Configuration
    
    [Header("Configuration")]
    [Tooltip("Optional: Hazard config ScriptableObject. If not assigned, uses local serialized values.")]
    [SerializeField] private HazardConfig hazardConfig;
    
    #endregion

    #region Settings
    
    [Header("Push Force")]
    [Tooltip("Upward force applied to the player")]
    [SerializeField] private float upwardForce = 15f;
    
    // Property uses config if assigned, otherwise fallback
    private float UpwardForce => hazardConfig != null ? hazardConfig.pushForce : upwardForce;
    
    [Tooltip("How quickly the force builds up")]
    [SerializeField] private float forceRampSpeed = 3f;
    
    [Tooltip("Maximum additional velocity that can be added")]
    #pragma warning disable 0414
    [SerializeField] private float maxVelocityBoost = 10f;
    #pragma warning restore 0414
    
    [Header("Visuals")]
    [Tooltip("Particle system for bubble effect")]
    [SerializeField] private ParticleSystem bubbleParticles;
    
    [Tooltip("Increase particle emission when player is inside")]
    [SerializeField] private float activeEmissionMultiplier = 2f;
    
    [Header("Audio")]
    [Tooltip("Ambient bubble sound")]
    [SerializeField] private AudioClip bubbleSound;
    
    [Tooltip("Sound when player enters stream")]
    [SerializeField] private AudioClip enterSound;
    
    #endregion

    #region Private State
    
    private System.Collections.Generic.HashSet<PlayerController> playersInStream = new();
    private System.Collections.Generic.Dictionary<PlayerController, float> playerForceBuildup = new();
    private float baseEmissionRate;
    private AudioSource audioSource;
    
    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        // Get base emission rate
        if (bubbleParticles != null)
        {
            var emission = bubbleParticles.emission;
            baseEmissionRate = emission.rateOverTime.constant;
        }
        
        // Setup audio
        if (bubbleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = bubbleSound;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.spatialBlend = 1f;
            audioSource.Play();
        }
        
        // Ensure trigger is set
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void FixedUpdate()
    {
        foreach (var player in playersInStream)
        {
            if (player == null) continue;
            
            // Build up force over time
            if (!playerForceBuildup.ContainsKey(player))
            {
                playerForceBuildup[player] = 0f;
            }
            
            float currentBuildup = playerForceBuildup[player];
            currentBuildup = Mathf.MoveTowards(currentBuildup, 1f, forceRampSpeed * Time.fixedDeltaTime);
            playerForceBuildup[player] = currentBuildup;
            
            // Calculate and apply force
            float force = UpwardForce * currentBuildup;
            Vector3 pushDirection = transform.up; // Local up direction
            
            // Use AddVelocity to push player upward
            player.AddVelocity(pushDirection * force * Time.fixedDeltaTime);
        }
        
        // Update particle emission
        UpdateParticles();
    }
    
    #endregion

    #region Trigger Handling
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (playersInStream.Contains(player)) return;
        
        playersInStream.Add(player);
        playerForceBuildup[player] = 0f;
        
        // Play enter sound
        if (enterSound != null)
        {
            AudioSource.PlayClipAtPoint(enterSound, player.transform.position);
        }
        
        GupDebug.LogHazardActivation("BubbleStream", other.name, true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        playersInStream.Remove(player);
        playerForceBuildup.Remove(player);
        
        GupDebug.LogHazardActivation("BubbleStream", other.name, false);
    }
    
    #endregion

    #region Visuals
    
    private void UpdateParticles()
    {
        if (bubbleParticles == null) return;
        
        var emission = bubbleParticles.emission;
        
        if (playersInStream.Count > 0)
        {
            emission.rateOverTime = baseEmissionRate * activeEmissionMultiplier;
        }
        else
        {
            emission.rateOverTime = baseEmissionRate;
        }
    }
    
    #endregion

    #region Debug
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        
        // Draw stream direction
        Gizmos.DrawRay(transform.position, transform.up * 3f);
        
        // Draw arrow head
        Vector3 arrowTip = transform.position + transform.up * 3f;
        Gizmos.DrawRay(arrowTip, Quaternion.Euler(0, 0, 45) * -transform.up * 0.5f);
        Gizmos.DrawRay(arrowTip, Quaternion.Euler(0, 0, -45) * -transform.up * 0.5f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.5f);
        
        // Draw collider bounds
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
    
    #endregion
}
