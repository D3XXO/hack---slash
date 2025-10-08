using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Light Attack Combo")]
    public float[] lightAttackDamage = { 10f, 15f, 20f };
    public float lightAttackCooldown = 0.3f;
    public float comboResetTime = 2f; // Now 2 seconds

    [Header("Burst Attack")]
    public float burstAttackDamage = 40f;
    public float burstAttackCooldown = 2f;
    public float burstAttackForce = 5f;

    [Header("Visual Effects")]
    public GameObject attackEffect;
    public Transform attackPoint;

    // Attack state variables
    private int currentCombo = 0;
    private float lastAttackTime = 0f;
    private bool canAttack = true;
    private Coroutine comboResetCoroutine;

    // Cooldown timers
    private float lightCooldownLeft = 0f;
    private float burstCooldownLeft = 0f;

    // References
    private Animator animator;
    private Camera cam;

    void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;

        // Create attack point if not assigned
        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = new Vector3(0.5f, 0, 0); // Right side of player
        }
    }

    void Update()
    {
        // Update cooldowns
        if (lightCooldownLeft > 0f)
            lightCooldownLeft -= Time.deltaTime;

        if (burstCooldownLeft > 0f)
            burstCooldownLeft -= Time.deltaTime;

        // Handle input
        HandleAttackInput();
    }

    void HandleAttackInput()
    {
        // Light Attack (Left Click) - Combo system
        if (Input.GetMouseButtonDown(0) && canAttack && lightCooldownLeft <= 0f)
        {
            LightAttack();
        }

        // Burst Attack (Right Click)
        if (Input.GetMouseButtonDown(1) && canAttack && burstCooldownLeft <= 0f)
        {
            BurstAttack();
        }
    }

    void LightAttack()
    {
        // Stop any existing combo reset coroutine
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
        }

        // Execute the current combo attack
        PerformAttack(lightAttackDamage[currentCombo], false);

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("LightAttack");
            animator.SetInteger("ComboStep", currentCombo);
        }

        // Visual effect
        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
        }

        Debug.Log($"Light Attack Combo {currentCombo + 1}! Damage: {lightAttackDamage[currentCombo]}");

        // Move to next combo or reset
        currentCombo++;
        if (currentCombo >= lightAttackDamage.Length)
        {
            currentCombo = 0;
            Debug.Log("Combo finished! Resetting to first attack.");
        }

        // Set cooldowns and timers
        lightCooldownLeft = lightAttackCooldown;
        lastAttackTime = Time.time;

        // Start combo reset timer (2 seconds)
        comboResetCoroutine = StartCoroutine(ComboResetTimer());
    }

    void BurstAttack()
    {
        // Execute burst attack
        PerformAttack(burstAttackDamage, true);

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("BurstAttack");
        }

        // Set cooldown
        burstCooldownLeft = burstAttackCooldown;

        // Visual effect
        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
        }

        // Reset combo since burst breaks the flow
        ResetCombo();

        Debug.Log($"Burst Attack! Damage: {burstAttackDamage}");
    }

    void PerformAttack(float damage, bool isBurst)
    {
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Apply damage
            Dummy enemyHealth = enemy.GetComponent<Dummy>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                // Apply knockback for burst attack
                if (isBurst)
                {
                    ApplyKnockback(enemy.transform);
                }
            }
        }
    }

    void ApplyKnockback(Transform enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 direction = (enemy.position - transform.position).normalized;
            enemyRb.AddForce(direction * burstAttackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator ComboResetTimer()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(comboResetTime);

        // If we haven't attacked again in 2 seconds, reset combo
        ResetCombo();
    }

    void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log("Combo reset due to timeout!");
        }

        currentCombo = 0;
        if (animator != null)
        {
            animator.SetInteger("ComboStep", 0);
        }

        comboResetCoroutine = null;
    }

    // Visualize attack range in Scene view
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    // Public methods for UI and other scripts
    public bool IsAttacking()
    {
        return lightCooldownLeft > 0f || burstCooldownLeft > 0f;
    }

    public float GetLightCooldownPercentage()
    {
        return 1f - (lightCooldownLeft / lightAttackCooldown);
    }

    public float GetBurstCooldownPercentage()
    {
        return 1f - (burstCooldownLeft / burstAttackCooldown);
    }

    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    public float GetComboResetTimeLeft()
    {
        if (comboResetCoroutine == null || currentCombo == 0)
            return 0f;

        return comboResetTime - (Time.time - lastAttackTime);
    }

    public float GetComboResetPercentage()
    {
        if (currentCombo == 0)
            return 0f;

        float timeSinceLastAttack = Time.time - lastAttackTime;
        return Mathf.Clamp01(timeSinceLastAttack / comboResetTime);
    }
}