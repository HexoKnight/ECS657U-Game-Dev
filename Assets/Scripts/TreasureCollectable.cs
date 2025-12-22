using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreasureCollectible : MonoBehaviour
{
    [Header("Base Model")]
    public GameObject baseModel;
    [Tooltip("Adjust the Initial Model Rotation")]
    public Vector3 modelRotation = new Vector3();
    [Tooltip("Adjust the Initial Model Scale")]
    public float modelScale = 1f;
    public Color lightColor = Color.white;
    public float lightIntensity = 10;

    [Header("Pickup")]
    public int value = 1;
    public AudioClip pickupSound;
    public ParticleSystem pickupParticles;

    [Header("Animation")]
    public float rotateSpeed = 50f;
    public float bobAmplitude = 0.15f;
    public float bobSpeed = 1.2f;
    Vector3 _startPos;

    [Header("Runtime")]
    public bool usePooling = false; // set true if using pooling

    void Awake()
    {
        _startPos = transform.localPosition;
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // add the model instead of the white sphere
        if (baseModel == null) {
            GetComponent<MeshRenderer>().enabled = true;
        }
        else {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Light>().color = lightColor;
            GetComponent<Light>().intensity = lightIntensity;
            baseModel.transform.localScale = Vector3.one * modelScale;
            baseModel.transform.localRotation = Quaternion.Euler(modelRotation);
            Instantiate(baseModel, transform);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        transform.localPosition = _startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
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

