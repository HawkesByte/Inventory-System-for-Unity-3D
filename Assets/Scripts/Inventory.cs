using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventory;
    public List<UISlot> InventorySlots = new List<UISlot>();
    public Image crosshair;
    public Text itemHoverText;

    [Header("Raycast")]
    public float raycastDistance;
    public LayerMask itemLayer;

    public void Start()
    {
        toggleInventory(false);

        foreach (UISlot uiSlot in InventorySlots)
        {
            uiSlot.initialiseSlot();
        }
    }

    public void Update()
    {
        itemRaycast(Input.GetMouseButtonDown(0));

        if (Input.GetKeyDown(KeyCode.Q))
            dropItem();

        if (Input.GetKeyDown(KeyCode.E))
            toggleInventory(!inventory.activeInHierarchy);
    }

    private void itemRaycast(bool hasClicked = false)
    {
        itemHoverText.text = "";
        Ray ray = Camera.main.ScreenPointToRay(crosshair.transform.position);
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
        for (int i = 0; i < InventorySlots.Count; i++) // Loop through every inventory slot
        {
            Item heldItem = InventorySlots[i].getItem();
            if (heldItem != null && itemToAdd.name == heldItem.name) // Meaning both these items are the same type.
            {
                int freeSpaceInSlot = heldItem.maxQuantity - heldItem.currentQuantity; // Calculate how much space this slot has until its full.
                
                if (freeSpaceInSlot >= leftoverQuantity) // We can add the entire quantity of the item. 
                {
                    heldItem.currentQuantity += leftoverQuantity;
                    Destroy(itemToAdd.gameObject); // We can destroy this gameobject as its quantity is now stored within another item.
                    return;
                }
                else // Add as much as possible to the current slot.
                {
                    heldItem.currentQuantity = heldItem.maxQuantity;
                    leftoverQuantity -= freeSpaceInSlot; // Decrement quantityToAdd by the amount we where able to put into an existing item.
                }
            }
            else if(heldItem == null) 
            {
                if(!openSlot) // If open slot has not be assigned a slot yet.
                    openSlot = InventorySlots[i]; // Take note that we have an open slot.
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

            foreach (UISlot a in InventorySlots) // If we close inventory without leaving a slot then we leave it hovered (need to find a nicer fix).
            {
                a.hovered = false;
            }
        }
    }

    private void dropItem() // Drop an item into the world space
    {
        for (int i = 0; i < InventorySlots.Count; i++) // Loop through every inventory slot
        {
            UISlot curSlot = InventorySlots[i];
            if (curSlot.hovered && curSlot.getItem() != null)
            {
                curSlot.getItem().gameObject.SetActive(true);
                curSlot.getItem().transform.position = new Vector3(transform.position.x + 2, transform.position.y + 1, transform.position.z);
                curSlot.setItem(null);
                break;
            }
        }
    }
}
