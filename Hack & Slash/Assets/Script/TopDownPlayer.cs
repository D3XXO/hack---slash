// TopDownPlayer.cs
using UnityEngine;

public class TopDownPlayer : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth;
    private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;

    [Header("Input Settings")]
    [SerializeField] KeyCode dashKey = KeyCode.LeftShift;

    private PlayerHealthUI healthUI;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 dashDirection;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float dashCooldownLeft = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Inisialisasi health player
        currentHealth = maxHealth;

        // Cari skrip UI Health secara otomatis
        healthUI = FindObjectOfType<PlayerHealthUI>();
        if (healthUI != null)
        {
            healthUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
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
        if (isDashing)
        {
            rb.velocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    // --- FUNGSI BARU UNTUK HEALTH ---
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Pastikan health tidak kurang dari 0

        Debug.Log($"Player took {damage} damage. Current Health: {currentHealth}");

        // Update UI Health Bar
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
        // Tambahkan logika kematian di sini (misal: game over, restart scene)
        gameObject.SetActive(false); // Contoh sederhana: nonaktifkan player
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
    // ---------------------------------

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