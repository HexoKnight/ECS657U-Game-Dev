using UnityEngine.Events;

public static class Options
{
    public class WatchableValue<T> : UnityEvent<T>
    {
        private T _value;
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

    public static readonly WatchableValue<float> mouseSensitivity = 1.0f;
}
