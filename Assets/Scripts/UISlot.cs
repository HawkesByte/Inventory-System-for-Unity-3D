using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovered;
    private Item heldItem;

    private Color opaque = new Color(1, 1, 1, 1);
    private Color transparent = new Color(1, 1, 1, 0);

    private Image thisSlotImage;
    private Text thisSlotQuantityText;

    private Inventory inventory;

    public enum ArmourType
    {
        None,
        Head,
        Chest,
        Backpack,
        Legs,
        Feet
    }

    public ArmourType armourType = ArmourType.None;

    public void initialiseSlot(Inventory inv)
    {
        thisSlotImage = gameObject.GetComponent<Image>();
        thisSlotQuantityText = transform.GetChild(0).GetComponent<Text>();
        thisSlotQuantityText.text = "";
        thisSlotImage.sprite = null;
        thisSlotImage.color = transparent;
        setItem(null);

        inventory = inv;
    }

    public void setItem(Item item)
    {
        heldItem = item;
        
        if (item != null)
        {
            thisSlotImage.sprite = heldItem.icon;
            thisSlotImage.color = opaque;
            updateData();
        }
        else
        {
            thisSlotImage.sprite = null;
            thisSlotImage.color = transparent;
            updateData();
        }
    }

    public Item getItem()
    {
        return heldItem;
    }

    public bool hasItem()
    {
        if (heldItem)
            return true;

        return false;
    }

    public void updateData()
    {
        if (heldItem != null)
        {
            thisSlotQuantityText.text = heldItem.currentQuantity.ToString();
        }
        else
        {
            thisSlotQuantityText.text = "";
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        hovered = true;

        if (inventory && hasItem()) // If we hover over the inventory and have an item.
            inventory.setHoveredSlotInfo(getItem(), transform.position); // Pass in the data
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        hovered = false;

        if (inventory) // If we exit the slot
            inventory.hideHoveredSlotInfo();
    }
}
