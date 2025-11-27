using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Tooltip("GameObject to make active when 'Back' is pressed")]
    public GameObject prevObject;

    [Tooltip("Slider representing mouse sensitivity")]
    public Slider mouseSensitivitySlider;

    private void Awake()
    {
        // like standard linear interpolation but t is between t_min and t_max rather than 0 and 1
        static float Lerp(float t, float t_min, float t_max, float r_min, float r_max) =>
            (t - t_min) / (t_max - t_min) * (r_max - r_min) + r_min;

        {
            // === mouse sensitivity === //
            const float MIN = 0.5f;
            const float MAX = 2.5f;

            float sliderToMouseSensitivity(float sliderValue) =>
                Lerp(sliderValue, mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue, MIN, MAX);
            float mouseSensitivityToSlider(float mouseSensitivity) =>
                Lerp(mouseSensitivity, MIN, MAX, mouseSensitivitySlider.minValue, mouseSensitivitySlider.maxValue);

            mouseSensitivitySlider.value = mouseSensitivityToSlider(Options.mouseSensitivity);
            mouseSensitivitySlider.onValueChanged.AddListener(sliderValue =>
                Options.mouseSensitivity.Value = sliderToMouseSensitivity(sliderValue)
            );
            Options.mouseSensitivity.AddListener(mouseSensitivity =>
                // don't notify or infinite loop
                mouseSensitivitySlider.SetValueWithoutNotify(mouseSensitivityToSlider(mouseSensitivity))
            );
        }
    }

    public void Back()
    {
        prevObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
