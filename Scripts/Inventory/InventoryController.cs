using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    public int slotAmount = 8;
    public InventorySlot[] slots;
    public InventorySlot[] startingItems;
    public event Action OnInventoryChanged;

    //Initialization
    private void Awake()
    {
        slots = new InventorySlot[slotAmount];
        for (int i = 0; i < slotAmount; i++)
        {
            slots[i] = new InventorySlot();
            if (startingItems != null && i < startingItems.Length 
                                      && startingItems[i] != null 
                                      && startingItems[i].itemDataSo != null 
                                      && startingItems[i].amount > 0) 
            {   //set startingItems in inventory
                slots[i].itemDataSo = startingItems[i].itemDataSo;
                slots[i].amount = startingItems[i].amount;
            }
        }

    }
    
    #region Add items
    public int AddItem(itemDataSO itemData, int amount)
    { 
        if(itemData == null ||  amount <= 0) return amount;
        amount = TryStacking(itemData, amount); // try stacking and return remaining amount
        if (amount == 0)
        {
            OnInventoryChanged?.Invoke();
            return 0;
        }
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty) continue;
            slot.itemDataSo = itemData;
            int toAdd = amount < slot.itemDataSo.maxStackAmount ? 
                amount : slot.itemDataSo.maxStackAmount; //check the amount to add
            slot.amount = toAdd;
            amount -= toAdd;
            if (amount == 0)
            {
                break;
            }
        }
        OnInventoryChanged?.Invoke();
        return amount;
    }

    public int TryStacking(itemDataSO itemData, int amount)
    {
        foreach (InventorySlot slot in slots ) //try stacking
        {
            if (slot.itemDataSo == itemData && !slot.IsFull) 
            { //if current slot item is same as item and slot amount < maxStack try stacking
                int remainingCapacity = slot.itemDataSo.maxStackAmount - slot.amount;
                if (amount <= remainingCapacity)  //stack all  if remaining slot capacity is less or equal to remaining capacity
                {
                    slot.amount += amount;
                    return 0;
                }  else if (amount > remainingCapacity) //if amount is more than remaining capacity -> add remaining, look for empty slot to add rest
                {
                    slot.amount += remainingCapacity;
                    amount -= remainingCapacity;
                }
            }
        }
        return amount;
    }
    #endregion
    
    #region Drop/Slot interaction
    public void DropItem(int index)
    {
        if (index < 0 || index > slots.Length - 1) return; //return if index is invalid
        InventorySlot slot = slots[index];
        if (slot.IsEmpty) return;
        
        SpawnItem(slot);
       
        ClearSlot(slot);
        OnInventoryChanged?.Invoke();
    }
    private void SpawnItem(InventorySlot slot)
    {
        float spawnOffsetAmount = 2f;
        Vector3 spawnPos = transform.position + Vector3.up * spawnOffsetAmount + transform.forward * spawnOffsetAmount;

        //create a raycast to find the ground and adjust spawn position to be where the ground hit + 1 in the y dir
        if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hitInfo,  10f))
        {
            spawnPos = hitInfo.point + Vector3.up;
        } 
        
        GameObject itemPrefab = slot.itemDataSo.itemPrefab;
        GameObject spawnedObj = 
            Instantiate(itemPrefab, spawnPos, itemPrefab.transform.rotation);
        
        ItemWorldObject itemWorldObject = spawnedObj.GetComponent<ItemWorldObject>();
        if(itemWorldObject != null) itemWorldObject.amount = slot.amount; //set the amount = amount in inventory slot
    }
    public void SwapItem(int indexTo, int indexFrom)
    {
        if (indexTo < 0 || indexTo >= slots.Length) return; //return if index out of bounds
        if (indexFrom < 0 || indexFrom >= slots.Length) return;
        
        InventorySlot targetSlot = slots[indexTo];
        InventorySlot ogSlot = slots[indexFrom];
        
        if (targetSlot.itemDataSo == null && ogSlot.itemDataSo == null) return; //return if both items are null
        
        if (targetSlot.itemDataSo == ogSlot.itemDataSo)
        {
            bool mergeSuccessful =  TryMerging(targetSlot, ogSlot);
            if (mergeSuccessful) {
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        
        //assign invSlot1 = invSlot2 and vice versa, then invoke invChanged event
        itemDataSO ogSlotItem = ogSlot.itemDataSo;
        itemDataSO targetSlotItem = targetSlot.itemDataSo;
        int ogSlotAmount = ogSlot.amount;
        int  targetSlotAmount = targetSlot.amount;
        
        slots[indexTo].itemDataSo = ogSlotItem;
        slots[indexTo].amount = ogSlotAmount;
        slots[indexFrom].itemDataSo = targetSlotItem;
        slots[indexFrom].amount = targetSlotAmount;
        OnInventoryChanged?.Invoke();
    }
    private bool TryMerging(InventorySlot targetSlot, InventorySlot ogSlot)
    {
        itemDataSO itemTypeSo  = targetSlot.itemDataSo;
        if (targetSlot.IsFull || ogSlot.IsFull) return false; //if either is full, return
        
        int targetSlotRemainingCapacity = itemTypeSo.maxStackAmount - targetSlot.amount;

        if (targetSlotRemainingCapacity >= ogSlot.amount)
        {
                targetSlot.amount += ogSlot.amount;
                ClearSlot(ogSlot);
        }
        else
        {
                targetSlot.amount += targetSlotRemainingCapacity;
                ogSlot.amount -= targetSlotRemainingCapacity;
        }
        return true;
    }
    #endregion

    #region resources
    
    public bool HasResources(List<ResourceCost> resourceList)
    {
        foreach (ResourceCost resource in resourceList )
        {
            itemDataSO item = resource.itemDataSo;
            int amount = resource.amount;
            bool hasResource = HasResource(item, amount);
            if(!hasResource) return false;
        }
        return true;
    }
    public bool HasResource(itemDataSO resourceData, int amount)
    {
        int totalAmount = 0;
        foreach (InventorySlot slot in slots)
        {
            if (slot.itemDataSo != resourceData) continue;
            totalAmount += slot.amount;
            if(totalAmount >= amount) return true;
        }
        return false;
    }
    public bool RemoveResources(List<ResourceCost> resourceList)
    {
        if(!HasResources(resourceList)) return false;
        foreach (ResourceCost resource in resourceList )
        {
            itemDataSO item = resource.itemDataSo;
            int amount =  resource.amount;
            bool removed = RemoveResource(item, amount);
            if(!removed) return false; //safety, just in case, probably overkill
        }
        OnInventoryChanged?.Invoke();
        return true;
    }
    public bool RemoveResource(itemDataSO resource, int amount) 
    {
        if(!HasResource(resource, amount)) return false;
        foreach (InventorySlot slot in slots)
        {
            if (slot.itemDataSo != resource) continue;
            if (slot.amount >= amount)
            {
                slot.amount -= amount;
                if (slot.amount <= 0)
                    ClearSlot(slot);
                return true;
            }
            else
            {
                int thisSlotAmount = slot.amount;
                ClearSlot(slot);
                amount -= thisSlotAmount;
            }
        }
        return false; //compiler requirement, function will never reach this point
    }
    #endregion
    public itemDataSO GetFirstItemOfType(ItemType itemType)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.itemDataSo == null || slot.itemDataSo.itemType != itemType)
                continue;
            return slot.itemDataSo;
        }
        return null;
    }
    public int GetItemCountByType(ItemType itemType)
    {
        int count = 0;
        foreach (InventorySlot slot in slots)
        {
            if(slot.itemDataSo == null || slot.itemDataSo.itemType != itemType)
                continue;
            count += slot.amount;
        }
        return count;
    }
    private void ClearSlot(InventorySlot slot)
    {
        slot.itemDataSo = null;
        slot.amount = 0;
    }
}