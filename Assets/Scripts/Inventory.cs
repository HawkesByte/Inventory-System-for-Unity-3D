using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Item Lists")]
    public List<UISlot> inventorySlots = new List<UISlot>();
    public List<UISlot> hotbarSlots = new List<UISlot>();
    private List<UISlot> allInventorySlots = new List<UISlot>();

    [Header("UI")]
    public GameObject inventory;
    public Image crosshair;
    public Text itemHoverText;

    [Header("Raycast")]
    public float raycastDistance;
    public LayerMask itemLayer;
    public Transform itemDropLocation;

    [Header("Drag and Drop")]
    public Image dragIconImage;
    private Item currentDraggedItem;
    private int currentDragSlotIndex = -1; // The index of the slot the item we are trying to drag was in before lifted.

    [Header("Equippable Items")]
    public List<GameObject> equippableItems = new List<GameObject>();


    public void Start()
    {
        toggleInventory(false);

        allInventorySlots.AddRange(hotbarSlots); // Fill the all slots list with our inventory slots and hotbar slots.
        allInventorySlots.AddRange(inventorySlots);

        foreach (UISlot uiSlot in allInventorySlots)
        {
            uiSlot.initialiseSlot();
        }
    }

    public void Update()
    {
        if (!inventory.activeInHierarchy)
            itemRaycast(Input.GetMouseButtonDown(0)); // Pick up item (or if we dont click, raycast to check if we are looking at an item).

        if (inventory.activeInHierarchy && Input.GetMouseButtonDown(0)) // If we click on a inventory slot while the inventory is active.
        {
            dragInventoryIcon();
        }
        else if (currentDragSlotIndex != -1 && Input.GetMouseButtonUp(0) || currentDragSlotIndex != -1 && !inventory.activeInHierarchy) // If we lift the mouse click we try to drop the held item.
        {
            dropInventoryIcon();
        }

        if (Input.GetKeyDown(KeyCode.Q)) // Drop an item out of the inventory.
            dropItem();

        if (Input.GetKeyDown(KeyCode.E)) // Activate the inventory.
            toggleInventory(!inventory.activeInHierarchy);

        for (int i = 1; i < hotbarSlots.Count + 1; i++)
        {
            if (Input.GetKeyDown(i.ToString())) // If we get any of the number keys on the keyboard down (trying to access an item in the hotbar).
            {
                enableHotbarItem(i - 1);
            }
        }

        dragIconImage.transform.position = Input.mousePosition; // Update drag icon image.
    }

    private void itemRaycast(bool hasClicked = false)
    {
        itemHoverText.text = "";
        Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position); // Raycast out from the camera to the crosshair (Centre of the screen).
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, itemLayer))
        {
            if (hit.collider != null) // We can assume that this will only return true if we are looking at an item becuase of the "itemLayer" variable
            {
                if (hasClicked)
                {
                    // Pick Up Item
                    Item newItem = hit.collider.GetComponent<Item>();
                    if (newItem)
                    {
                        addItemToInventory(newItem);
                    }
                }
                else
                {
                    Item newItem = hit.collider.GetComponent<Item>();

                    if (newItem)
                    {
                        itemHoverText.text = newItem.name;
                    }
                }
            }
        }
    }

    private void addItemToInventory(Item itemToAdd) // Three stage plan - 1. Stack as much as possible. 2. Add to new slot. 3. No space, give up.
    {
        int leftoverQuantity = itemToAdd.currentQuantity;
        UISlot openSlot = null;
        for (int i = 0; i < allInventorySlots.Count; i++) // Loop through every inventory slot
        {
            Item heldItem = allInventorySlots[i].getItem();
            if (heldItem != null && itemToAdd.name == heldItem.name) // Meaning both these items are the same type.
            {
                int freeSpaceInSlot = heldItem.maxQuantity - heldItem.currentQuantity; // Calculate how much space this slot has until its full.

                if (freeSpaceInSlot >= leftoverQuantity) // We can add the entire quantity of the item. 
                {
                    heldItem.currentQuantity += leftoverQuantity;
                    Destroy(itemToAdd.gameObject); // We can destroy this gameobject as its quantity is now stored within another item.
                    updateAllInventorySlots();
                    return;
                }
                else // Add as much as possible to the current slot.
                {
                    heldItem.currentQuantity = heldItem.maxQuantity;
                    leftoverQuantity -= freeSpaceInSlot; // Decrement quantityToAdd by the amount we where able to put into an existing item.
                }
            }
            else if (heldItem == null)
            {
                if (!openSlot) // If open slot has not be assigned a slot yet.
                    openSlot = allInventorySlots[i]; // Take note that we have an open slot.
            }
        }

        if (leftoverQuantity > 0 && openSlot) // The new items quantity is still bigger than 0 meaning the item still exists.
        {
            openSlot.setItem(itemToAdd);
            itemToAdd.currentQuantity = leftoverQuantity;
            itemToAdd.gameObject.SetActive(false); // As opposed to destroying the item it now takes up its own slot, so to drop it later on we
                                                   // should keep its gameobject.
        }
        else // In the situation where we have leftover quantity but no open slots...
        {
            itemToAdd.currentQuantity = leftoverQuantity; // We tried to pick up the item but it didnt fit, we may have managed to get some of
                                                          // its quantity into the inventory though so update its current quantity.
        }

        updateAllInventorySlots();
    }

    private void updateAllInventorySlots()
    {
        foreach (UISlot a in allInventorySlots) // Update all the slots in the inventory to display the correct quantity.
        {
            a.updateData();
        }
    }

    private void toggleInventory(bool Enable)
    {
        inventory.SetActive(Enable); // Toggles the inventory.

        if (inventory.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Disable player camera movement (different depending on FPSController)
            Camera.main.GetComponent<FirstPersonLook>().sensitivity = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Disable player camera movement (different depending on FPSController)
            Camera.main.GetComponent<FirstPersonLook>().sensitivity = 2;

            foreach (UISlot a in allInventorySlots) // If we close inventory without leaving a slot then we leave it hovered (need to find a nicer fix).
            {
                a.hovered = false;
            }
        }
    }

    private void dropItem() // Drop an item into the world space.
    {
        for (int i = 0; i < allInventorySlots.Count; i++) // Loop through every inventory slot.
        {
            UISlot curSlot = allInventorySlots[i];
            if (curSlot.hovered && curSlot.hasItem())
            {
                curSlot.getItem().gameObject.SetActive(true);
                curSlot.getItem().transform.position = itemDropLocation.position;
                curSlot.setItem(null);
                break;
            }
        }
    }

    private void dragInventoryIcon() // Gives us the ability to move objects around in the inventory.
    {
        for (int i = 0; i < allInventorySlots.Count; i++) // Loop through every inventory slot.
        {
            UISlot curSlot = allInventorySlots[i];
            if (curSlot.hovered && curSlot.hasItem())
            {
                currentDragSlotIndex = i; // Update the current drag slot index variable.

                currentDraggedItem = curSlot.getItem(); // Get the item from the current slot.
                dragIconImage.sprite = currentDraggedItem.icon; // Set the drag icon image sprite to the current items icon.
                dragIconImage.color = new Color(1, 1, 1, 1); // Make drag icon image visible.

                curSlot.setItem(null); // Remove the item from the current slot.
            }
        }
    }

    private void dropInventoryIcon()
    {
        // Reset the drag item variables
        dragIconImage.sprite = null; // Set the drag icon image sprite to null.
        dragIconImage.color = new Color(1, 1, 1, 0); // Make drag icon image invisible.

        for (int i = 0; i < allInventorySlots.Count; i++) // Loop through every inventory slot.
        {
            UISlot curSlot = allInventorySlots[i];
            if (curSlot.hovered) // Find the curslot we are trying to drop the item into.
            {
                if (curSlot.hasItem()) // Check to see if the slot is empty, if it isn't we need to swap the two items.
                {
                    Item itemToSwap = curSlot.getItem(); // Record the item in the slot we are trying to drop the new item into.

                    curSlot.setItem(currentDraggedItem); // Replace that item with the new item we are dragging.

                    allInventorySlots[currentDragSlotIndex].setItem(itemToSwap); // Set the old slots item as the one we just swapped place with.

                    resetDragVariables();
                    return;
                }
                else // If the slot is empty we can just move the item there.
                {
                    curSlot.setItem(currentDraggedItem);
                    resetDragVariables();
                    return;
                }
            }
        }
        
        // If we get to here it means we've dropped the item where there is no slot to place it into. Put it back where it was before.
        allInventorySlots[currentDragSlotIndex].setItem(currentDraggedItem);
        resetDragVariables();
    }

    private void resetDragVariables()
    {
        currentDraggedItem = null;
        currentDragSlotIndex = -1;
    }

    private void enableHotbarItem(int hotbarIndex)
    {
        foreach (GameObject a in equippableItems)
        {
            a.SetActive(false); // Set all the equippable items to be inactive.
        }

        UISlot hotbarSlot = hotbarSlots[hotbarIndex];

        if (hotbarSlot.hasItem()) // If the hotbar slot we have clicked has an item...
        {
            if (hotbarSlot.getItem().equippableItemIndex != -1) // And that item is equippable (which we are determining by if its equippableIndex isn't -1.
            {
                equippableItems[hotbarSlot.getItem().equippableItemIndex].SetActive(true); // Enable the object pointed to by the item in the horbar slot.
            }
        }
    }
}
