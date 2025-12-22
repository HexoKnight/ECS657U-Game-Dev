#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLNative
{
    [DllImport("__Internal")]
    public static extern void RegisterCursorLockChangedCallback(System.Action<bool> callback);

    // attribute allows for passing method to JS
    [AOT.MonoPInvokeCallback(typeof(System.Action<bool>))]
    private static void SetCursorLockState(bool locked) =>
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Startup()
    {
        // we have no WebGLInput.stickyCursorLock in this unity version afaict but this will suffice
        // https://docs.unity3d.com/2023.1/Documentation/Manual/webgl-cursorfullscreen.html
        RegisterCursorLockChangedCallback(SetCursorLockState);
    }
}

#endif
