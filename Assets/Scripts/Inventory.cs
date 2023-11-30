using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int inventorySpace = 10;
    public LayerMask itemLayer;
    public float pickUpDistance = 5f;

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
                    bool itemAdded = addItem(item);

                    if (itemAdded)
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            displayInventory();
        }
    }

    private bool addItem(Item item)
    {
        if (items.Count < inventorySpace)
        {
            items.Add(item);
            Debug.Log("Item added: " + item.name);
            return true;
        }
        else
        {
            Debug.Log("Inventory full - cannot add item: " + item.name);
            return false;
        }
    }

    private void displayInventory()
    {
        // toggleMouseLock();
        foreach (Item item in items)
        {
            Debug.Log(item.name);
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
