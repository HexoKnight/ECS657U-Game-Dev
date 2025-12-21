using UnityEngine;
using UnityEngine.Events;

namespace GUP.Core
{
    /// <summary>
    /// Static options container with observable values for settings synchronization.
    /// </summary>
    public static class Options
    {
        /// <summary>
        /// Generic observable value that invokes listeners when changed.
        /// </summary>
        /// <typeparam name="T">The type of value to watch.</typeparam>
        public class WatchableValue<T> : UnityEvent<T>
        {
            private T _value;
            
            /// <summary>Gets or sets the value, invoking listeners on change.</summary>
            public T Value
            {
                get => _value;
                set
                {
                    _value = value;
                    Invoke(_value);
                }
            }

            private WatchableValue(T value) => _value = value;
            public static implicit operator WatchableValue<T>(T value) => new(value);
            public static implicit operator T(WatchableValue<T> watchablevalue) => watchablevalue.Value;
        }

        /// <summary>Mouse sensitivity multiplier (default 1.0)</summary>
        public static readonly WatchableValue<float> mouseSensitivity = 1.0f;
        
        /// <summary>Current graphics quality level</summary>
        public static readonly WatchableValue<int> graphicsQuality = QualitySettings.GetQualityLevel();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Startup()
        {
            graphicsQuality.AddListener(QualitySettings.SetQualityLevel);
        }
    }
}
