using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MainMenu : MonoBehaviour, IUIPageManager
{
    [SerializeField] private GameObject actionTextPrefab;
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private GameObject controlsContainer;
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject controlsPage;
    [SerializeField] private GameObject playPage;
    [SerializeField] private GameObject optionsPage;
    private GameObject currentPage;

    private List<GameObject> inputTexts;

    public event Action OnPageChanged;
    public event Action OnUIClosed;
    public GameObject CurrentPage => currentPage;

    private void Awake()
    {
        if(controlsPage != null) controlsPage.SetActive(false);
        if(mainMenuPage != null) mainMenuPage.SetActive(false);
        if(playPage != null) playPage.SetActive(false);
        if(optionsPage != null) optionsPage.SetActive(false);
        SwitchActivePage(mainMenuPage);
    }

    public void LoadScene(string sceneName)
    {
        ApplicationController.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        ApplicationController.ExitGame();
    }

    public void ReloadScene()
    {
        ApplicationController.ReloadScene();
    }

    public void ShowControlsPage()
    {
        SwitchActivePage(controlsPage);
        if(inputTexts == null || inputTexts.Count == 0) GenerateInputTexts();
    }

    private void GenerateInputTexts()
    {
        inputTexts = new List<GameObject>();
        foreach (InputActionMap actionMap in inputAsset.actionMaps)
        {
            foreach (InputAction action in actionMap.actions)
            {
                string actionName = action.name.
                    Replace("Movement", "Movimiento").
                    Replace("Look", "Ver").
                    Replace("Run", "Correr").
                    Replace("Interact", "Interactuar").
                    Replace("OpenPlayerMenu", "Abrir Menú").
                    Replace("OpenPauseMenu", "Pausar").
                    Replace("ToggleView", "Cambiar Vista").
                    Replace("Throw", "Soltar Ofrenda").
                    Replace("CloseUI",  "Cerrar Menú");
                string binding = action.GetBindingDisplayString().
                    Replace("Delta", "Mouse").
                    Replace("Tabulacion", "Tab");
                GameObject inputText = Instantiate(actionTextPrefab, controlsContainer.transform);
                inputTexts.Add(inputText);
                inputText.TryGetComponent(out TextMeshProUGUI text);
                text.text = $"{actionName} - {binding}";
            }
        }
    }

    public void ShowPlayPage()
    {
        SwitchActivePage(playPage);
    }

    public void ReturnToMain()
    {
        SwitchActivePage(mainMenuPage);
    }

    private void SwitchActivePage(GameObject page)
    {
        if (page == null || page == currentPage) return;
        currentPage?.SetActive(false);
        page.SetActive(true);
        currentPage = page;
        
        OnPageChanged?.Invoke();
    }

    public void ShowOptionsPage()
    {
        SwitchActivePage(optionsPage);
    }
}