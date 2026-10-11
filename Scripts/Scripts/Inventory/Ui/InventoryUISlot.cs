using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryUISlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemImg;
    public TextMeshProUGUI itemTxt;
    public int slotIndex;
    private InventoryUIController inventoryUIController;
    private SlotViewData slotData;
    
    public void Initialize(InventoryUIController invUI, int index)
    {
        inventoryUIController = invUI;
        slotIndex = index;
    }
    public void UpdateSlot(SlotViewData slotData)
    {
        this.slotData = slotData;
        if (!slotData.HasItem)
        {
            ClearSlot();
        }
        else
        {
            SetSlotValues(slotData);
        }
    }
    private void SetSlotValues(SlotViewData data)
    {
        itemImg.sprite = data.Sprite;
        itemImg.color = Color.white;
        itemTxt.text = data.ItemAmount.ToString();
    }
    private void ClearSlot()
    {
        itemImg.sprite = null;
        itemImg.color = Color.clear;
        itemTxt.text = "";
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;
        
        DragAndDrop dragAndDrop = droppedObject.GetComponent<DragAndDrop>();
        if (dragAndDrop == null) return;
        
        InventoryUISlot droppedParentUiSlot = dragAndDrop.SourceSlot;
        if (droppedParentUiSlot == null) return;
        
        int droppedObjIndex = droppedParentUiSlot.slotIndex;
        inventoryUIController.RequestSwapItem(slotIndex, droppedObjIndex);
        
        if(slotData.HasItem)
            inventoryUIController.OnSlotHover(slotData);
        else
            inventoryUIController.OnSlotUnhover();
    }
    public void OnItemDropOut()
    {
        inventoryUIController.RequestDropItem(slotIndex);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!slotData.HasItem) return;
        inventoryUIController.OnSlotHover(slotData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryUIController.OnSlotUnhover();
    }
}
