using UnityEngine;

public class ExplodingFishEnemy : EnemyBase
{
    [Header("Explosion Settings")]
    public float detectionRadius = 10f;
    public float explosionRadius = 5f;
    public float explosionDelay = 1.5f;
    public float chaseSpeedMultiplier = 2f;
    
    [Header("Visuals")]
    public GameObject explosionEffectPrefab;
    public Color warningColor = Color.red;

    private Transform playerTransform;
    private bool isChasing = false;
    private bool isExploding = false;
    private float explodeTimer = 0f;
    private Renderer rend;
    private Color originalColor;

    public override void OnSpawn()
    {
        base.OnSpawn();
        rend = GetComponent<Renderer>();
        if (rend != null) originalColor = rend.material.color;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null) playerTransform = player.transform;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (isExploding)
        {
            explodeTimer -= Time.deltaTime;
            // Flash color
            if (rend != null)
            {
                float t = Mathf.PingPong(Time.time * 10f, 1f);
                rend.material.color = Color.Lerp(originalColor, warningColor, t);
            }

            if (explodeTimer <= 0)
            {
                Explode();
            }
            return;
        }

        if (distanceToPlayer < detectionRadius)
        {
            isChasing = true;
        }

        if (isChasing)
        {
            // Chase
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * chaseSpeedMultiplier * GlobalSpeedMultiplier * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);

            if (distanceToPlayer < explosionRadius * 0.5f) // Close enough to start detonation
            {
                StartExplosionSequence();
            }
        }
    }

    private void StartExplosionSequence()
    {
        if (isExploding) return;
        isExploding = true;
        explodeTimer = explosionDelay;
    }

    private void Explode()
    {
        // Deal damage in radius
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                     damageable.TakeDamage(contactDamage * 5f, transform.position, (hit.transform.position - transform.position).normalized);
                }
            }
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        OnDeath();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
