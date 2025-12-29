using System.Collections.Generic;
using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Event channel that broadcasts events with a GameObject parameter.
    /// Use for events like OnEnemyKilled, OnCheckpointActivated, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "GameObjectEventChannel", menuName = "GUP/Events/GameObject Event Channel")]
    public class GameObjectEventChannel : GameEventSO
    {
        private readonly List<System.Action<GameObject>> listeners = new();
        
        /// <summary>
        /// Raise the event with a GameObject reference.
        /// </summary>
        public void Raise(GameObject value)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Subscribe to this event.
        /// </summary>
        public void Subscribe(System.Action<GameObject> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from this event.
        /// </summary>
        public void Unsubscribe(System.Action<GameObject> listener)
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
