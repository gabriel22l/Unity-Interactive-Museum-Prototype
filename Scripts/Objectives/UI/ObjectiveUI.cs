using System;
using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    private ObjectiveTracker objectiveTracker;

    private void OnDisable()
    {
        if (objectiveTracker == null)
            return;

        objectiveTracker.OnObjectiveChanged -= UpdateUI;
        objectiveTracker.OnObjectiveProgress -= UpdateUI;
        objectiveTracker.OnAllObjectivesCompleted -= UpdateUI;
    }
    public void Initialize(ObjectiveTracker objectiveTracker)
    {
        this.objectiveTracker = objectiveTracker;
        if (objectiveTracker == null)
            return;
        
        objectiveTracker.OnObjectiveChanged += UpdateUI;
        objectiveTracker.OnObjectiveProgress += UpdateUI;
        objectiveTracker.OnAllObjectivesCompleted += UpdateUI;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        string description = objectiveTracker?.CurrentObjDescription ?? "";
        string progressText = objectiveTracker?.CurrentObjProgressText ?? "";
        //allow empty progress text but not empty description
        if (description == "")
        {
            SetEmptyText();
            return;
        } else if (progressText == "")
        {
            objectiveText.text = description;
            return;
        }
        objectiveText.text = $"{description}:  {progressText}";
    }
    private void SetEmptyText()
    {
        objectiveText.text = "";
    }
}
