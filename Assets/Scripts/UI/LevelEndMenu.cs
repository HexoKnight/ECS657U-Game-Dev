using UnityEngine;
using Eflatun.SceneReference;

public class LevelEndMenu : MonoBehaviour
{
    [Header("Continue Scene")]
    [Tooltip("The scene to load into on 'Continue'")]
    public SceneReference loadScene;
    [Tooltip("Fade to continue scene colour")]
    public Color loadSceneFadeColour = Color.black;
    [Tooltip("Fade to continue scene duration")]
    [Min(0)] public float loadSceneFadeDuration = 1f;

    [Header("Start Scene")]
    [Tooltip("The scene to load into on 'Quit To Title'")]
    public SceneReference startScene;
    [Tooltip("Fade to start scene colour")]
    public Color startSceneFadeColour = Color.black;
    [Tooltip("Fade to start scene duration")]
    [Min(0)] public float startSceneFadeDuration = 1f;

    [Header("Other")]

    public TMPro.TMP_Text treasureText;

    [Tooltip("The canvas group to fade in when the level ends")]
    public CanvasGroup canvasGroup;

    [Tooltip("Screen open duration")]
    [Min(0)] public float openDuration = 1f;

    [Tooltip("ScreenFader in current scene")]
    public ScreenFader screenFader;

    void OnEnable()
    {
        treasureText.text = $"Treasure Collected:\n{TreasureManager.Instance.totalCollected}/{TreasureManager.Instance.totalNeeded}";

        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
        StartCoroutine(LerpTimeCoroutine.Create(
            openDuration,
            t =>
            {
                canvasGroup.alpha = t;
            },
            ShowEndScreen
        ));
    }

    private void ShowEndScreen()
    {
        // incredibly hacky but should work
        PlayerInputs playerInputs = FindFirstObjectByType<PlayerInputs>();
        playerInputs.SetPaused(true);
        playerInputs.enabled = false;
        playerInputs.gameObject.SetActive(false);
        FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include).gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void Continue()
    {
        canvasGroup.interactable = false;

        screenFader.FadeToScene(loadSceneFadeColour, loadSceneFadeDuration, loadScene);
    }

    public void QuitToTitle()
    {
        canvasGroup.interactable = false;

        screenFader.FadeToScene(startSceneFadeColour, startSceneFadeDuration, startScene);
    }
}
