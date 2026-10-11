using UnityEngine;
using System;
using TMPro;

public class UIController : MonoBehaviour, IUIPageManager
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
    
    private IPauseControl pauseControl;

    public event Action OnUIOpen;
    public event Action OnUIClose;
    
    private bool isGameOver;
    
    private Coroutine gameOverCoroutine;
    private float defaultGameOverDelayTime = 3;
    private float defaultGameOverFadeDuration = 2;

    private float winGameOverDelayTime = 0.5f;
    private float winGameOverFadeDuration = 1f;
    
    //IUIPageManager events
    public event Action OnPageChanged;
    public event Action OnUIClosed;
    public GameObject CurrentPage => currentActiveMenu;
    
    #region Initialization & Subscriptions
    public void Initialize(PlayerContext playerContext, ObjectiveTracker objectiveTracker, IPauseControl pauseControl)
    {
        this.playerInput = playerContext.PlayerInput;
        playerInput.OnPlayerMenuOpen += HandlePlayerMenuOpen;
        playerInput.OnPauseMenuOpen += HandlePauseMenuOpen;
        playerInput.CloseUIPressed += CloseActiveMenu;

        inventoryUIController?.Initialize(playerContext.InventoryViewModel);
        uiHealthBar?.Initialize(playerContext.Health);
        objectiveUI?.Initialize(objectiveTracker);
        
        this.pauseControl = pauseControl;

        SetCursorVisible(false);
    }
    private void OnDisable()
    {
        if(playerInput == null) return;
        playerInput.OnPlayerMenuOpen -= HandlePlayerMenuOpen;
        playerInput.OnPauseMenuOpen -= HandlePauseMenuOpen;
        playerInput.CloseUIPressed -= CloseActiveMenu;
    }
    #endregion
    #region Menu Handling
    //Called on PlayerInput.OnPlayerMenuOpen event
    private void HandlePlayerMenuOpen()
    {
        SetActiveMenu(playerMenu, showCursor: true);
    }
    // Called on PlayerInput.PauseMenuOpen event
    private void HandlePauseMenuOpen()
    {
        SetActiveMenu(pauseMenu, showCursor: true, pauseGame: true); 
    }
    // Called on PlayerInput.CloseUIPressed event
    private void CloseActiveMenu()
    {
        if(isGameOver) return;
        currentActiveMenu?.SetActive(false);
        currentActiveMenu = null;
        
        SetGamePaused(false);
        OnUIClose?.Invoke();
        OnUIClosed?.Invoke();
        
        SetCursorVisible(false);
    }
    // Called from the Resume button in the pause menu
    public void OnResumeButtonClick()
    {
        CloseActiveMenu();
    }
    private void SetActiveMenu(GameObject menu, bool showCursor, bool pauseGame = false)
    {
        if (isGameOver && menu != gameOverMenu && menu != winGameOverMenu)
            return;
        if(menu == null)
            return;
        currentActiveMenu?.SetActive(false);
        menu.SetActive(true);
        currentActiveMenu = menu;
        OnUIOpen?.Invoke();
        OnPageChanged?.Invoke();
        
        SetCursorVisible(showCursor);
        SetGamePaused(pauseGame);
    }
    #endregion
    #region External Menu Openers
    public void OpenInfoPanel(itemDataSO itemData)
    {
        if(itemData == null) return;
        SetActiveMenu(infoPanelMenu, showCursor: false);
        UpdateInfoPanelContent(itemData);
    }
    private void UpdateInfoPanelContent(itemDataSO data)
    {
        //could benefit from a data translator/intermediary but not needed right now.
        infoTitle.text = data.itemName;
        infoText.text = data.itemDescription;
    }
    public void ShowLoseScreen()
    {
        ShowEndgameScreen(gameOverMenu, true);
    }
    public void ShowWinScreen()
    {
        ShowEndgameScreen(winGameOverMenu, true, winGameOverDelayTime, winGameOverFadeDuration);
    }
    #endregion
    #region GameOver presentation
    private void ShowEndgameScreen(GameObject menu, bool delay, float delayTime = 0, float fadeDuration = 0)
    {
        if(menu == null)
            return;
        SetActiveMenu(menu, showCursor: true);
        isGameOver = true;
        if(gameOverCoroutine != null)
            return;
        gameOverCoroutine = StartCoroutine(FadeInEndgameScreen(menu, delay, delayTime, fadeDuration));
    }
    private System.Collections.IEnumerator FadeInEndgameScreen(GameObject obj, bool delay, float delayTime = 0, float fadeDuration = 0)
    {
        if (!obj.TryGetComponent(out CanvasGroup canvasGroup))
        {
            Debug.LogError($"No canvas group in {nameof(obj)}");
            yield break;
        }
        fadeDuration = fadeDuration > 0 ? fadeDuration : defaultGameOverFadeDuration;
        delayTime = delayTime > 0 ? delayTime : defaultGameOverDelayTime;
        
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        if(delay) yield return new WaitForSeconds(delayTime);
        
        float t = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f,  1f, t /  fadeDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    #endregion
    #region helpers
    private void SetCursorVisible(bool visible)
    {
        ApplicationController.SetCursorVisible(visible);
    }
    private void SetGamePaused(bool pause)
    {
        pauseControl?.RequestPause(pause);
    }
    #endregion
}