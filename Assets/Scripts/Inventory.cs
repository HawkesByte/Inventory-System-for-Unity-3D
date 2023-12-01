using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public List<Item> heldItems = new List<Item>();
    private int inventorySpace = 0;
    public float pickUpDistance = 5f;
    public LayerMask itemLayer;

    [Header("UI")]
    public GameObject inventoryUI;
    public List<Image> inventorySlots = new List<Image>();

    public int currentHoveredSlot = -1;

    // Drag and Drop
    private int previousSlotIndex = -1;
    public Image dragDropUIImage;

    private void Start()
    {
        toggleMouseLock();
        inventorySpace = inventorySlots.Count;
    }

    private void Update()
    {
        dragDropUIImage.transform.position = Input.mousePosition;

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
                    updateInventory();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
            displayInventory();

        if (Input.GetKeyDown(KeyCode.Q) && currentHoveredSlot != -1)
        {
            if (inventorySlots[currentHoveredSlot].sprite != null)
            {
                dropItem();
            }
        }

        if (Input.GetMouseButtonDown(0) && currentHoveredSlot != -1)
        {
            if (inventorySlots[currentHoveredSlot].sprite != null)
            {
                dragUIItem();
                previousSlotIndex = currentHoveredSlot;
            }
        }
        else if (Input.GetMouseButtonUp(0) && previousSlotIndex != -1)
        {
            dropUIItem();
        }
    }

    private void addItem(Item newItem)
    {
        int leftOverQuantity = newItem.currentQuantity;
        bool itemProcessed = false;

        // Loop through all slots
        for (int i = 0; i < heldItems.Count; i++)
        {
            Item invSlot = heldItems[i];

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

        if (heldItems.Count < inventorySpace)
        {
            heldItems.Add(null); // Create a new space
        }

        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i] == null)
            {
                // Add to the empty slot
                heldItems[i] = newItem;
                heldItems[i].currentQuantity = Mathf.Min(newItem.currentQuantity, newItem.maxQuantity);

                Debug.Log("Added " + heldItems[i].currentQuantity + " " + newItem.itemName + "(s) to a new slot.");
                newItem.gameObject.SetActive(false); // Object is taking over the new slot. Must keep.

                for (int a = 0; a < inventorySlots.Count; a++)
                {
                    if (inventorySlots[a].sprite == null)
                    {
                        newItem.uiSlotIndex = a;
                        break;
                    }
                }

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

    private void dropItem()
    {
        Item itemToDrop = heldItems.Find(item => item != null && item.uiSlotIndex == currentHoveredSlot);
        itemToDrop.gameObject.SetActive(true);
        itemToDrop.transform.position = new Vector3(transform.position.x + 2f, transform.position.y + 2f, transform.position.z);

        heldItems.Remove(itemToDrop);
        inventorySlots[currentHoveredSlot].sprite = null;
        inventorySlots[currentHoveredSlot].color = new Color(1,1,1,0);
    }

    private void displayInventory()
    {
        inventoryUI.SetActive(!inventoryUI.activeInHierarchy);
        toggleMouseLock();
    }

    private void updateInventory()
    {
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems != null)
            {
                Item invSlot = heldItems[i];
                inventorySlots[invSlot.uiSlotIndex].sprite = invSlot.icon;
                inventorySlots[invSlot.uiSlotIndex].color = new Color(1, 1, 1, 1);
            }
        }
    }

    private void dragUIItem()
    {
        dragDropUIImage.enabled = true;
        dragDropUIImage.sprite = inventorySlots[currentHoveredSlot].sprite;
        inventorySlots[currentHoveredSlot].sprite = null;
        inventorySlots[currentHoveredSlot].color = new Color(1, 1, 1, 0);
    }

    private void dropUIItem()
    {
        if (currentHoveredSlot != -1)
        {
            if (inventorySlots[currentHoveredSlot].sprite != null) // Swap items
            {
                Item curItem = getCurrentItemFromInventoryIndex(previousSlotIndex);
                Item swapItem = getCurrentItemFromInventoryIndex(currentHoveredSlot);

                inventorySlots[previousSlotIndex].sprite = inventorySlots[currentHoveredSlot].sprite;
                inventorySlots[previousSlotIndex].color = new Color(1, 1, 1, 1);

                inventorySlots[currentHoveredSlot].sprite = dragDropUIImage.sprite;
                inventorySlots[currentHoveredSlot].color = new Color(1, 1, 1, 1);

                curItem.uiSlotIndex = currentHoveredSlot;
                swapItem.uiSlotIndex = previousSlotIndex;

                dragDropUIImage.sprite = null;
                dragDropUIImage.enabled = false;
            }
            else // Drop Item
            {
                inventorySlots[currentHoveredSlot].sprite = dragDropUIImage.sprite;
                inventorySlots[currentHoveredSlot].color = new Color(1, 1, 1, 1);
                dragDropUIImage.sprite = null;
                dragDropUIImage.enabled = false;

                Item curItem = getCurrentItemFromInventoryIndex(previousSlotIndex);
                curItem.uiSlotIndex = currentHoveredSlot;
            }
        }
        else // Return Item
        {
            inventorySlots[previousSlotIndex].sprite = dragDropUIImage.sprite;
            inventorySlots[previousSlotIndex].color = new Color(1, 1, 1, 1);
            dragDropUIImage.sprite = null;
            dragDropUIImage.enabled = false;
        }

        previousSlotIndex = -1;
        updateInventory();
    }

    private Item getCurrentItemFromInventoryIndex(int curInvIndex)
    {
        foreach (Item i in heldItems)
        {
            if (i.uiSlotIndex == curInvIndex)
            {
                return i;
            }
        }

        return null;
    }

    private void toggleMouseLock()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            transform.GetChild(0).GetComponent<FirstPersonLook>().sensitivity = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            transform.GetChild(0).GetComponent<FirstPersonLook>().sensitivity = 2;
        }
    }
}
