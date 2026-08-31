using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject actionTextPrefab;
    [SerializeField] private InputActionAsset inputAsset;
    [SerializeField] private GameObject controlsContainer;
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject controlsPage;
    [SerializeField] private GameObject playPage;
    private GameObject currentPage;

    private List<GameObject> inputTexts;
    private void Awake()
    {
        if(controlsPage != null) controlsPage.SetActive(false);
        if(mainMenuPage != null) mainMenuPage.SetActive(false);
        if(playPage != null) playPage.SetActive(false);
        SwitchActivePage(mainMenuPage);
    }
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }
    public void ExitGame()
    {
        #if UNITY_EDITOR 
        UnityEditor.EditorApplication.isPlaying = false;
  #endif
        Application.Quit();
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
    }
    public void ReloadScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
