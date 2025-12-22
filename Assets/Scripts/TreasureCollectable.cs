using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreasureCollectible : MonoBehaviour
{
    [Tooltip("Child object that will bob and rotate")]
    public Transform child;

    [Header("Pickup")]
    public int value = 1;
    public AudioClip pickupSound;
    public ParticleSystem pickupParticles;

    [Header("Animation")]
    public float rotateSpeed = 50f;
    public float bobAmplitude = 0.15f;
    public float bobSpeed = 1.2f;

    [Header("Runtime")]
    public bool usePooling = false; // set true if using pooling

    void Update()
    {
        child.Rotate(transform.up, rotateSpeed * Time.deltaTime, Space.World);
        child.localPosition = Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;

        // Add to manager
        TreasureManager.Instance?.AddTreasure(value);

        // Play audio
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Particle
        if (pickupParticles != null)
        {
            // play then detach so it persists while we destroy/hide object
            var ps = Instantiate(pickupParticles, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        // Hide/destroy or return to pool
        if (usePooling)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

