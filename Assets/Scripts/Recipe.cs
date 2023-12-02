using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class Recipe : ScriptableObject
{
    public GameObject createdItemPrefab;
    public int quantityProduced = 1;
    public List<requiredIngredient> requiredIngredients = new List<requiredIngredient>();
}

[System.Serializable]
public class requiredIngredient
{
    public string itemName;
    public int requiredQuantity = 1;
}