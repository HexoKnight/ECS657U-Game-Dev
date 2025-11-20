using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;

public class DollyTrackMount : MonoBehaviour
{
    [Tooltip("Virtual camera for the 'cart'.")]
    public CinemachineCamera splineCam;

    [Tooltip("Speed of the cart in seconds.")]
    public float rideSpeed = 6f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            CinemachineCamera playerCam = playerController.GetComponentInChildren<CinemachineCamera>();

            splineCam.Target = new()
            {
                TrackingTarget = playerController.transform,
                LookAtTarget = playerController.transform,
                CustomLookAtTarget = true,
            };

            PrioritySettings oldPriority = splineCam.Priority;
            splineCam.Priority = playerCam.Priority + 1;

            void callback()
            {
                splineCam.Priority = oldPriority;
                splineCam.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            playerController.StartStaticSplineStart(GetComponent<SplineContainer>(), rideSpeed, callback);
        }
    }
}
