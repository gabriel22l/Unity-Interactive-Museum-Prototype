using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    private PlayerContext playerContext;
    private PlayerInput playerInput; //for subscribing to Interact event
    private InteractionContext interactionContext;
    
    private InteractableIndicatorController indicatorController;
    
    [SerializeField] private Transform rayOrigin;
    private readonly float  interactRange = 2f;
    private readonly float interactRadius = 1f;
    
    private readonly float detectionRange = 6f;
    private readonly float detectionRadius = 3f;
    
    private IInteractable currentInteractable;

    public void Initialize(PlayerContext playerContext, PlayerInput playerInput, InteractableIndicatorController indicatorController)
    {
        this.playerContext = playerContext;
        this.playerInput = playerInput;
        this.indicatorController = indicatorController;
        
        interactionContext = new InteractionContext
        {
            PlayerContext = this.playerContext,
        };
        playerInput.InteractEvent += OnInteract;
    }
    private void Awake()
    {
        if(rayOrigin == null) rayOrigin = transform;
    }
    private void OnDestroy()
    {
        playerInput.InteractEvent -= OnInteract;
    }
    private void Update()
    {
        if (indicatorController == null) return;
        
        GetInteractable();
        List<IInteractable> interactablesInRange = GetInteractablesInRange();
        indicatorController.UpdateIndicators(interactablesInRange, currentInteractable);
    }
    private void GetInteractable()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, interactRadius, interactRange);
    
        currentInteractable = null;
        float closestDistance = Mathf.Infinity;
    
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable) && interactable.IsInteractable)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    currentInteractable = interactable;
                }
            }
        }
    }
    private List<IInteractable> GetInteractablesInRange()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, detectionRadius, detectionRange);
        
        Dictionary<IInteractable, float> distances = new Dictionary<IInteractable, float>();
        
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out IInteractable interactable) || !interactable.IsInteractable) continue;
            
            if(!distances.TryGetValue(interactable, out float distance) || hit.distance < distance) 
                distances[interactable] = hit.distance;
            
        }
        List<IInteractable> sorted = new List<IInteractable>(distances.Keys);
        sorted.Sort((x, y) => distances[x].CompareTo(distances[y]));
        
        int maxCount = indicatorController.IndicatorCount;
        if (sorted.Count > maxCount)
        {
            sorted.RemoveRange(maxCount, sorted.Count - maxCount);
        }
        return sorted;
    }
    private void OnInteract()
    {
        currentInteractable?.Interact(interactionContext);
    }
}

public struct InteractionContext
{
    public PlayerContext PlayerContext;
}