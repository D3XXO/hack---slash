using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text damageText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeOutTime;

    public Vector3 worldPositionToFollow;

    private float fadeOutTimer;

    void Start()
    {
        if (damageText == null)
        {
            damageText = GetComponentInChildren<Text>();
        }
        fadeOutTimer = fadeOutTime;
    }

    void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(worldPositionToFollow);

        fadeOutTimer -= Time.deltaTime;
        if (fadeOutTimer <= 0)
        {
            Color newColor = damageText.color;
            newColor.a -= (Time.deltaTime / fadeOutTime) * 2f;
            damageText.color = newColor;

            if (newColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetDamage(float damage, bool isCrit)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();

        if (isCrit)
        {
            damageText.color = Color.yellow;
            damageText.fontSize += 5;
        }
    }
}