using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spiky fish that patrols and shoots spines when player approaches.
/// Inflates before shooting, creating a "mobile turret" behavior.
/// </summary>
public class SpikyFishEnemy : EnemyBase
{
    #region Spine Shooting Settings
    
    [Header("Spine Shooting")]
    [Tooltip("Prefab for spine projectile")]
    [SerializeField] private GameObject spinePrefab;
    
    [Tooltip("Number of spines to shoot per burst")]
    [SerializeField] private int spinesPerBurst = 3;
    
    [Tooltip("Time between spine shots in a burst")]
    [SerializeField] private float spineShootInterval = 0.3f;
    
    [Tooltip("Spread angle for spines in degrees")]
    [SerializeField] private float spineSpreadAngle = 30f;
    
    [Tooltip("Damage per spine")]
    [SerializeField] private float spineDamage = 1f;
    
    [Tooltip("Knockback per spine")]
    [SerializeField] private float spineKnockback = 5f;
    
    [Header("Puff Behavior")]
    [Tooltip("Duration of puffed state")]
    [SerializeField] private float puffDuration = 2f;
    
    [Tooltip("Cooldown before can puff again")]
    [SerializeField] private float puffCooldown = 4f;
    
    [Tooltip("Detection range to trigger puffing")]
    [SerializeField] private float puffTriggerRange = 6f;
    
    [Tooltip("Scale multiplier when puffed")]
    [SerializeField] private float puffScaleMultiplier = 1.5f;
    
    #endregion

    #region Private State
    
    private Vector3 originalScale;
    private float lastPuffTime;
    private bool isPuffed;
    
    #endregion

    #region Properties
    
    public int SpinesPerBurst => spinesPerBurst;
    public float SpineShootInterval => spineShootInterval;
    public float PuffDuration => puffDuration;
    
    #endregion

    #region Lifecycle
    
    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        
        // Spiky fish uses patrol but doesn't chase
        usePatrol = true;
        useAlertState = false;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Check for puff trigger during patrol
        if (!isPuffed && !isDead && Time.time - lastPuffTime >= puffCooldown)
        {
            if (playerDetector != null && playerDetector.IsPlayerInRange(puffTriggerRange))
            {
                TriggerPuff();
            }
        }
    }
    
    private void TriggerPuff()
    {
        isPuffed = true;
        lastPuffTime = Time.time;
        stateMachine.ChangeState(new PuffedUpState(stateMachine, this));
    }
    
    #endregion

    #region Puff Visuals
    
    public void StartPuffVisuals()
    {
        isPuffed = true;
    }
    
    public void UpdatePuffVisuals(float elapsed)
    {
        // Animate scale during puff
        float progress = elapsed / (puffDuration * 0.3f); // Scale up in first 30%
        progress = Mathf.Clamp01(progress);
        
        Vector3 targetScale = originalScale * puffScaleMultiplier;
        transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
    }
    
    public void EndPuffVisuals()
    {
        isPuffed = false;
        transform.localScale = originalScale;
    }
    
    #endregion

    #region Spine Shooting
    
    public void ShootSpine()
    {
        if (spinePrefab == null)
        {
            Debug.LogWarning($"[SpikyFishEnemy] {name} has no spine prefab assigned!");
            return;
        }
        
        if (playerDetector == null || playerDetector.Player == null) return;
        
        // Calculate direction to player with spread
        Vector3 baseDirection = playerDetector.GetDirectionToPlayer();
        float randomSpread = Random.Range(-spineSpreadAngle * 0.5f, spineSpreadAngle * 0.5f);
        Quaternion spreadRotation = Quaternion.Euler(0, randomSpread, 0);
        Vector3 shootDirection = spreadRotation * baseDirection;
        
        // Spawn spine
        Vector3 spawnPos = transform.position + baseDirection * 0.5f; // Offset from center
        GameObject spine = Instantiate(spinePrefab, spawnPos, Quaternion.identity);
        
        // Initialize projectile
        SpineProjectile projectile = spine.GetComponent<SpineProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(shootDirection, gameObject, spineDamage, spineKnockback);
        }
        else
        {
            // Fallback: just set velocity on rigidbody
            Rigidbody rb = spine.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootDirection * 15f;
            }
        }
        
        if (debugStates)
        {
            Debug.Log($"[SpikyFishEnemy] {name} shot a spine!");
        }
    }
    
    #endregion

    #region Attack Override
    
    public override void PerformAttack()
    {
        // Spiky fish doesn't do melee attacks, only spine shooting
        // This override prevents base class attack behavior
    }
    
    #endregion

    #region Gizmos
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Puff trigger range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, puffTriggerRange);
    }
    
    #endregion
}
