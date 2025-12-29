using UnityEngine;
using System.Collections.Generic;

using GUP.Core.Config;

/// <summary>
/// Hazard that slows the player and optionally impairs vision when touching sticky trash.
/// Uses a proper modifier system instead of directly changing player values.
/// </summary>
public class StickyTrashHazard : MonoBehaviour
{
    #region Configuration
    
    [Header("Configuration")]
    [Tooltip("Optional: Hazard config ScriptableObject. If not assigned, uses local serialized values.")]
    [SerializeField] private HazardConfig hazardConfig;
    
    #endregion

    #region Settings
    
    [Header("Slow Effect")]
    [Tooltip("How much to slow the player (0-1, where 1 is stopped)")]
    [Range(0f, 0.9f)]
    [SerializeField] private float slowFactor = 0.5f;
    
    // Property uses config if assigned, otherwise fallback
    private float SlowFactor => hazardConfig != null ? hazardConfig.slowFactor : slowFactor;
    
    [Tooltip("How quickly the slow effect applies")]
    [SerializeField] private float slowTransitionSpeed = 3f;
    
    [Header("Visual Impairment")]
    [Tooltip("Enable vision blur effect")]
    [SerializeField] private bool impairVision = true;
    
    [Tooltip("Darkness overlay color")]
    [SerializeField] private Color overlayColor = new Color(0.1f, 0.08f, 0.05f, 0.6f);
    
    [Header("Audio")]
    [Tooltip("Sound to play when entering hazard")]
    [SerializeField] private AudioClip enterSound;
    
    [Tooltip("Sound to play while in hazard")]
    [SerializeField] private AudioClip loopSound;
    
    #endregion

    #region Private State
    
    // Track players in hazard
    private HashSet<PlayerController> playersInHazard = new HashSet<PlayerController>();
    private Dictionary<PlayerController, float> originalGroundSpeeds = new Dictionary<PlayerController, float>();
    private Dictionary<PlayerController, float> originalAirSpeeds = new Dictionary<PlayerController, float>();
    private Dictionary<PlayerController, float> currentSpeedModifiers = new Dictionary<PlayerController, float>();
    
    // Vision overlay
    private GameObject visionOverlay;
    private CanvasGroup overlayCanvasGroup;
    private AudioSource audioSource;
    
    #endregion

    #region Unity Lifecycle
    
    private void Start()
    {
        // Create audio source for loop
        if (loopSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = loopSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }
    
    private void Update()
    {
        // Smoothly apply slow effect
        foreach (var player in playersInHazard)
        {
            if (player == null) continue;
            
            // Get current modifier
            if (!currentSpeedModifiers.ContainsKey(player))
            {
                currentSpeedModifiers[player] = 1f;
            }
            
            // Lerp towards target modifier
            float targetModifier = 1f - SlowFactor;
            currentSpeedModifiers[player] = Mathf.Lerp(
                currentSpeedModifiers[player],
                targetModifier,
                Time.deltaTime * slowTransitionSpeed
            );
            
            // Apply modifier
            float modifier = currentSpeedModifiers[player];
            if (originalGroundSpeeds.ContainsKey(player))
            {
                player.maxGroundMoveSpeed = originalGroundSpeeds[player] * modifier;
                player.maxAirMoveSpeed = originalAirSpeeds[player] * modifier;
            }
        }
        
        // Update vision overlay
        UpdateVisionOverlay();
    }
    
    private void OnDestroy()
    {
        // Cleanup vision overlay
        if (visionOverlay != null)
        {
            Destroy(visionOverlay);
        }
    }
    
    #endregion

    #region Trigger Handling
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (playersInHazard.Contains(player)) return;
        
        // Store original speeds
        originalGroundSpeeds[player] = player.maxGroundMoveSpeed;
        originalAirSpeeds[player] = player.maxAirMoveSpeed;
        currentSpeedModifiers[player] = 1f;
        
        playersInHazard.Add(player);
        
        // Start effects
        if (impairVision)
        {
            EnableVisionOverlay();
        }
        
        // Audio
        if (enterSound != null)
        {
            AudioSource.PlayClipAtPoint(enterSound, player.transform.position);
        }
        
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        Debug.Log($"[StickyTrashHazard] {other.name} entered sticky trash");
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        
        if (!playersInHazard.Contains(player)) return;
        
        // Restore original speeds
        if (originalGroundSpeeds.ContainsKey(player))
        {
            player.maxGroundMoveSpeed = originalGroundSpeeds[player];
            player.maxAirMoveSpeed = originalAirSpeeds[player];
            
            originalGroundSpeeds.Remove(player);
            originalAirSpeeds.Remove(player);
            currentSpeedModifiers.Remove(player);
        }
        
        playersInHazard.Remove(player);
        
        // Disable effects if no players remain
        if (playersInHazard.Count == 0)
        {
            DisableVisionOverlay();
            
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
        
        Debug.Log($"[StickyTrashHazard] {other.name} exited sticky trash");
    }
    
    #endregion

    #region Vision Overlay
    
    private void EnableVisionOverlay()
    {
        if (!impairVision) return;
        if (visionOverlay != null) return;
        
        // Create overlay canvas
        visionOverlay = new GameObject("StickyTrashOverlay");
        Canvas canvas = visionOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // Add canvas group for alpha control
        overlayCanvasGroup = visionOverlay.AddComponent<CanvasGroup>();
        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.blocksRaycasts = false;
        overlayCanvasGroup.interactable = false;
        
        // Create image
        GameObject imageObj = new GameObject("OverlayImage");
        imageObj.transform.SetParent(visionOverlay.transform, false);
        
        UnityEngine.UI.Image image = imageObj.AddComponent<UnityEngine.UI.Image>();
        image.color = overlayColor;
        
        // Fill screen
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
    
    private void UpdateVisionOverlay()
    {
        if (overlayCanvasGroup == null) return;
        
        float targetAlpha = playersInHazard.Count > 0 ? 1f : 0f;
        overlayCanvasGroup.alpha = Mathf.Lerp(
            overlayCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * slowTransitionSpeed
        );
        
        // Cleanup when faded out
        if (playersInHazard.Count == 0 && overlayCanvasGroup.alpha < 0.01f)
        {
            DisableVisionOverlay();
        }
    }
    
    private void DisableVisionOverlay()
    {
        if (visionOverlay != null)
        {
            Destroy(visionOverlay);
            visionOverlay = null;
            overlayCanvasGroup = null;
        }
    }
    
    #endregion

    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.5f);
        
        // Draw collider bounds
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
    
    #endregion
}
