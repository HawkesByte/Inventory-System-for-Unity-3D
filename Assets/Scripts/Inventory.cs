using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public List<Item> inventorySlots = new List<Item>();
    public int inventorySpace = 10;
    public float pickUpDistance = 5f;
    public LayerMask itemLayer;

    [Header("UI")]
    public GameObject inventoryUI;

    private void Start()
    {
        toggleMouseLock();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pickUpDistance, itemLayer))
            {
                Item item = hit.collider.GetComponent<Item>();

                if (item)
                {
                    addItem(item);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            displayInventory();
        }
    }

    private void addItem(Item newItem)
    {
        int leftOverQuantity = newItem.currentQuantity;
        bool itemProcessed = false;

        // Loop through all slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Item invSlot = inventorySlots[i];

            // Check if the slot holds an item with the same name as newItem
            if (invSlot != null && newItem.itemName == invSlot.itemName)
            {
                // Calculate how much can be added to the current slot
                int availableSpace = invSlot.maxQuantity - invSlot.currentQuantity;
                int quantityToAdd = Mathf.Min(leftOverQuantity, availableSpace);

                // Add to the current slot
                invSlot.currentQuantity += quantityToAdd;
                leftOverQuantity -= quantityToAdd;

                // If leftOverQuantity is zero, the item is completely added
                if (leftOverQuantity == 0)
                {
                    Debug.Log("Added " + newItem.currentQuantity + " " + newItem.itemName + "(s) to existing slot(s).");
                    Destroy(newItem.gameObject); // Can destroy this gameobject as matching object now holds its quantity.
                    itemProcessed = true;
                    break;
                }
            }
        }

        // If the item has been processed, exit the method
        if (itemProcessed) return;

        // If there is left over quantity, try to add it to a new slot

        if (inventorySlots.Count < inventorySpace)
        {
            inventorySlots.Add(null); // Create a new space
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null)
            {
                // Add to the empty slot
                inventorySlots[i] = newItem;
                inventorySlots[i].currentQuantity = Mathf.Min(newItem.currentQuantity, newItem.maxQuantity);

                Debug.Log("Added " + inventorySlots[i].currentQuantity + " " + newItem.itemName + "(s) to a new slot.");
                newItem.gameObject.SetActive(false); // Object is taking over the new slot. Must keep.
                itemProcessed = true;
                break;
            }
        }


        // If the item has been processed, exit the method
        if (itemProcessed) return;

        // If no empty slots left
        newItem.currentQuantity = leftOverQuantity;
        Debug.Log("Couldn't fit item into inventory");
    }

    private void displayInventory()
    {
        foreach (Item invSlot in inventorySlots)
        {
            if (invSlot != null)
            {
                Debug.Log(invSlot.itemName + " | Quantity: " + invSlot.currentQuantity);
            }
        }
    }


    private void toggleMouseLock()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
