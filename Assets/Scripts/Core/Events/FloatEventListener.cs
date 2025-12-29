using UnityEngine;
using UnityEngine.Events;

namespace GUP.Core.Events
{
    /// <summary>
    /// MonoBehaviour that listens to a FloatEventChannel and invokes a UnityEvent response.
    /// </summary>
    public class FloatEventListener : MonoBehaviour
    {
        [Tooltip("The event channel to listen to")]
        [SerializeField] private FloatEventChannel eventChannel;
        
        [Tooltip("Response to invoke when event is raised")]
        [SerializeField] private UnityEvent<float> onEventRaised;
        
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
        
        private void OnEventRaised(float value)
        {
            onEventRaised?.Invoke(value);
        }
    }
}
