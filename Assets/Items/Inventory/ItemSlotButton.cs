using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ItemSlotButton : MonoBehaviour
{

    public int slotIndex;

    private InventorySystem inventorySystem;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        Assert.IsNotNull(image);

        inventorySystem = GetComponentInParent<InventorySystem>();
        Assert.IsNotNull(inventorySystem);

        GetComponent<Button>().onClick.AddListener(delegate { inventorySystem.OpenInventory(slotIndex); });
    }

    public void ChangeIcon(EquipableId id)
    {
        image.sprite = ItemsDataBase.GetInstance().GetSprite(id);
    }
}
