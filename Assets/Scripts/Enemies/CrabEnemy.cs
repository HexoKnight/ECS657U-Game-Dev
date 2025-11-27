using UnityEngine;

public class CrabEnemy : EnemyBase
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float waitTime = 1f;

    [Header("Chase Settings")]
    public float detectionRadius = 5f;
    public float chaseRadius = 8f;
    public float chaseSpeed = 3f;

    private Vector3 targetPosition;
    private float waitTimer;
    private bool movingToB = true;
    private Transform playerTransform;
    private bool isChasing = false;

    public override void OnSpawn()
    {
        base.OnSpawn();
        if (pointA != null) targetPosition = pointA.position;
        else targetPosition = transform.position;

        // Find player
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) playerTransform = pc.transform;
        }

        if (isChasing)
        {
            ChaseBehavior();
        }
        else
        {
            PatrolBehavior();
            CheckForPlayer();
        }
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null) return;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance < detectionRadius)
        {
            isChasing = true;
        }
    }

    private void ChaseBehavior()
    {
        if (playerTransform == null)
        {
            isChasing = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance > chaseRadius)
        {
            isChasing = false;
            // Return to nearest patrol point
            if (pointA != null && pointB != null)
            {
                float distA = Vector3.Distance(transform.position, pointA.position);
                float distB = Vector3.Distance(transform.position, pointB.position);
                targetPosition = distA < distB ? pointA.position : pointB.position;
                movingToB = (targetPosition == pointB.position);
            }
            return;
        }

        // Move towards player
        float step = chaseSpeed * GlobalSpeedMultiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);

        // Face player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Keep rotation upright
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void PatrolBehavior()
    {
        if (pointA == null || pointB == null) return;

        // Simple patrol logic
        float step = moveSpeed * GlobalSpeedMultiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // Face direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                movingToB = !movingToB;
                targetPosition = movingToB ? pointB.position : pointA.position;
                waitTimer = 0f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
