using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public class PauseMenu : MonoBehaviour
{
    [Tooltip("The scene to load into on 'Quit to Title'")]
    public SceneReference loadScene;

    [Tooltip("GameObject to make active when 'Options' is pressed")]
    public GameObject optionsObject;

    public void Resume()
    {
        // TODO: improve
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();

        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                source.UnPause();
            }
		}
        FindFirstObjectByType<PlayerInputs>(FindObjectsInactive.Include).SetPaused(false);
    }

    public void OpenOptions()
    {
        optionsObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void QuitToTitle()
    {
        SceneManager.LoadScene(loadScene.BuildIndex);
    }
}
