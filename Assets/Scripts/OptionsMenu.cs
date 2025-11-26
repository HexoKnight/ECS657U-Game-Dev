using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [Tooltip("GameObject to make active when 'Back' is pressed")]
    public GameObject prevObject;

    public void Back()
    {
        prevObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
