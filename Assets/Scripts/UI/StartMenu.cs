using UnityEngine;
using Eflatun.SceneReference;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    [Tooltip("The scene to load into on 'Start Game'")]
    public SceneReference loadScene;

    [Tooltip("GameObject to make active when 'Options' is pressed")]
    public GameObject optionsObject;

    [Tooltip("ScreenFader in current scene")]
    public ScreenFader screenFader;

    [Tooltip("Screen fade colour")]
    public Color fadeColour = Color.black;
    [Tooltip("Screen fade duration")]
    [Min(0)] public float fadeDuration = 1f;

    private SceneLoader.StartSceneActivation _startSceneActivation;

    private bool _sceneLoaded = false;
    private bool _startPressed = false;

    void Start()
    {
        IEnumerator coroutine;
        (coroutine, _startSceneActivation) =
            SceneLoader.LoadScene(
                loadScene,
                null,
                startSceneActivation =>
                {
                    _sceneLoaded = true;
                    if (_startPressed)
                        FadeAndActivateScene();
                    else
                        Debug.Log("scene loading finished in the background!");
                }
            );

        StartCoroutine(coroutine);
    }

    private void FadeAndActivateScene()
    {
        screenFader.FadeScreen(fadeColour, fadeDuration, () => _startSceneActivation());
    }

    public void StartGame()
    {
        if (_startPressed) return;

        _startPressed = true;

        if (_sceneLoaded) FadeAndActivateScene();
    }

    public void OpenOptions()
    {
        optionsObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
