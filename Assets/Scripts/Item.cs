using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemName = "New Item";
    public Sprite icon = null;
    public int currentQuantity = 1;
    public int maxQuantity = 12;

    public int uiSlotIndex;

    public int handIndex = -1;
}
