using UnityEngine;
using UnityEngine.UI;

public class TreasureUI : MonoBehaviour
{
    public Text treasureText;           // legacy UI Text OR TextMeshPro - adapt if using TMP
    public Slider progressBar;          // optional progress bar (0..1)
    public GameObject allCollectedPanel; // optional celebration UI

    void Start()
    {
        if (TreasureManager.Instance != null)
        {
            TreasureManager.Instance.OnTreasureChanged += UpdateUI;
            TreasureManager.Instance.OnAllCollected += OnAllCollected;
            // initialize
            UpdateUI(TreasureManager.Instance.totalCollected, TreasureManager.Instance.totalNeeded);
        }
    }

    void OnDestroy()
    {
        if (TreasureManager.Instance != null)
        {
            TreasureManager.Instance.OnTreasureChanged -= UpdateUI;
            TreasureManager.Instance.OnAllCollected -= OnAllCollected;
        }
    }

    void UpdateUI(int collected, int needed)
    {
        if (treasureText != null) treasureText.text = $"Treasure: {collected}/{needed}";
        if (progressBar != null) progressBar.value = Mathf.Clamp01((float)collected / Mathf.Max(1, needed));
    }

    void OnAllCollected()
    {
        if (allCollectedPanel != null) allCollectedPanel.SetActive(true);
    }
}
