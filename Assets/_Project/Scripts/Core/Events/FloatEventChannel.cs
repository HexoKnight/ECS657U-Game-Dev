using System.Collections.Generic;
using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Event channel that broadcasts events with a float parameter.
    /// Use for events like OnDifficultyChanged, OnHealthChanged, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "FloatEventChannel", menuName = "GUP/Events/Float Event Channel")]
    public class FloatEventChannel : GameEventSO
    {
        private readonly List<System.Action<float>> listeners = new();
        
        /// <summary>
        /// Raise the event with a float value.
        /// </summary>
        public void Raise(float value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Subscribe to this event.
        /// </summary>
        public void Subscribe(System.Action<float> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from this event.
        /// </summary>
        public void Unsubscribe(System.Action<float> listener)
        {
            listeners.Remove(listener);
        }
        
        /// <summary>
        /// Clear all listeners.
        /// </summary>
        public void Clear()
        {
            listeners.Clear();
        }
    }
}
