using System.Collections.Generic;
using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Event channel that broadcasts events with a Vector3 parameter.
    /// Use for events like OnPlayerRespawned, OnCheckpointReached, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector3EventChannel", menuName = "GUP/Events/Vector3 Event Channel")]
    public class Vector3EventChannel : GameEventSO
    {
        private readonly List<System.Action<Vector3>> listeners = new();
        
        /// <summary>
        /// Raise the event with a Vector3 value.
        /// </summary>
        public void Raise(Vector3 value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Subscribe to this event.
        /// </summary>
        public void Subscribe(System.Action<Vector3> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from this event.
        /// </summary>
        public void Unsubscribe(System.Action<Vector3> listener)
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
