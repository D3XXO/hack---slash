using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Weapon[] weapons = new Weapon[3];
    private int currentWeaponIndex = 0;

    [Header("Attack Settings")]
    [SerializeField] float attackRange;
    [SerializeField] LayerMask enemyLayer;

    [Header("Critical Hit Settings")]
    [Range(0, 100)]
    [SerializeField] float critChance;
    [SerializeField] float critMultiplier;
    [SerializeField] GameObject critEffect;

    [Header("Visual Effects")]
    [SerializeField] GameObject attackEffect;
    [SerializeField] Transform attackPoint;

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

    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public float[] lightAttackDamage;
        public float burstAttackDamage;
        public float lightAttackKnockbackForce;
        public float burstAttackForce;
        public float lightAttackCooldown;
        public float burstAttackCooldown;
        public float comboResetTime;
        public GameObject weaponModel;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;

        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = new Vector3(0, 0, 0);
        }

        // Activate first weapon
        SwitchWeapon(0);
    }

    void Update()
    {
        if (lightCooldownLeft > 0f)
            lightCooldownLeft -= Time.deltaTime;

        if (burstCooldownLeft > 0f)
            burstCooldownLeft -= Time.deltaTime;

        HandleWeaponSwitch();
        HandleAttackInput();
    }

    void HandleWeaponSwitch()
    {
        // Switch weapons with 1, 2, 3 keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchWeapon(2);
        }
    }

    void SwitchWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Length) return;

        // Deactivate current weapon model
        if (weapons[currentWeaponIndex].weaponModel != null)
            weapons[currentWeaponIndex].weaponModel.SetActive(false);

        currentWeaponIndex = weaponIndex;

        // Activate new weapon model
        if (weapons[currentWeaponIndex].weaponModel != null)
            weapons[currentWeaponIndex].weaponModel.SetActive(true);

        // Reset combo when switching weapons
        ResetCombo();

        Debug.Log($"Switched to: {weapons[currentWeaponIndex].weaponName}");
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

        Weapon currentWeapon = weapons[currentWeaponIndex];

        // Calculate damage with crit chance
        float baseDamage = currentWeapon.lightAttackDamage[currentCombo];
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
            Debug.Log($"<color=yellow>CRITICAL! {currentWeapon.weaponName} Light Attack Combo {currentCombo + 1}! Damage: {finalDamage}</color>");
        }
        else
        {
            Debug.Log($"{currentWeapon.weaponName} Light Attack Combo {currentCombo + 1}! Damage: {finalDamage}");
        }

        currentCombo++;
        if (currentCombo >= currentWeapon.lightAttackDamage.Length)
        {
            currentCombo = 0;
            Debug.Log($"{currentWeapon.weaponName} combo finished! Resetting to first attack.");
        }

        lightCooldownLeft = currentWeapon.lightAttackCooldown;
        lastAttackTime = Time.time;
        comboResetCoroutine = StartCoroutine(ComboResetTimer());

        totalAttacks++;
    }

    void BurstAttack()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // Calculate damage with crit chance
        bool isCrit = RollForCrit();
        float finalDamage = isCrit ? currentWeapon.burstAttackDamage * critMultiplier : currentWeapon.burstAttackDamage;

        PerformAttack(finalDamage, true, isCrit);

        if (animator != null)
        {
            animator.SetTrigger("BurstAttack");
        }

        burstCooldownLeft = currentWeapon.burstAttackCooldown;

        if (attackEffect != null)
        {
            Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
        }

        // Show appropriate message
        if (isCrit)
        {
            Debug.Log($"<color=yellow>CRITICAL BURST! {currentWeapon.weaponName} Damage: {finalDamage}</color>");
        }
        else
        {
            Debug.Log($"{currentWeapon.weaponName} Burst Attack! Damage: {finalDamage}");
        }

        ResetCombo();
        totalAttacks++;
    }

    bool RollForCrit()
    {
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

        Weapon currentWeapon = weapons[currentWeaponIndex];
        float knockbackForce = isBurst ? currentWeapon.burstAttackForce : currentWeapon.lightAttackKnockbackForce;

        foreach (Collider2D enemy in hitEnemies)
        {
            Dummy enemyScript = enemy.GetComponent<Dummy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage, isCrit);

                if (isCrit && critEffect != null)
                {
                    Instantiate(critEffect, enemy.transform.position, Quaternion.identity);
                }

                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                enemyScript.ApplyKnockback(knockbackDirection, knockbackForce);
            }
        }
    }

    IEnumerator ComboResetTimer()
    {
        float resetTime = weapons[currentWeaponIndex].comboResetTime;
        yield return new WaitForSeconds(resetTime);
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
        Weapon currentWeapon = weapons[currentWeaponIndex];
        return 1f - (lightCooldownLeft / currentWeapon.lightAttackCooldown);
    }

    public float GetBurstCooldownPercentage()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];
        return 1f - (burstCooldownLeft / currentWeapon.burstAttackCooldown);
    }

    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    public int GetMaxCombo()
    {
        return weapons[currentWeaponIndex].lightAttackDamage.Length;
    }

    public float GetComboResetTimeLeft()
    {
        if (comboResetCoroutine == null || currentCombo == 0)
            return 0f;

        float resetTime = weapons[currentWeaponIndex].comboResetTime;
        return resetTime - (Time.time - lastAttackTime);
    }

    public float GetComboResetPercentage()
    {
        if (currentCombo == 0)
            return 0f;

        float resetTime = weapons[currentWeaponIndex].comboResetTime;
        float timeSinceLastAttack = Time.time - lastAttackTime;
        return Mathf.Clamp01(timeSinceLastAttack / resetTime);
    }

    public string GetCurrentWeaponName()
    {
        return weapons[currentWeaponIndex].weaponName;
    }

    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }

    public Weapon GetCurrentWeapon()
    {
        return weapons[currentWeaponIndex];
    }

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