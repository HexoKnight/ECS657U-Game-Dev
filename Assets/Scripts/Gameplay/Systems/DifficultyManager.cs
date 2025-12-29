using UnityEngine;

using GUP.Core;
/// <summary>
/// Dynamic Difficulty Adjustment (DDA) system that monitors player performance
/// and adjusts game difficulty in real-time.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    #region Singleton
    
    public static DifficultyManager Instance { get; private set; }
    
    #endregion

    #region Settings
    
    [Header("Difficulty Range")]
    [Tooltip("Minimum difficulty multiplier (easier)")]
    [SerializeField] private float minDifficulty = 0.5f;
    
    [Tooltip("Maximum difficulty multiplier (harder)")]
    [SerializeField] private float maxDifficulty = 1.5f;
    
    [Tooltip("Starting difficulty")]
    [SerializeField] private float startingDifficulty = 1f;
    
    [Header("Adjustment Settings")]
    [Tooltip("How quickly difficulty adjusts")]
    [SerializeField] private float adjustmentSpeed = 0.1f;
    
    [Tooltip("Deaths within this window affect difficulty")]
    [SerializeField] private float deathTrackingWindow = 60f;
    
    [Tooltip("Deaths threshold to lower difficulty")]
    [SerializeField] private int deathsToLowerDifficulty = 3;
    
    [Tooltip("Checkpoint time threshold (seconds) - faster = harder")]
    [SerializeField] private float checkpointTimeThreshold = 30f;
    
    [Header("Debug")]
    [Tooltip("Show difficulty debug UI")]
    [SerializeField] private bool showDebugUI = true;
    
    [Tooltip("Enable difficulty adjustment")]
    [SerializeField] private bool enableDDA = true;
    
    #endregion

    #region Private State
    
    private float currentDifficulty;
    private float targetDifficulty;
    
    // Tracking metrics
    private System.Collections.Generic.Queue<float> recentDeathTimes = new();
    private float lastCheckpointTime;
    private int totalDeaths;
    private int enemiesKilled;
    
    #endregion

    #region Properties
    
    /// <summary>Current difficulty multiplier (0.5 - 1.5)</summary>
    public float CurrentDifficulty => currentDifficulty;
    
    /// <summary>Current difficulty as percentage (0-100)</summary>
    public float DifficultyPercent => (currentDifficulty - minDifficulty) / (maxDifficulty - minDifficulty) * 100f;
    
    /// <summary>Deaths in the tracking window</summary>
    public int RecentDeaths => recentDeathTimes.Count;
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        currentDifficulty = startingDifficulty;
        targetDifficulty = startingDifficulty;
        lastCheckpointTime = Time.time;
    }
    
    private void OnEnable()
    {
        // Subscribe to events
        GameEvents.OnPlayerDied += OnPlayerDied;
        GameEvents.OnCheckpointActivated += OnCheckpointReached;
        GameEvents.OnEnemyKilled += OnEnemyKilled;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameEvents.OnPlayerDied -= OnPlayerDied;
        GameEvents.OnCheckpointActivated -= OnCheckpointReached;
        GameEvents.OnEnemyKilled -= OnEnemyKilled;
    }
    
    private void Update()
    {
        // Clean old death records
        CleanOldDeathRecords();
        
        // Evaluate and adjust difficulty
        if (enableDDA)
        {
            EvaluateDifficulty();
            
            // Smooth adjustment
            currentDifficulty = Mathf.Lerp(
                currentDifficulty,
                targetDifficulty,
                adjustmentSpeed * Time.deltaTime
            );
            
            // Apply to enemy multipliers
            ApplyDifficulty();
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugUI) return;
        
        // Debug UI in top-right corner
        GUILayout.BeginArea(new Rect(Screen.width - 250, 10, 240, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"<b>Difficulty Manager</b>");
        GUILayout.Label($"Current Difficulty: {currentDifficulty:F2}x ({DifficultyPercent:F0}%)");
        GUILayout.Label($"Target Difficulty: {targetDifficulty:F2}x");
        GUILayout.Label($"Recent Deaths: {RecentDeaths}");
        GUILayout.Label($"Total Deaths: {totalDeaths}");
        GUILayout.Label($"Enemies Killed: {enemiesKilled}");
        
        // Difficulty bar
        float barWidth = 220f;
        float fillWidth = barWidth * (currentDifficulty - minDifficulty) / (maxDifficulty - minDifficulty);
        GUI.Box(new Rect(10, 120, barWidth, 20), "");
        GUI.Box(new Rect(10, 120, fillWidth, 20), "");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    #endregion

    #region Event Handlers
    
    private void OnPlayerDied()
    {
        totalDeaths++;
        recentDeathTimes.Enqueue(Time.time);
        
        Debug.Log($"[DifficultyManager] Player died (Total: {totalDeaths}, Recent: {RecentDeaths})");
    }
    
    private void OnCheckpointReached(GameObject checkpoint)
    {
        float timeSinceLastCheckpoint = Time.time - lastCheckpointTime;
        lastCheckpointTime = Time.time;
        
        Debug.Log($"[DifficultyManager] Checkpoint reached in {timeSinceLastCheckpoint:F1}s");
        
        // Fast completion = increase difficulty slightly
        if (timeSinceLastCheckpoint < checkpointTimeThreshold && RecentDeaths == 0)
        {
            targetDifficulty += 0.05f;
        }
    }
    
    private void OnEnemyKilled(GameObject enemy)
    {
        enemiesKilled++;
    }
    
    #endregion

    #region Difficulty Logic
    
    private void CleanOldDeathRecords()
    {
        float cutoffTime = Time.time - deathTrackingWindow;
        
        while (recentDeathTimes.Count > 0 && recentDeathTimes.Peek() < cutoffTime)
        {
            recentDeathTimes.Dequeue();
        }
    }
    
    private void EvaluateDifficulty()
    {
        // Too many deaths = lower difficulty
        if (RecentDeaths >= deathsToLowerDifficulty)
        {
            targetDifficulty -= 0.1f * Time.deltaTime;
        }
        // No deaths in a while = slight increase
        else if (RecentDeaths == 0)
        {
            targetDifficulty += 0.02f * Time.deltaTime;
        }
        
        // Clamp to range
        targetDifficulty = Mathf.Clamp(targetDifficulty, minDifficulty, maxDifficulty);
    }
    
    private void ApplyDifficulty()
    {
        // Apply to enemy global multipliers
        EnemyBase.GlobalSpeedMultiplier = currentDifficulty;
        EnemyBase.GlobalDamageMultiplier = currentDifficulty;
        
        // Broadcast event
        GameEvents.RaiseDifficultyChanged(currentDifficulty);
    }
    
    #endregion

    #region Public API
    
    /// <summary>
    /// Manually set difficulty level (0-1 normalized).
    /// </summary>
    public void SetDifficulty(float normalized)
    {
        float difficulty = Mathf.Lerp(minDifficulty, maxDifficulty, normalized);
        targetDifficulty = difficulty;
        currentDifficulty = difficulty;
        ApplyDifficulty();
    }
    
    /// <summary>
    /// Reset difficulty to starting value.
    /// </summary>
    public void ResetDifficulty()
    {
        targetDifficulty = startingDifficulty;
        currentDifficulty = startingDifficulty;
        recentDeathTimes.Clear();
        ApplyDifficulty();
    }
    
    /// <summary>
    /// Toggle DDA system on/off.
    /// </summary>
    public void SetDDAEnabled(bool enabled)
    {
        enableDDA = enabled;
        
        if (!enabled)
        {
            // Reset to default when disabled
            EnemyBase.GlobalSpeedMultiplier = 1f;
            EnemyBase.GlobalDamageMultiplier = 1f;
        }
    }
    
    #endregion
}
