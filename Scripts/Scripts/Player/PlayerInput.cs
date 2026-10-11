using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerInput : MonoBehaviour
{
    private PlayerContext playerContext;
    private UIController UIController => playerContext?.UIController;
    private PlayerViewController PlayerViewController => playerContext?.PlayerViewController;
    
    [FormerlySerializedAs("cinemachineInputAxisController")]
    [SerializeField] private CinemachineInputAxisController fpCmInputController;
    [SerializeField] private CinemachineInputAxisController tpCmInputController;
    private InputActions input;
    private Vector2 moveInput;
    private bool isRunning;
    public Vector2 MoveInput => moveInput;
    public bool IsRunning => isRunning;
    private InputActionMap currentActionMap;

    public event Action InteractEvent;
    public event Action OnPlayerMenuOpen;
    public event Action OnPauseMenuOpen;
    public event Action CloseUIPressed;

    public event Action OnThrowInput;

    public void Initialize(PlayerContext playerContext)
    {
        this.playerContext = playerContext;
        UIController.OnUIOpen += OnUIOpened;
        UIController.OnUIClose += OnUIClosed;
    }
    private void Awake()
    {
        input = new InputActions();
    }
    private void OnEnable()
    {
        input.Gameplay.Movement.performed += OnMove;
        input.Gameplay.Movement.canceled += OnMove;
        input.Gameplay.Run.performed += OnRun;
        input.Gameplay.Interact.performed += OnInteract;
        input.Gameplay.OpenPlayerMenu.performed += OpenPlayerMenu;
        input.Gameplay.OpenPauseMenu.performed += OpenPauseMenu;
        input.Gameplay.ToggleView.performed += ToggleView;
        input.Gameplay.Throw.performed += ThrowInput;
        
        input.UI.CloseUI.performed += CloseUIInput;

        SwitchActionMap(input.Gameplay);
    }
    private void OnDisable()
    {
        if (UIController != null)
        {
            UIController.OnUIClose -= OnUIClosed;
            UIController.OnUIOpen -= OnUIOpened;
        }

        input.Gameplay.Movement.performed -= OnMove;
        input.Gameplay.Movement.canceled -= OnMove;
        input.Gameplay.Run.performed -= OnRun;
        input.Gameplay.Interact.performed -= OnInteract;
        input.Gameplay.OpenPlayerMenu.performed -= OpenPlayerMenu;
        input.Gameplay.OpenPauseMenu.performed -= OpenPauseMenu;
        input.Gameplay.ToggleView.performed -= ToggleView;
        
        input.UI.CloseUI.performed -= CloseUIInput;
        
        input.UI.Disable();
        input.Gameplay.Disable();
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    private void OnRun(InputAction.CallbackContext ctx)
    {
        isRunning = !isRunning;
    }
    private void OnInteract(InputAction.CallbackContext ctx)
    {
        InteractEvent?.Invoke();
    }
    private void OpenPlayerMenu(InputAction.CallbackContext ctx)
    {
        OnPlayerMenuOpen?.Invoke();
    }
    private void OpenPauseMenu(InputAction.CallbackContext ctx)
    {
        OnPauseMenuOpen?.Invoke();
    }
    private void ToggleView(InputAction.CallbackContext ctx)
    {
        PlayerViewController?.ToggleView();
    }
    private void ThrowInput(InputAction.CallbackContext ctx)
    {
        OnThrowInput?.Invoke();
    }
    private void CloseUIInput(InputAction.CallbackContext ctx)
    {
        CloseUIPressed?.Invoke();
    }
    private void OnUIOpened()
    {
        SwitchActionMap(input.UI);
        if(fpCmInputController) fpCmInputController.enabled = false;
        if(tpCmInputController) tpCmInputController.enabled = false;
    }
    private void OnUIClosed()
    {
        SwitchActionMap(input.Gameplay);
        if(fpCmInputController) fpCmInputController.enabled = true;
        if(tpCmInputController) tpCmInputController.enabled = true;
    }

    private void SwitchActionMap(InputActionMap actionMap)
    {
        if (actionMap == currentActionMap || actionMap == null) return;
        
        currentActionMap?.Disable();
        actionMap.Enable();
        currentActionMap = actionMap;
    }
}