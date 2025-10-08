using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text healthText;

    void Start()
    {
        if (healthText == null)
        {
            healthText = GetComponentInChildren<Text>();
        }
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
        }
    }
}