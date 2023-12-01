using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Inventory inv;

    public void OnPointerEnter(PointerEventData eventData)
    {
        inv.currentHoveredSlot = inv.inventorySlots.IndexOf(GetComponent<Image>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inv.currentHoveredSlot = -1;
    }
}
