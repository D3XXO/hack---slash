using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Light Attack Combo")]
    public float[] lightAttackDamage = { 10f, 15f, 20f, 25f };
    public float lightAttackCooldown = 0.3f;
    public float comboResetTime = 2f;

    [Header("Burst Attack")]
    public float burstAttackDamage = 40f;
    public float burstAttackCooldown = 2f;
    public float burstAttackForce = 5f;

    [Header("Critical Hit Settings")]
    [Range(0, 100)]
    public float critChance = 15f; // 15% chance to crit
    public float critMultiplier = 2f; // 2x damage on crit
    public Color critTextColor = Color.yellow;
    public GameObject critEffect;

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

    // Stats tracking
    private int totalCrits = 0;
    private int totalAttacks = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;

        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = new Vector3(0.5f, 0, 0);
        }
    }

    void Update()
    {
        if (lightCooldownLeft > 0f)
            lightCooldownLeft -= Time.deltaTime;

        if (burstCooldownLeft > 0f)
            burstCooldownLeft -= Time.deltaTime;

        HandleAttackInput();
    }

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && canAttack && lightCooldownLeft <= 0f)
        {
            LightAttack();
        }

        if (Input.GetMouseButtonDown(1) && canAttack && burstCooldownLeft <= 0f)
        {
            BurstAttack();
        }
    }

    void LightAttack()
    {
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
        }

        // Calculate damage with crit chance
        float baseDamage = lightAttackDamage[currentCombo];
        bool isCrit = RollForCrit();
        float finalDamage = isCrit ? baseDamage * critMultiplier : baseDamage;

        PerformAttack(finalDamage, false, isCrit);

        if (animator != null)
        {
            animator.SetTrigger("LightAttack");
            animator.SetInteger("ComboStep", currentCombo);
        }

        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
        }

        // Show appropriate message
        if (isCrit)
        {
            Debug.Log($"<color=yellow>CRITICAL! Light Attack Combo {currentCombo + 1}! Damage: {finalDamage}</color>");
        }
        else
        {
            Debug.Log($"Light Attack Combo {currentCombo + 1}! Damage: {finalDamage}");
        }

        currentCombo++;
        if (currentCombo >= lightAttackDamage.Length)
        {
            currentCombo = 0;
            Debug.Log("Combo finished! Resetting to first attack.");
        }

        lightCooldownLeft = lightAttackCooldown;
        lastAttackTime = Time.time;
        comboResetCoroutine = StartCoroutine(ComboResetTimer());

        totalAttacks++;
    }

    void BurstAttack()
    {
        // Calculate damage with crit chance
        bool isCrit = RollForCrit();
        float finalDamage = isCrit ? burstAttackDamage * critMultiplier : burstAttackDamage;

        PerformAttack(finalDamage, true, isCrit);

        if (animator != null)
        {
            animator.SetTrigger("BurstAttack");
        }

        burstCooldownLeft = burstAttackCooldown;

        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
        }

        // Show appropriate message
        if (isCrit)
        {
            Debug.Log($"<color=yellow>CRITICAL BURST! Damage: {finalDamage}</color>");
        }
        else
        {
            Debug.Log($"Burst Attack! Damage: {finalDamage}");
        }

        ResetCombo();
        totalAttacks++;
    }

    bool RollForCrit()
    {
        // Generate random number between 0-100 and check if it's below crit chance
        float roll = Random.Range(0f, 100f);
        bool isCritical = roll <= critChance;

        if (isCritical)
        {
            totalCrits++;
        }

        return isCritical;
    }

    void PerformAttack(float damage, bool isBurst, bool isCrit = false)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Dummy enemyHealth = enemy.GetComponent<Dummy>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                // Show crit effect if it was a critical hit
                if (isCrit && critEffect != null)
                {
                    Instantiate(critEffect, enemy.transform.position, Quaternion.identity);
                }

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
        yield return new WaitForSeconds(comboResetTime);
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

    // New methods for crit system
    public float GetCritChance()
    {
        return critChance;
    }

    public float GetCritMultiplier()
    {
        return critMultiplier;
    }

    public float GetCritRate()
    {
        if (totalAttacks == 0) return 0f;
        return (float)totalCrits / totalAttacks * 100f;
    }

    public int GetTotalCrits()
    {
        return totalCrits;
    }

    public int GetTotalAttacks()
    {
        return totalAttacks;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}