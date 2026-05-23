using UnityEngine;
using System;
using TMPro;

public class UIController : MonoBehaviour
{
    private PlayerInput playerInput;
    
    [SerializeField] private GameObject playerMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject infoPanelMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject winGameOverMenu;
    
    private GameObject currentActiveMenu;
    
    [SerializeField] private InventoryUIController inventoryUIController;
    [SerializeField] private UIHealthBar uiHealthBar;
    [SerializeField] private ObjectiveUI objectiveUI;
    
    [Header("Info Panel")]
    [SerializeField] private TextMeshProUGUI infoTitle;
    [SerializeField] private TextMeshProUGUI infoText;

    public event Action OnUIOpen;
    public event Action OnUIClose;
    
    private bool isGameOver;
    
    private Coroutine gameOverCoroutine;
    private float gameOverDelay = 3;
    private float gameOverFadeDuration = 2;
    public void Initialize(PlayerContext playerContext, InventoryViewModel inventoryVM, ObjectiveTracker objectiveTracker)
    {
        this.playerInput = playerContext.PlayerInput;
        playerInput.OnPlayerMenuOpen += OnPlayerMenuOpen;
        playerInput.OnPauseMenuOpen += OnPauseMenuOpen;
        playerInput.CloseUIPressed += OnMenuClose;

        inventoryUIController?.Initialize(inventoryVM);
        uiHealthBar?.Initialize(playerContext.Health);
        objectiveUI?.Initialize(objectiveTracker);
        
        SetCursorState(false);
    }
    private void OnDisable()
    {
        Time.timeScale = 1;
        if(playerInput == null) return;
        playerInput.OnPlayerMenuOpen -= OnPlayerMenuOpen;
        playerInput.OnPauseMenuOpen -= OnPauseMenuOpen;
        playerInput.CloseUIPressed -= OnMenuClose;
    }
    private void OnPlayerMenuOpen()
    {
        SetActiveMenu(playerMenu, true);
    }
    private void OnPauseMenuOpen()
    {
        SetActiveMenu(pauseMenu, true, true); 
    }
    public void OpenInfoPanelMenu(itemDataSO itemData)
    {
        if(itemData == null) return;
        SetActiveMenu(infoPanelMenu, false);
        SetInfoPanelData(itemData);
    }
    private void SetInfoPanelData(itemDataSO data)
    {
        //could benefit from a data translator/intermediary but not needed right now.
        infoTitle.text = data.itemName;
        infoText.text = data.itemDescription;
    }
    public void OpenGameOverMenu()
    {
        OpenEndGameMenu(gameOverMenu, true);
    }
    public void OpenWinGameOverMenu()
    {
        OpenEndGameMenu(winGameOverMenu, false);
    }
    private void OpenEndGameMenu(GameObject menu, bool delay)
    {
        if(menu == null)
            return;
        SetActiveMenu(menu, true);
        isGameOver = true;
        if(gameOverCoroutine != null)
            return;
        gameOverCoroutine = StartCoroutine(GameOverFadeIn(menu, delay));
    }
    private void OnMenuClose()
    {
        if(isGameOver) return;
        currentActiveMenu?.SetActive(false);
        currentActiveMenu = null;
        
        Time.timeScale = 1;
        OnUIClose?.Invoke();
        
        SetCursorState(false);
    }
    private void SetActiveMenu(GameObject menu, bool showCursor, bool pauseGame = false)
    {
        if (isGameOver && menu != gameOverMenu && menu != winGameOverMenu)
            return;
        currentActiveMenu?.SetActive(false);
        menu.SetActive(true);
        currentActiveMenu = menu;
        OnUIOpen?.Invoke();
        
        SetCursorState(showCursor);
        PauseGame(pauseGame);
    }
    public void OnResumeButtonClick()
    {
        OnMenuClose();
    }
    private void SetCursorState(bool visible)
    {
        if (visible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void PauseGame(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
    }
    private System.Collections.IEnumerator GameOverFadeIn(GameObject obj, bool delay)
    {
        if (!obj.TryGetComponent(out CanvasGroup canvasGroup))
        {
            Debug.LogError($"No canvas group in {nameof(obj)}");
            yield break;
        }
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if(delay) yield return new WaitForSeconds(gameOverDelay);
        float t = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        while (t < gameOverFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f,  1f, t /  gameOverFadeDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
}
