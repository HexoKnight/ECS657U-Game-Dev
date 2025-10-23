using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // inital code mostly from:
    // https://discussions.unity.com/t/simplest-character-controlling-code-in-unity-6-3d/1597930/4

    public new Rigidbody rigidbody;
    public new Camera camera;
    public float Right;
    public float Left;
    public float moveSpeed;
    public float mouseSensitivity;
    public float deadZone;

    bool isGrounded = false;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            transform.RotateAround(transform.position, transform.up, mouseSensitivity * Input.GetAxis("Mouse X"));
            camera.transform.RotateAround(camera.transform.position, transform.right, mouseSensitivity * -Input.GetAxis("Mouse Y"));
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 10, ForceMode.Impulse);
        }

        if (transform.position.y < deadZone)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        float moveForceMult = 1.0f;
        if (isGrounded) moveForceMult *= 3;

        if (Input.GetKey(KeyCode.W)) rigidbody.AddRelativeForce(Vector3.forward * 10 * moveForceMult, ForceMode.Force);
        if (Input.GetKey(KeyCode.A)) rigidbody.AddRelativeForce(Vector3.left * 7 * moveForceMult, ForceMode.Force);
        if (Input.GetKey(KeyCode.D)) rigidbody.AddRelativeForce(Vector3.right * 7 * moveForceMult, ForceMode.Force);
        if (Input.GetKey(KeyCode.S)) rigidbody.AddRelativeForce(Vector3.back * 9 * moveForceMult, ForceMode.Force);
        if (Input.GetKey(KeyCode.Q)) transform.RotateAround(transform.position, transform.up, -100 * Time.deltaTime);
        if (Input.GetKey(KeyCode.E)) transform.RotateAround(transform.position, transform.up, 100 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
