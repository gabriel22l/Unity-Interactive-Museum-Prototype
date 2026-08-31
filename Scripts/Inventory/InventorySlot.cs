
[System.Serializable]
public class InventorySlot
{
    public itemDataSO itemDataSo;
    public int amount;
    public bool IsEmpty => itemDataSo == null || amount <= 0;
    public bool IsFull => itemDataSo != null && amount >= itemDataSo.maxStackAmount;
}
