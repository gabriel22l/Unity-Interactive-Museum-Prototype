using UnityEngine;

public class CollectArtifactsObjective : Objective
{
    private InventoryController inventory;
    private int currentArtifactAmount;
    private CollectArtifactsObjectiveData data;
    public CollectArtifactsObjective(CollectArtifactsObjectiveData data)
    {
        this.data = data;
    }
    public override string Description => data.Description;
    public override void Initialize(ObjectiveContext context)
    {
        inventory = context.Inventory;
        inventory.OnInventoryChanged += UpdateObjective;
        UpdateObjective();
    }
    public override string GetProgressText()
    {
        return $"{currentArtifactAmount} / {data.requiredArtifactAmount}";
    }
    private void UpdateObjective()
    {
        if(inventory == null)
            return;
        int lastArtifactAmount = currentArtifactAmount;
        currentArtifactAmount = inventory.GetItemCountByType(ItemType.Artifact);
        if (currentArtifactAmount != lastArtifactAmount)
            NotifyProgressUpdated();
        if(currentArtifactAmount >=  data.requiredArtifactAmount)
            Complete();
    }
    public override void Cleanup()
    {
        if (inventory == null)
            return;
        inventory.OnInventoryChanged -=  UpdateObjective;
        inventory = null;
    }
}
