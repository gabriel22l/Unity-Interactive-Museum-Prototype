using System;
using UnityEngine;

public class InventoryViewModel
{
    private readonly InventoryController inventoryController;
    public event Action OnInvDataChanged;
    public int InventorySlotAmount => inventoryController.slotAmount;

    public InventoryViewModel(InventoryController inventoryC)
    {
        inventoryController = inventoryC;
        inventoryC.OnInventoryChanged += OnDataChanged;
    }
    public void Dispose()
    {
        inventoryController.OnInventoryChanged -= OnDataChanged;
    }

    private void OnDataChanged()
    {
        OnInvDataChanged?.Invoke();
    }

    public SlotViewData GetInventoryViewData(int index)
    {
        SlotViewData data;
        InventorySlot slot = inventoryController.slots[index];
        itemDataSO itemData = slot.itemDataSo;
        if (slot.itemDataSo != null)
        {
            data = new SlotViewData(true, itemData.itemIcon, slot.amount, itemData.itemName, itemData.itemDescription);
        } else
        {
            data = new SlotViewData(false, null, 0, null, null);
        }
        return data;
    }
    public void RequestSwapItems(int indexTo, int indexFrom)
    {
        inventoryController.SwapItem(indexTo, indexFrom);
    }
    public void RequestDropItem(int index)
    {
        inventoryController.DropItem(index);
    }
}

public struct SlotViewData
{
    public readonly bool HasItem;
    public readonly Sprite Sprite;
    public readonly int ItemAmount;
    public readonly string ItemName;
    public readonly string ItemDescription;

    public SlotViewData(bool hasItem, Sprite sprite, int itemAmount, string itemName, string itemDescription)
    {
        HasItem = hasItem;
        Sprite = sprite;
        ItemAmount = itemAmount;
        ItemName = itemName;
        ItemDescription = itemDescription;
    }
}