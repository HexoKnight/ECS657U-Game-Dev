using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class StartMenu : MonoBehaviour
{
    [Tooltip("The scene to load into on 'Start Game'")]
    public SceneReference loadScene;

    [Tooltip("GameObject to make active when 'Options' is pressed")]
    public GameObject optionsObject;

    private AsyncOperation _sceneLoadOperation;

    // was the scene loaded the last time we checked
    private bool _wasSceneLoaded = false;

    // stops at 90% if `allowSceneActivation` == false:
    // https://docs.unity3d.com/2023.1/Documentation/ScriptReference/AsyncOperation-progress.html
    private bool IsSceneLoaded => _sceneLoadOperation.progress >= 0.9f;

    void Start()
    {
        _sceneLoadOperation = SceneManager.LoadSceneAsync(loadScene.BuildIndex);
        _sceneLoadOperation.allowSceneActivation = false;
    }

    void Update()
    {
        if (!_wasSceneLoaded && IsSceneLoaded)
        {
            Debug.Log("scene loading finished in the background!");
            _wasSceneLoaded = true;
        }
    }

    public void StartGame()
    {
        _sceneLoadOperation.allowSceneActivation = true;
        // TODO: maybe fading
        if (!IsSceneLoaded)
        {
            // loading bar...
        }
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
