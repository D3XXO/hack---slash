using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public event Action OnInventoryChanged;

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData itemData;
        public int quantity;

        public InventorySlot(ItemData data, int amount)
        {
            itemData = data;
            quantity = amount;
        }

        public void AddToStack(int amount)
        {
            quantity += amount;
        }

        public void RemoveFromStack(int amount)
        {
            quantity -= amount;
        }
    }

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize;
    public List<InventorySlot> inventorySlots;

    private void Awake()
    {
        inventorySlots = new List<InventorySlot>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(null);
        }
    }

    public bool AddItem(ItemData item, int quantity)
    {
        int quantityLeft = quantity;

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].itemData == item)
            {
                int spaceAvailable = item.maxStackSize - inventorySlots[i].quantity;
                if (spaceAvailable > 0)
                {
                    int amountToAdd = Mathf.Min(quantityLeft, spaceAvailable);
                    inventorySlots[i].AddToStack(amountToAdd);
                    quantityLeft -= amountToAdd;
                }
            }

            if (quantityLeft <= 0)
            {
                Debug.Log($"Berhasil menambahkan {quantity} {item.itemName}.");
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        while (quantityLeft > 0)
        {
            int emptySlotIndex = -1;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i] == null)
                {
                    emptySlotIndex = i;
                    break;
                }
            }
            
            if (emptySlotIndex != -1)
            {
                int amountToAdd = Mathf.Min(quantityLeft, item.maxStackSize);
                inventorySlots[emptySlotIndex] = new InventorySlot(item, amountToAdd);
                quantityLeft -= amountToAdd;
            }
            else
            {
                Debug.LogWarning("Inventaris penuh! Sebagian item mungkin tidak tertampung.");
                OnInventoryChanged?.Invoke();
                return false;
            }
        }

        Debug.Log($"Berhasil menambahkan {quantity} {item.itemName}.");
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void RemoveItem(ItemData item, int quantity)
    {
        int quantityLeftToRemove = quantity;

        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i] != null && inventorySlots[i].itemData == item)
            {
                int amountToRemove = Mathf.Min(quantityLeftToRemove, inventorySlots[i].quantity);
                inventorySlots[i].RemoveFromStack(amountToRemove);
                quantityLeftToRemove -= amountToRemove;
                
                if (inventorySlots[i].quantity <= 0)
                {
                    inventorySlots[i] = null;
                }
            }
            
            if (quantityLeftToRemove <= 0)
            {
                break;
            }
        }
        
        OnInventoryChanged?.Invoke();
    }
}