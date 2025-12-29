using UnityEngine;

public class StartScreen : MonoBehaviour
{
    [Tooltip("Transform to rotate for camera pivoting")]
    public Transform cameraPivot;

    [Tooltip("The speed with which to rotate the camera pivot")]
    public float pivotSpeed = 10f;

    private void LateUpdate()
    {
        cameraPivot.RotateAround(cameraPivot.transform.position, cameraPivot.transform.up, pivotSpeed * Time.deltaTime);
    }
}
