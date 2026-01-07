using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using Eflatun.SceneReference;

public static class SceneLoader
{
    public delegate void StartSceneActivation();

    // scene started loading previously and that operation either hasn't started hasn't finished
    public static bool IsLoadingScene => isLoadingScene && lastLoadOperation?.isDone == false;

    private static bool isLoadingScene = false;
    private static AsyncOperation lastLoadOperation = null;

    public static (IEnumerator, StartSceneActivation) LoadScene(
        SceneReference loadScene,
        System.Action<float> updateProgress,
        System.Action<StartSceneActivation> loadingFinished
    )
    {
        if (IsLoadingScene)
        {
            Debug.LogError("cannot load scene: a scene is already loading!");
            return (null, null);
        }
        lastLoadOperation = null;
        isLoadingScene = true;

        AsyncOperation loadOperation = null;
        // in case startSceneActivation is called before the coroutine is started
        bool earlySceneActivation = false;

        void startSceneActivation()
        {
            if (loadOperation == null)
                earlySceneActivation = true;
            else
                loadOperation.allowSceneActivation = true;
        }

        // don't start scene loading until coroutine is started
        IEnumerator coroutine()
        {
            loadOperation = SceneManager.LoadSceneAsync(loadScene.BuildIndex);
            loadOperation.allowSceneActivation = earlySceneActivation;

            lastLoadOperation = loadOperation;

            // stops at 90% if `allowSceneActivation` == false:
            // https://docs.unity3d.com/2023.1/Documentation/ScriptReference/AsyncOperation-progress.html
            while (loadOperation.progress < 0.9f)
            {
                updateProgress?.Invoke(loadOperation.progress);
                yield return null;
            }

            loadingFinished?.Invoke(startSceneActivation);

            while (!loadOperation.isDone)
            {
                updateProgress?.Invoke(loadOperation.progress);
                yield return null;
            }
        }

        return (coroutine(), startSceneActivation);
    }
}
