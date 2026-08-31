using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectiveTracker : MonoBehaviour
{
    [SerializeField] private List<ObjectiveSO> objectiveScriptableObjects;
    private List<Objective> objectives;
    
    private Objective currentObjective;
    private int currentIndex;
    
    private ObjectiveContext objectiveContext;
    
    public event Action OnObjectiveChanged;
    public event Action OnAllObjectivesCompleted;
    
    public event Action OnObjectiveProgress;

    public string CurrentObjDescription => currentObjective?.Description ?? "";
    public string CurrentObjProgressText => currentObjective?.GetProgressText() ?? "";

    public void Initialize(ObjectiveContext objectiveContext)
    {
        if (objectiveScriptableObjects == null || objectiveScriptableObjects.Count <= 0)
        {
            OnAllObjectivesCompleted?.Invoke();
            return;
        }
        this.objectiveContext = objectiveContext;
        objectives = new List<Objective>();
        foreach (ObjectiveSO objectiveData in objectiveScriptableObjects)
        {
            Objective obj = objectiveData.CreateObjective();
            objectives.Add(obj);
        }
        currentIndex = 0;
        SetCurrentObjective(objectives[currentIndex]);
    }
    private void SetCurrentObjective(Objective obj)
    {
        currentObjective = obj;
        currentObjective.OnCompleted += HandleCompleted;
        currentObjective.OnProgressUpdate += NotifyProgress;
        currentObjective.Initialize(objectiveContext);
        
        OnObjectiveChanged?.Invoke();
    }
    private void HandleCompleted()
    {
        currentObjective.OnCompleted -= HandleCompleted;
        currentObjective.OnProgressUpdate -= NotifyProgress;
        currentObjective.Cleanup();

        currentIndex++;

        if (currentIndex >= objectives.Count)
        {
            currentObjective = null;
            OnAllObjectivesCompleted?.Invoke();
            return;
        }
        
        SetCurrentObjective(objectives[currentIndex]);
    }
    private void NotifyProgress()
    {
        OnObjectiveProgress?.Invoke();
    }
}
