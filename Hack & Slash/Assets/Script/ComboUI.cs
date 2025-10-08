using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerAttack playerAttack;
    
    [Header("UI Elements")]
    public Slider comboResetSlider;
    public Text comboText;
    public Image comboFillImage;
    
    [Header("Colors")]
    public Color comboActiveColor = Color.yellow;
    public Color comboInactiveColor = Color.gray;
    
    void Start()
    {
        // If not assigned in inspector, try to find them
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();
            
        if (comboResetSlider == null)
            comboResetSlider = GetComponentInChildren<Slider>();
            
        if (comboText == null)
            comboText = GetComponentInChildren<Text>();
            
        if (comboFillImage == null && comboResetSlider != null)
            comboFillImage = comboResetSlider.fillRect.GetComponent<Image>();
    }
    
    void Update()
    {
        if (playerAttack == null) return;
        
        int currentCombo = playerAttack.GetCurrentCombo();
        
        // Update combo text
        if (comboText != null)
        {
            comboText.text = $"Combo: {currentCombo + 1}/3";
        }
        
        // Update progress bar
        if (comboResetSlider != null)
        {
            if (currentCombo > 0)
            {
                // Show slider when combo is active
                comboResetSlider.gameObject.SetActive(true);
                comboResetSlider.value = playerAttack.GetComboResetPercentage();
                
                // Change color based on how close we are to reset
                if (comboFillImage != null)
                {
                    float timeLeft = playerAttack.GetComboResetTimeLeft();
                    comboFillImage.color = timeLeft > 1f ? comboActiveColor : Color.red;
                }
            }
            else
            {
                // Hide slider when no combo
                comboResetSlider.gameObject.SetActive(false);
            }
        }
    }
}