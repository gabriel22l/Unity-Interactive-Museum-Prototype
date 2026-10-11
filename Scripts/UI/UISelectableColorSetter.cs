using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UISelectableColorSetter : MonoBehaviour
{
    private IUIPageManager pageManager;
    [SerializeField] private Color32 selectedColor = new Color32(175, 255, 80, 255);
    [SerializeField] private Color32 highlightedColor = new Color32(175, 255, 80, 255);
    
    private HashSet<GameObject> seenPages = new HashSet<GameObject>();

    private void OnEnable()
    {
        if (pageManager == null && !TryGetComponent(out pageManager))
        {
            Debug.Log($"[UISelectableColorSetter] No UIPageManager found on {gameObject.name}");
            return;
        }
        pageManager.OnPageChanged += HandlePageChanged;
    }
    private void OnDisable()
    {
        if (pageManager != null)
        {
            pageManager.OnPageChanged -= HandlePageChanged;
        }
    }
    private void HandlePageChanged()
    {
        SetSelectablesColors(pageManager.CurrentPage);
    }
    private void SetSelectablesColors(GameObject page)
    {
        if (page == null) return;
        if (!seenPages.Add(page)) return;
        foreach (Selectable selectable in page.GetComponentsInChildren<Selectable>())
        {
            ColorBlock colors = selectable.colors;
            colors.selectedColor = selectedColor;
            colors.highlightedColor = highlightedColor;
            selectable.colors = colors;
        }
    }
}
