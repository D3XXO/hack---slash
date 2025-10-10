using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(InventoryManager))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode pickupKey;
    
    private InventoryManager inventoryManager;
    private List<ItemPickup> nearbyItems = new List<ItemPickup>();

    private void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey) && nearbyItems.Count > 0)
        {
            PickupClosestItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ItemPickup item = other.GetComponent<ItemPickup>();
        if (item != null && !nearbyItems.Contains(item))
        {
            nearbyItems.Add(item);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ItemPickup item = other.GetComponent<ItemPickup>();
        if (item != null && nearbyItems.Contains(item))
        {
            nearbyItems.Remove(item);
        }
    }

    private void PickupClosestItem()
    {
        nearbyItems.RemoveAll(item => item == null);

        if (nearbyItems.Count == 0) return;

        ItemPickup closestItem = nearbyItems
            .OrderBy(item => Vector2.Distance(transform.position, item.transform.position))
            .FirstOrDefault();

        if (closestItem != null)
        {
            if (inventoryManager.AddItem(closestItem.itemData, closestItem.quantity))
            {
                nearbyItems.Remove(closestItem);
                Destroy(closestItem.gameObject);
            }
        }
    }
}