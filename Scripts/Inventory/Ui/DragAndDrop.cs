using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Transform ogParent;
    private Canvas canvas;
    public InventoryUISlot SourceSlot  { get; private set; }
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        SourceSlot = GetComponentInParent<InventoryUISlot>();
        ogParent = transform.parent;
        if(canvas != null)transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(ogParent);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;

        if (SourceSlot != null && eventData.pointerEnter == null)
        {
            SourceSlot.OnItemDropOut();
        }
    }
}