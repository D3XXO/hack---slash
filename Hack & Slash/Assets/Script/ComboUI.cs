using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] PlayerAttack playerAttack;

    [Header("UI Elements")]
    [SerializeField] Text comboText;
    [SerializeField] Slider comboResetSlider;
    [SerializeField] Text critChanceText;

    void Start()
    {
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();
    }

    void Update()
    {
        if (playerAttack == null || comboText == null) return;

        int currentCombo = playerAttack.GetCurrentCombo();
        comboText.text = $"Combo: {currentCombo + 1}/4";

        // Update crit chance display if available
        if (critChanceText != null)
        {
            critChanceText.text = $"Crit: {playerAttack.GetCritChance()}%";
        }

        if (comboResetSlider != null)
        {
            if (currentCombo > 0)
            {
                comboResetSlider.gameObject.SetActive(true);
                comboResetSlider.value = playerAttack.GetComboResetPercentage();
            }
            else
            {
                comboResetSlider.gameObject.SetActive(false);
            }
        }
    }
}