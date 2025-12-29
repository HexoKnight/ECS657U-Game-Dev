using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Base class for all ScriptableObject-based event channels.
    /// Provides a decoupled, inspector-configurable event system.
    /// </summary>
    public abstract class GameEventSO : ScriptableObject
    {
        #if UNITY_EDITOR
        [TextArea(1, 3)]
        [Tooltip("Optional description for this event (editor only)")]
        [SerializeField] private string description;
        #endif
    }
}
