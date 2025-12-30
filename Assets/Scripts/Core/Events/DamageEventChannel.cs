using System.Collections.Generic;
using UnityEngine;

namespace GUP.Core.Events
{
    /// <summary>
    /// Event channel that broadcasts damage events with DamageData.
    /// Use for OnPlayerDamaged, OnEntityDamaged, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEventChannel", menuName = "GUP/Events/Damage Event Channel")]
    public class DamageEventChannel : GameEventSO
    {
        private readonly List<System.Action<GameObject, DamageData>> listeners = new();
        
        /// <summary>
        /// Raise the event with entity and damage data.
        /// </summary>
        public void Raise(GameObject entity, DamageData damage)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i]?.Invoke(entity, damage);
            }
        }
        
        /// <summary>
        /// Subscribe to this event.
        /// </summary>
        public void Subscribe(System.Action<GameObject, DamageData> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Unsubscribe from this event.
        /// </summary>
        public void Unsubscribe(System.Action<GameObject, DamageData> listener)
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
