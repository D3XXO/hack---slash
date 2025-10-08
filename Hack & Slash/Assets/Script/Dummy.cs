using UnityEngine;

public class Dummy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Visual Feedback")]
    public bool showDamageNumbers = true;
    public Color damageColor = Color.red;
    public float colorFlashDuration = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        // Get sprite renderer for visual feedback
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        Debug.Log($"{gameObject.name} spawned with {currentHealth} health");
    }

    public void TakeDamage(float damage)
    {
        // Reduce health
        currentHealth -= damage;

        // Show damage feedback
        if (showDamageNumbers)
        {
            Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}");
        }

        // Visual feedback
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashColor());
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        // Increase health, but don't exceed max health
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        Debug.Log($"{gameObject.name} healed for {healAmount}! Health: {currentHealth}");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log($"{gameObject.name} health reset to {currentHealth}");
    }

    private System.Collections.IEnumerator FlashColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(colorFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");

        // You can replace this with your own death logic:
        // - Play death animation
        // - Spawn particles
        // - Drop loot
        // - etc.

        Destroy(gameObject);
    }

    // Public methods to check health status
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }
}