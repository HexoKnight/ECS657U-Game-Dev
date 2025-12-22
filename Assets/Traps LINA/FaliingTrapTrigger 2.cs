using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    public Rigidbody2D rb; // Or Rigidbody for 3D

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
