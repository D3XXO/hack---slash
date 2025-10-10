using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsGrid;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        if (playerAttack == null)
        {
            playerAttack = FindObjectOfType<PlayerAttack>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            bool isOpening = !inventoryPanel.activeSelf;
            
            inventoryPanel.SetActive(isOpening);

            if (isOpening)
            {
                Time.timeScale = 0f;
                playerAttack.enabled = false;
                DrawInventory();
            }
            else
            {
                Time.timeScale = 1f;
                playerAttack.enabled = true;
            }
        }
    }

    public void DrawInventory()
    {
        for (int i = 0; i < slotsGrid.childCount; i++)
        {
            Transform slotUI = slotsGrid.GetChild(i);
            Image itemIcon = slotUI.Find("ItemIcon").GetComponent<Image>();
            Text quantityText = slotUI.Find("QuantityText").GetComponent<Text>();

            InventoryManager.InventorySlot itemSlot = inventoryManager.inventorySlots[i];

            if (itemSlot != null)
            {
                itemIcon.sprite = itemSlot.itemData.icon;
                itemIcon.enabled = true;
                quantityText.text = itemSlot.quantity > 1 ? itemSlot.quantity.ToString() : "";
            }
            else
            {
                itemIcon.enabled = false;
                quantityText.text = "";
            }
        }
    }
}