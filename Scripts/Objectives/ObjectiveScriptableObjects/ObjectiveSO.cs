using UnityEngine;

public abstract class ObjectiveSO : ScriptableObject
{
    public string Description;
    public abstract Objective CreateObjective();
}
