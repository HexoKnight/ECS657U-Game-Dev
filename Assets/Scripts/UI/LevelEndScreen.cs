using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LevelEndScreen : MonoBehaviour
{
    [Tooltip("GameObject to make active when level ends")]
    public GameObject levelEndMenu;

    private bool levelEnded = false;

    void OnTriggerEnter(Collider other)
    {
        if (levelEnded || other.GetComponent<PlayerController>() == null) return;

        levelEnded = true;
        levelEndMenu.SetActive(true);
    }
}
