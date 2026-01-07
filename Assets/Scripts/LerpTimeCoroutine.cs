using System.Collections;

using UnityEngine;

public static class LerpTimeCoroutine
{
    public static IEnumerator Create(float duration, System.Action<float> progress, System.Action andThen)
    {
        float timeLeft = 0;

        while (timeLeft < duration)
        {
            timeLeft += Time.deltaTime;
            progress?.Invoke(System.Math.Min(1, timeLeft / duration));
            yield return null;
        }

        andThen?.Invoke();
    }
}
