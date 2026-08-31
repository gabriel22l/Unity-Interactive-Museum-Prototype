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
    private void Awake()
    {
        playerContext.Initialize(uiController, interactableIndicatorController);
        
        invVM = new InventoryViewModel(playerContext.InventoryController);
        uiController.Initialize(playerContext, invVM, objectiveTracker);

        playerContext.Health.OnDeath += HandleGameOver;
        
        InitializeObjectiveSystem();
    }
    private void OnDisable()
    {
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
        ObjectiveContext objectiveContext = new ObjectiveContext();
        objectiveContext.Inventory = playerContext.InventoryController;
        objectiveTracker.OnAllObjectivesCompleted += HandleWin;
        objectiveTracker.Initialize(objectiveContext);
    }
    private void HandleGameOver()
    {
        if(isGameOver) 
            return;
        uiController.OpenGameOverMenu();
        isGameOver = true;
    }
    private void HandleWin()
    {
        if (isGameOver || !gameOverOnWin)
            return;
        uiController.OpenWinGameOverMenu();
        isGameOver = true;
        playerContext.Health?.SetCanTakeDamage(false);
    }
}
