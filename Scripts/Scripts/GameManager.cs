using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player and UI")]
    [SerializeField] private PlayerContext playerContext;
    [SerializeField] private UIController uiController;
    [SerializeField] private InteractableIndicatorController interactableIndicatorController;
    
    InventoryViewModel invVM;
    
    private bool isGameOver;

    [Header("Objective System")]
    [SerializeField] private ObjectiveTracker objectiveTracker;
    
    [SerializeField] private bool gameOverOnWin = true;
    
    [Header("Pause Control")]
    [SerializeField] private bool allowPause = true;
    private IPauseControl pauseControl;
    private void Start()
    {
        playerContext.Initialize(uiController, interactableIndicatorController);
        
        pauseControl = new PauseControl(allowPause);
        pauseControl.RequestPause(false);
        uiController.Initialize(playerContext, objectiveTracker, pauseControl);

        playerContext.Health.OnDeath += HandleGameOver;
        
        InitializeObjectiveSystem();
    }
    private void OnDisable()
    {
        pauseControl?.RequestPause(false);
        if(playerContext?.Health)
            playerContext.Health.OnDeath -= HandleGameOver;
        if(objectiveTracker != null)
            objectiveTracker.OnAllObjectivesCompleted -= HandleWin;
    }
    private void InitializeObjectiveSystem()
    {
        if (objectiveTracker == null && !TryGetComponent(out objectiveTracker))
        {
            Debug.Log($"{nameof(objectiveTracker)} could not be found");
            return;
        }
        ObjectiveContext objectiveContext = new ObjectiveContext(playerContext);
        objectiveTracker.OnAllObjectivesCompleted += HandleWin;
        objectiveTracker.Initialize(objectiveContext);
    }
    private void HandleGameOver()
    {
        if(isGameOver) 
            return;
        uiController.ShowLoseScreen();
        isGameOver = true;
    }
    private void HandleWin()
    {
        if (isGameOver || !gameOverOnWin)
            return;
        uiController.ShowWinScreen();
        isGameOver = true;
        playerContext.Health?.SetCanTakeDamage(false);
    }
}
