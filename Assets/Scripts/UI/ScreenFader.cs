using System.Collections;

using UnityEngine;
using Eflatun.SceneReference;

public class ScreenFader : MonoBehaviour
{
    [Tooltip("The canvas to enable/disable when fading")]
    public Canvas canvas;

    [Tooltip("Image in the canvas that fills the screen")]
    public UnityEngine.UI.Graphic image;

    private Coroutine _currentCoroutine = null;

    public void FadeScreen(Color colour, float duration, System.Action callback)
    {
        if (_currentCoroutine != null)
        {
            Debug.LogError("cannot fade screen: screen already fading!");
        }

        Color currentColour = colour;
        currentColour.a = 0;
        image.color = currentColour;
        canvas.enabled = true;

        _currentCoroutine = StartCoroutine(LerpTimeCoroutine.Create(
            duration,
            t =>
            {
                currentColour.a = t;
                image.color = currentColour;
            },
            () =>
            {
                _currentCoroutine = null;
                callback();
            }
        ));
    }

    public void FadeToScene(Color colour, float duration, SceneReference loadScene)
    {
        (IEnumerator coroutine, var startSceneActivation) =
            SceneLoader.LoadScene(loadScene, null, null);
        StartCoroutine(coroutine);

        FadeScreen(colour, duration, () => startSceneActivation());
    }
}
