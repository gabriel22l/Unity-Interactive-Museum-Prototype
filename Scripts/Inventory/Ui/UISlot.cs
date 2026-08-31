using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class UISlot : MonoBehaviour
{
    private Image slotImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        slotImg = GetComponent<Image>();
    }
    public void SetValues(string txt, Sprite sprite)
    {
        if(text != null)
            text.text = txt;
        
        if (iconImg != null)
        {
            iconImg.sprite = sprite;
            iconImg.color = Color.white;
        }
    }
    public void Clear()
    {
        if (iconImg != null)
        {
            iconImg.sprite = null;
            iconImg.color = new Color32(0, 0, 0, 0);
        }
        if(text != null)
            text.text = "";
    }
    public void SetTextColor(Color color)
    {
        if(text != null)
            text.color = color;
    }
    public void SetIconColor(Color color)
    {
        if(iconImg != null)
            iconImg.color = color;
    }
    public void SetSlotBackgroundColor(Color color)
    {
        if(slotImg != null)
            slotImg.color = color;
    }
}
