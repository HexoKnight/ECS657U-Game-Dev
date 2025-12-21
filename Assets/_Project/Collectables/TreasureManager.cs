using UnityEngine;
using System;

public class TreasureManager : MonoBehaviour
{
    public static TreasureManager Instance { get; private set; }

    [Header("Goal")]
    public int totalNeeded = 10; // required to finish/trigger event
    public int totalCollected { get; private set; } = 0;

    public event Action<int, int> OnTreasureChanged; // (collected, needed)
    public event Action OnAllCollected;

    [Header("Persistence")]
    public bool saveProgress = true;
    public string saveKey = "TreasureCollected";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (saveProgress) Load();
    }

    public void AddTreasure(int amount = 1)
    {
        totalCollected += Mathf.Max(0, amount);
        OnTreasureChanged?.Invoke(totalCollected, totalNeeded);

        if (saveProgress) Save();

        if (totalCollected >= totalNeeded)
        {
            OnAllCollected?.Invoke();
        }

        Debug.Log($"TreasureManager: {totalCollected}/{totalNeeded}");
    }

    public void ResetProgress()
    {
        totalCollected = 0;
        if (saveProgress) PlayerPrefs.DeleteKey(saveKey);
        OnTreasureChanged?.Invoke(totalCollected, totalNeeded);
    }

    void Save()
    {
        PlayerPrefs.SetInt(saveKey, totalCollected);
        PlayerPrefs.Save();
    }

    void Load()
    {
        totalCollected = PlayerPrefs.GetInt(saveKey, 0);
        OnTreasureChanged?.Invoke(totalCollected, totalNeeded);
    }
}
