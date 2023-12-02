using UnityEngine;

public class Item : MonoBehaviour
{
    public new string name = "New Item";
    public string description = "New Description";
    public Sprite icon;
    public int currentQuantity = 1;
    public int maxQuantity = 16;

    public int equippableItemIndex = -1;

    public ItemRarity itemRarity = ItemRarity.Common;
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Unique
    }

    public Color getItemRarity()
    {
        switch (itemRarity)
        {
            case ItemRarity.Common:
                return Color.gray;
            case ItemRarity.Uncommon:
                return Color.green;
            case ItemRarity.Rare:
                return Color.blue;
            case ItemRarity.Legendary:
                return new Color(1.0f, 0.5f, 0.0f); // Orange
            case ItemRarity.Unique:
                return Color.red;
            default:
                return Color.white;
        }
    }
}
