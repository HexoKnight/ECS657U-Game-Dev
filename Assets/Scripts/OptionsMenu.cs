using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Tooltip("GameObject to make active when 'Back' is pressed")]
    public GameObject prevObject;

    [Tooltip("'Mouse Sensitivity' slider")]
    public Slider mouseSensitivitySlider;

    [Tooltip("'Graphics Quality' dropdown")]
    public TMPro.TMP_Dropdown graphicsQualityDropdown;

    private void Awake()
    {
        // like standard linear interpolation but t is between t_min and t_max rather than 0 and 1
        static float Lerp(float t, float t_min, float t_max, float r_min, float r_max) =>
            (t - t_min) / (t_max - t_min) * (r_max - r_min) + r_min;

        static void SyncOption<E, M>(Options.WatchableValue<E> option, UnityEvent<M> menuEvent, Action<E> onOptionChange, Func<M, E> onMenuChange)
        {
            onOptionChange(option);
            menuEvent.AddListener(menuValue => option.Value = onMenuChange(menuValue));
            option.AddListener(new(onOptionChange));
        }

        {
            // === mouse sensitivity === //
            const float MIN = 0.5f;
            const float MAX = 2.5f;

            SyncOption(
                Options.mouseSensitivity,
                mouseSensitivitySlider.onValueChanged,
                // don't notify or infinite loop
                mouseSensitivity => mouseSensitivitySlider.SetValueWithoutNotify(
                    Lerp(mouseSensitivity, MIN, MAX, mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue)
                ),
                sliderValue =>
                    Lerp(sliderValue, mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue, MIN, MAX)
            );
        }
        {
            // === graphics quality === //

            graphicsQualityDropdown.options =
                QualitySettings.names
                .Select(name => new TMPro.TMP_Dropdown.OptionData(name))
                .ToList();

            SyncOption(
                Options.graphicsQuality,
                graphicsQualityDropdown.onValueChanged,
                // don't notify or infinite loop
                graphicsQualityDropdown.SetValueWithoutNotify,
                i => i
            );
        }
    }

    public void Back()
    {
        prevObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
