using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI;

#endif

// primarily exists to allow buttons to lock the cursor:
// https://docs.unity3d.com/2023.1/Documentation/Manual/webgl-cursorfullscreen.html
// which, TLDR, requires an additional user input after activating the button
// in this case the additional input is the user releasing the mouse button
public class ImmediateButton : Button
{
    [Tooltip("Whether to 'trigger' this button immediately on click (ie. on `OnPointerDown` rather than the default `OnPointerClick`)")]
    public bool immediate;

    public override void OnPointerDown(PointerEventData pointerEventData)
    {
        if (immediate) base.OnPointerClick(pointerEventData);
    }

    public override void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!immediate) base.OnPointerClick(pointerEventData);
    }
}

#if UNITY_EDITOR

// mirroring:
// https://github.com/needle-mirror/com.unity.ugui/blob/1.0.0/Unity-2021.1.17f1/Editor/UI/ButtonEditor.cs
[CustomEditor(typeof(ImmediateButton), true)]
public class CustomLevelButtonEditor : ButtonEditor
{
    private SerializedProperty immediateProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        immediateProperty = serializedObject.FindProperty("immediate");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(immediateProperty);
        serializedObject.ApplyModifiedProperties();
    }
}

#endif
