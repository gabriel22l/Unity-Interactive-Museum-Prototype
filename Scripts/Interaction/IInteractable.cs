using UnityEngine;

public interface IInteractable
{
    public void Interact(InteractionContext interactionContext);
    public bool IsInteractable { get; }
}
