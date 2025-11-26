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
        mouseSensitivitySlider.value = Options.mouseSensitivity;
        mouseSensitivitySlider.onValueChanged.AddListener(ChangeMouseSensitivity);
    }

    public void ChangeMouseSensitivity(float value)
    {
        const float MIN = 0.5f;
        const float MAX = 2.5f;

        Options.mouseSensitivity = (value - mouseSensitivitySlider.minValue) / mouseSensitivitySlider.maxValue * (MAX - MIN) + MIN;
    }

    public void Back()
    {
        prevObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
