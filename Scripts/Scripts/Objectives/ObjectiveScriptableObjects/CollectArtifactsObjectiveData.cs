using UnityEngine;

[CreateAssetMenu(fileName = "CollectArtifactsObjectiveData", menuName = "Scriptable Objects/CollectArtifactsObjectiveData")]
public class CollectArtifactsObjectiveData : ObjectiveSO
{
    public int requiredArtifactAmount;
    public override Objective CreateObjective()
    {
        return new CollectArtifactsObjective(this);
    }
}
