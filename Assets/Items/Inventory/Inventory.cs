using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public List<EquipableId> itemsInInventory;

    private InventorySystem inventorySystem;

    private void Start()
    {
        inventorySystem = GetComponentInParent<InventorySystem>();
        Assert.IsNotNull(inventorySystem);

        LoadInventory();
    }

    private void LoadInventory()
    {
        foreach (EquipableId id in itemsInInventory)
        {
            GameObject item = Object.Instantiate(inventorySlotPrefab);
            item.transform.SetParent(gameObject.transform, false);
            item.GetComponent<Image>().sprite = ItemsDataBase.GetInstance().GetSprite(id);
            item.GetComponent<Button>().onClick.AddListener(delegate { inventorySystem.SelectItemAndCloseInventory(id); });
        }
    }
}
