using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class InventorySystem : MonoBehaviour
{
    public bool slowMotionWhenOpened = true;

    public bool IsInventoryOpened { get; private set; }

    private Dictionary<int, ItemSlotButton> slots;

    private Inventory inventory;

    private Player player;
    private GameController gameController;
    private int currentSlot = -1;

    void Start()
    {
        IsInventoryOpened = false;

        inventory = GetComponentInChildren<Inventory>();
        Assert.IsNotNull(inventory);
        inventory.gameObject.SetActive(false);

        player = FindObjectOfType<Player>();
        Assert.IsNotNull(player);

        slots = new Dictionary<int, ItemSlotButton>();
        foreach (ItemSlotButton slot in GetComponentsInChildren<ItemSlotButton>())
        {
            slots.Add(slot.slotIndex, slot);
        }

        gameController = FindObjectOfType<GameController>();
        Assert.IsNotNull(gameController);
    }

    public void OpenInventory(int slot)
    {
        IsInventoryOpened = true;
        currentSlot = slot;
        inventory.gameObject.SetActive(true);
        if (slowMotionWhenOpened)
        {
            gameController.RequestStartSlowmo(gameObject.name);
        }
    }

    public void SelectItemAndCloseInventory(EquipableId id)
    {
        Assert.AreNotEqual(currentSlot, -1);
        player.Equip(id, currentSlot);
        slots[currentSlot].ChangeIcon(id);
        CloseInventory();
    }

    public void CloseInventory()
    {
        IsInventoryOpened = false;
        currentSlot = -1;
        inventory.gameObject.SetActive(false);
        if (slowMotionWhenOpened)
        {
            gameController.RequestStopSlowmo(gameObject.name);
        }
    }
}
