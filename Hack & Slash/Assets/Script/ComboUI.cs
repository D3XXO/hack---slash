using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerAttack playerAttack;
    public TopDownPlayer playerHealth;

    [Header("Weapon UI Elements")]
    public Text weaponText;

    [Header("Combo UI Elements")]
    public Text comboText;
    public Slider comboResetSlider;
    public Image comboFillImage;

    [Header("Crit UI Elements")]
    public Text critChanceText;
    public Text critStatsText;

    [Header("Health UI Elements")]
    public Text healthText;
    public Slider healthBar;
    public Image healthFillImage;

    [Header("Colors")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public Color critColor = Color.yellow;
    public Color critStatsColor = Color.yellow;
    public Color comboActiveColor = new Color(1f, 0.5f, 0f); // Orange

    void Start()
    {
        // Auto-find references if not assigned
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();

        if (playerHealth == null)
            playerHealth = FindObjectOfType<TopDownPlayer>();

        // Get fill images for color changes
        if (comboResetSlider != null && comboFillImage == null)
            comboFillImage = comboResetSlider.fillRect.GetComponent<Image>();

        if (healthBar != null && healthFillImage == null)
            healthFillImage = healthBar.fillRect.GetComponent<Image>();
    }

    void Update()
    {
        if (playerAttack == null || playerHealth == null)
        {
            ShowErrorMessages();
            return;
        }

        UpdateWeaponDisplay();
        UpdateComboDisplay();
        UpdateCritDisplay();
        UpdateCritStatsDisplay();
        UpdateHealthDisplay();
    }

    void UpdateWeaponDisplay()
    {
        if (weaponText != null)
        {
            string weaponName = playerAttack.GetCurrentWeaponName();
            int weaponIndex = playerAttack.GetCurrentWeaponIndex() + 1;
            weaponText.text = $"Weapon [{weaponIndex}]: {weaponName}";
        }
    }

    void UpdateComboDisplay()
    {
        if (comboText != null)
        {
            int currentCombo = playerAttack.GetCurrentCombo();
            int maxCombo = playerAttack.GetMaxCombo();
            comboText.text = $"Combo: {currentCombo + 1}/{maxCombo}";
            comboText.color = currentCombo > 0 ? comboActiveColor : Color.white;
        }

        if (comboResetSlider != null)
        {
            int currentCombo = playerAttack.GetCurrentCombo();
            if (currentCombo > 0)
            {
                comboResetSlider.gameObject.SetActive(true);
                comboResetSlider.value = playerAttack.GetComboResetPercentage();

                if (comboFillImage != null)
                {
                    float timeLeft = playerAttack.GetComboResetTimeLeft();
                    comboFillImage.color = timeLeft > 1f ? Color.green :
                                         timeLeft > 0.5f ? Color.yellow : Color.red;
                }
            }
            else
            {
                comboResetSlider.gameObject.SetActive(false);
            }
        }
    }

    void UpdateCritDisplay()
    {
        if (critChanceText != null)
        {
            float critChance = playerAttack.GetCritChance();
            float critMultiplier = playerAttack.GetCritMultiplier();
            critChanceText.text = $"Crit: {critChance}%";
            critChanceText.color = critColor;
        }
    }

    void UpdateCritStatsDisplay()
    {
        if (critStatsText != null)
        {
            float critChance = playerAttack.GetCritChance();
            float critMultiplier = playerAttack.GetCritMultiplier();
            critStatsText.text = $"Crit: (x{critMultiplier})";
            critStatsText.color = critStatsColor;
        }
    }

    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            float currentHealth = playerHealth.GetCurrentHealth();
            float maxHealth = playerHealth.GetMaxHealth();
            healthText.text = $"Health: {currentHealth:F0}/{maxHealth:F0}";
        }

        if (healthBar != null)
        {
            float currentHealth = playerHealth.GetCurrentHealth();
            float maxHealth = playerHealth.GetMaxHealth();
            float healthPercent = currentHealth / maxHealth;

            healthBar.value = healthPercent;

            // Change health bar color based on percentage
            if (healthFillImage != null)
            {
                if (healthPercent > 0.6f)
                    healthFillImage.color = healthyColor;
                else if (healthPercent > 0.3f)
                    healthFillImage.color = warningColor;
                else
                    healthFillImage.color = dangerColor;
            }
        }
    }

    void ShowErrorMessages()
    {
        if (weaponText != null) weaponText.text = "Weapon: ---";
        if (comboText != null) comboText.text = "Combo: ---";
        if (critChanceText != null) critChanceText.text = "Crit: ---";
        if (healthText != null) healthText.text = "Health: ---";
        if (healthBar != null) healthBar.value = 0f;
    }
}