using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerViewController : MonoBehaviour
{
    private enum ViewState { FirstPerson, ThirdPerson, }
    private ViewState currentView;
    [SerializeField] private ViewState defaultView;
    
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private PlayerMovement playerMovementTp;
    [SerializeField] private PlayerMovementFP playerMovementFp;
    [SerializeField] private CinemachineCamera cmCameraFp;
    [SerializeField] private CinemachineCamera cmCameraTp;
    [SerializeField] private PlayerAnimation playerAnimation;
    
    //References to Cm components
    private CinemachinePanTilt cmPanTilt;
    private CinemachineOrbitalFollow cmOrbitalFollow;
    
    //default rotation values
    private float tpDefaultVerticalRotation = 20f;
    private float fpDefaultVerticalRotation = 0f;

    private void Awake()
    {
        if(!cmCameraFp.TryGetComponent(out cmPanTilt))
            Debug.LogError($"{nameof(cmPanTilt)} not found");
        if (!cmCameraTp.TryGetComponent(out cmOrbitalFollow))
            Debug.LogError($"{nameof(cmOrbitalFollow)} not found");
    }
    private void Start()
    {
        //force initial run
        currentView = defaultView;
        SetViewState(defaultView, true);
    }
    
    private void SetViewState(ViewState newView, bool forceRun = false)
    {
        //Set camera and player y rotation to current y rotation when switching viewState, apply view 
        if (!forceRun && newView == currentView)
            return;

        bool switchingToTp = newView == ViewState.ThirdPerson &&
                             currentView == ViewState.FirstPerson && 
                             !forceRun;
        bool switchingToFp = newView == ViewState.FirstPerson && 
                             currentView == ViewState.ThirdPerson && 
                             !forceRun;
        
        if (switchingToTp)
        {
            cmOrbitalFollow.HorizontalAxis.Value = cmPanTilt.PanAxis.Value;
            cmOrbitalFollow.VerticalAxis.Value = tpDefaultVerticalRotation;
            playerMovementTp.SetVelocity(playerMovementFp.CurrentVelocityVector, playerMovementFp.CurrentVerticalVelocity);
        }
        else if (switchingToFp)
        {
            cmPanTilt.PanAxis.Value = cmOrbitalFollow.HorizontalAxis.Value;
            cmPanTilt.TiltAxis.Value = fpDefaultVerticalRotation;
            playerMovementFp.SetVelocity(playerMovementTp.CurrentVelocityVector, playerMovementTp.CurrentVerticalVelocity);
        }

        currentView = newView;
        
        ApplyView();
    }
    private void ApplyView()
    {
        //toggle SkinnedMeshRenderer, TP/FP movement script, TP/FP camera priority value, align camera and player object rotation
        bool thirdPerson = currentView == ViewState.ThirdPerson;
        bool firstPerson = currentView == ViewState.FirstPerson;

        meshRenderer.shadowCastingMode = thirdPerson ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
        
        playerMovementTp.enabled = thirdPerson;
        playerMovementFp.enabled = firstPerson;
        
        playerAnimation.SetMovementData(thirdPerson ?  playerMovementTp : playerMovementFp);
        
        cmCameraTp.Priority.Value = thirdPerson ? 10: 0;
        cmCameraFp.Priority.Value = firstPerson ? 10 : 0;
    }
    public void ToggleView()
    {
        SetViewState(GetNextViewState());
    }
    private ViewState GetNextViewState()
    {
        int stateLength = Enum.GetValues(typeof(ViewState)).Length;
        int nextIndex = ((int)currentView + 1) % stateLength;
        return (ViewState)nextIndex;
    }
}