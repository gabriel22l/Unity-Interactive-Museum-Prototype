using UnityEngine;

public class InteractableInfoPanel : MonoBehaviour, IInteractable
{
    [SerializeField] private itemDataSO itemData;
    public bool IsInteractable => true;

    public void Interact(InteractionContext interactionContext)
    {
        if (itemData == null)
        {
            Debug.LogError($"No item data found in {gameObject.name}");
            return;
        }
        
        UIController uiController = interactionContext.PlayerContext.UIController;
        uiController.OpenInfoPanelMenu(itemData);
    }
}