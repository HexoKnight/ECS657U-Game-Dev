using System.Collections.Generic;
using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Event channel that broadcasts events with no parameters.
    /// Use for simple signals like OnPlayerDied, OnLevelCompleted, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "GUP/Events/Void Event Channel")]
    public class VoidEventChannel : GameEventSO
    {
        private readonly List<System.Action> listeners = new();
        
        /// <summary>
        /// Raise the event, notifying all listeners.
        /// </summary>
        public void Raise()
        {
            // Iterate backwards in case listeners unsubscribe during callback
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke();
            }
        }
        
        /// <summary>
        /// Subscribe to this event.
        /// </summary>
        public void Subscribe(System.Action listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from this event.
        /// </summary>
        public void Unsubscribe(System.Action listener)
        {
            listeners.Remove(listener);
        }
        
        /// <summary>
        /// Clear all listeners. Use when changing scenes.
        /// </summary>
        public void Clear()
        {
            listeners.Clear();
        }
    }
}
