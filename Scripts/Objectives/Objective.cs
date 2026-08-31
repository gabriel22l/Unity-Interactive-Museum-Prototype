using UnityEngine;
using System;

public abstract class Objective
{
    public bool IsComplete { get; protected set; }
    
    public event Action OnCompleted;
    public event Action OnProgressUpdate;
    
    public abstract string Description { get; }
    public abstract string GetProgressText();
    
    public abstract void Initialize(ObjectiveContext context);
    public abstract void Cleanup();
    
    protected void Complete()
    {
        if (IsComplete)
            return;
        IsComplete = true;
        OnCompleted?.Invoke();
    }
    protected void NotifyProgressUpdated()
    {
        OnProgressUpdate?.Invoke();
    }
}

public class ObjectiveContext
{
    public InventoryController Inventory;
}
