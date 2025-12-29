using UnityEngine;
using UnityEngine.Events;

namespace GUP.Core.Events
{
    /// <summary>
    /// MonoBehaviour that listens to a VoidEventChannel and invokes a UnityEvent response.
    /// Attach to GameObjects to respond to events in the inspector.
    /// </summary>
    public class VoidEventListener : MonoBehaviour
    {
        [Tooltip("The event channel to listen to")]
        [SerializeField] private VoidEventChannel eventChannel;
        
        [Tooltip("Response to invoke when event is raised")]
        [SerializeField] private UnityEvent onEventRaised;
        
        private void OnEnable()
        {
            if (eventChannel != null)
            {
                eventChannel.Subscribe(OnEventRaised);
            }
        }
        
        private void OnDisable()
        {
            if (eventChannel != null)
            {
                eventChannel.Unsubscribe(OnEventRaised);
            }
        }
        
        private void OnEventRaised()
        {
            onEventRaised?.Invoke();
        }
    }
}
