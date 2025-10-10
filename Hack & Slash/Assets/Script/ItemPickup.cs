using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;
    public int quantity = 1;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
        
        if (itemData != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }
}