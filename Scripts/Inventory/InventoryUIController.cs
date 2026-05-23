using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUIController : MonoBehaviour
{
    private InventoryViewModel inventoryViewModel;
    [SerializeField] private GameObject slotPrefab;
    private InventoryUISlot[] uiSlots;

    [SerializeField] private GameObject itemDataPanel;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    public void Initialize(InventoryViewModel ivm) 
    {
        this.inventoryViewModel = ivm;
        InstantiateSlots();
    }
    private void InstantiateSlots() //only call in initialize
    {   
        //uiSlots amount = inventoryC slot amount
        uiSlots = new InventoryUISlot[inventoryViewModel.InventorySlotAmount]; 
        
        //instantiate slots, get the class containing text/img child objects
        for(int i = 0; i < inventoryViewModel.InventorySlotAmount; i++) 
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            uiSlots[i] = slot.GetComponent<InventoryUISlot>();
            uiSlots[i].Initialize(this, i);
        }
    }
    private void OnEnable() 
    { 
        //only refresh and subscribe to events while enabled to prevent array reference bugs
        if (inventoryViewModel == null) return;
        RefreshInventoryUI(); 
        inventoryViewModel.OnInvDataChanged += RefreshInventoryUI;
        
        if(itemDataPanel.activeSelf) 
            itemDataPanel.SetActive(false);
        ClearPanelData();
    }
    private void OnDisable()
    {
        if(inventoryViewModel == null) return;
        inventoryViewModel.OnInvDataChanged -= RefreshInventoryUI;
    }

    private void RefreshInventoryUI()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {   
            SlotViewData currentSlotData = inventoryViewModel.GetInventoryViewData(i);
            uiSlots[i].UpdateSlot(currentSlotData);
        }
    }
    public void RequestSwapItem(int indexTo, int indexFrom)
    {
        if(indexTo >= uiSlots.Length || indexTo < 0 || indexFrom  >= uiSlots.Length || indexFrom < 0) return;
        inventoryViewModel.RequestSwapItems(indexTo, indexFrom);
    }
    public void RequestDropItem(int index)
    {
        inventoryViewModel.RequestDropItem(index);
    }
    public void OnSlotHover(SlotViewData data)
    {
        if(!itemDataPanel.activeSelf)
            itemDataPanel.SetActive(true);
        SetPanelData(data);
    }
    public void OnSlotUnhover()
    {
        if(itemDataPanel.activeSelf)
            itemDataPanel.SetActive(false);
        ClearPanelData();
    }
    private void SetPanelData(SlotViewData data)
    {
        itemNameText.text = data.ItemName;
        itemDescriptionText.text = data.ItemDescription;
    }
    private void ClearPanelData()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
    }
}