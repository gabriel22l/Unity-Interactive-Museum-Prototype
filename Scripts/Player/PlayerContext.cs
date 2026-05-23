using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    //PlayerContext class provides references to external systems to player systems
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private PlayerViewController playerViewController;
    [SerializeField] private Health health;
    [SerializeField] private AudioEmitter audioEmitter;
    
    public UIController UIController { get; private set; } //UIController wired to this player instance
    public PlayerInput PlayerInput => playerInput;
    public InventoryController InventoryController => inventoryController;
    public PlayerInteraction PlayerInteraction => playerInteraction;
    public PlayerViewController PlayerViewController => playerViewController;
    public Health Health => health;
    public AudioEmitter AudioEmitter => audioEmitter;
    
    public void Initialize(UIController uiController, InteractableIndicatorController interactableIndicatorController)
    {
        this.UIController = uiController;
        PlayerInput.Initialize(this);
        playerInteraction.Initialize(this, PlayerInput,  interactableIndicatorController);
    }
}