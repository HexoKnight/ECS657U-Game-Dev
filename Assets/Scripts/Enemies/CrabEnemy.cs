using UnityEngine;

public class CrabEnemy : EnemyBase
{
    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    public float waitTime = 1f;

    private Vector3 targetPosition;
    private float waitTimer;
    private bool movingToB = true;

    public override void OnSpawn()
    {
        base.OnSpawn();
        if (pointA != null) targetPosition = pointA.position;
        else targetPosition = transform.position;
    }

    private void Update()
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
}
