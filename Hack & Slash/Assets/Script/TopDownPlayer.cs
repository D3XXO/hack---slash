using UnityEngine;
using System.Collections;

public class TopDownPlayer : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] float maxHealth;
    private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;
    
    [Header("Knockback Settings")]
    [SerializeField] float knockbackDuration;

    [Header("Input Settings")]
    [SerializeField] KeyCode dashKey = KeyCode.LeftShift;

    public CameraController mainCameraController;
    private PlayerHealthUI healthUI;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 dashDirection;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;

    private bool isBeingKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        healthUI = FindObjectOfType<PlayerHealthUI>();
        if (healthUI != null)
        {
            healthUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isBeingKnockedBack) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontalInput) > Mathf.Abs(verticalInput))
        {
            movement = new Vector2(horizontalInput, 0f);
        }
        else
        {
            movement = new Vector2(0f, verticalInput);
        }

        movement = movement.normalized;

        if (Input.GetKeyDown(dashKey) && dashCooldownLeft <= 0f && !isDashing && movement != Vector2.zero)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                EndDash();
            }
        }

        if (dashCooldownLeft > 0f)
        {
            dashCooldownLeft -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (isBeingKnockedBack) return;

        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed * Time.deltaTime;
        }
        else
        {
            rb.velocity = movement * moveSpeed * Time.deltaTime;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isDashing) EndDash();
        
        StopAllCoroutines();
        StartCoroutine(KnockbackCoroutine(direction, force));
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isBeingKnockedBack = true;
        
        rb.velocity = direction * force;

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
        isBeingKnockedBack = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        Debug.Log($"Player took {damage} damage. Current Health: {currentHealth}");

        if (healthUI != null)
        {
            healthUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        gameObject.SetActive(false); 
    }
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;
        dashDirection = movement;
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetDashCooldownPercentage()
    {
        return 1f - (dashCooldownLeft / dashCooldown);
    }
}