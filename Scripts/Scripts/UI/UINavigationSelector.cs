using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Must be in the same GameObject as the IUIPageManager
public class UINavigationSelector : MonoBehaviour
{
    private IUIPageManager pageManager;
    private InputSystemUIInputModule uiInputModule;
    
    // Track last selected
    private Dictionary<GameObject, Selectable> lastSelectedDictionary = new Dictionary<GameObject, Selectable>();
    private GameObject currentPage;
    private void OnEnable()
    {
        if (pageManager == null && !TryGetComponent(out pageManager))
        {
            Debug.Log($"[UINavigationSelector] No UIPageManager found on {gameObject.name}");
            return;
        }
        pageManager.OnPageChanged += HandlePageChanged;
        pageManager.OnUIClosed += HandleUIClosed;

        if (uiInputModule == null)
            uiInputModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();

        if (uiInputModule != null)
        {
            uiInputModule.move.action.performed += OnNavigationInput;
            uiInputModule.submit.action.performed += OnNavigationInput;
        }
        HandlePageChanged();
    }

    private void OnDisable()
    {
        if (pageManager != null)
        {
            pageManager.OnPageChanged -= HandlePageChanged;
            pageManager.OnUIClosed -= HandleUIClosed;
        }

        if (uiInputModule != null)
        {
            uiInputModule.move.action.performed -= OnNavigationInput;
            uiInputModule.submit.action.performed -= OnNavigationInput;
        }
    }

    private void Update()
    {
        GameObject currentSelected = EventSystem.current?.currentSelectedGameObject;
        if (!currentPage || !currentSelected) return;
        if (currentSelected.TryGetComponent(out Selectable selectable))
            lastSelectedDictionary[currentPage] = selectable;
    }
    private void HandlePageChanged()
    {
        GameObject page = pageManager?.CurrentPage;
        currentPage = page;
        if (page == null) return;
        RestoreOrSetSelection(page);
    }

    private void HandleUIClosed()
    {
        EventSystem.current?.SetSelectedGameObject(null);
        currentPage = null;
    }
    
    private void OnNavigationInput(InputAction.CallbackContext ctx)
    {
        if (pageManager?.CurrentPage == null) return;
        if (EventSystem.current?.currentSelectedGameObject != null) return;
        RestoreOrSetSelection(pageManager.CurrentPage);
    }

    private void RestoreOrSetSelection(GameObject page)
    {
        if (page == null) return;
        if (TryRestoreSelectable(page)) return;
        if (SelectFirstInteractable(page)) return;
        
        EventSystem.current?.SetSelectedGameObject(null);
    }
    private bool TryRestoreSelectable(GameObject page)
    {
        if (!lastSelectedDictionary.TryGetValue(page, out Selectable selectable) || !ValidateSelectable(selectable)) 
            return false;
        
        EventSystem.current?.SetSelectedGameObject(selectable.gameObject);
        return true;
        
    }
    private bool SelectFirstInteractable(GameObject page)
    {
        if(page == null) return false;
        
        Selectable[] selectableArray = page.GetComponentsInChildren<Selectable>();
        foreach (Selectable selectable in selectableArray)
        {
            if (!ValidateSelectable(selectable)) continue;
            EventSystem.current?.SetSelectedGameObject(selectable.gameObject);
            return true;
        }
        return false;
    }
    private bool ValidateSelectable(Selectable selectable)
    {
        return selectable != null && selectable.isActiveAndEnabled && selectable.interactable;
    }
}