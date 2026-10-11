using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
public static class FixOpenXrDpad
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        InputSystem.onAfterUpdate += CheckDpadLayout;
    }

    private static void CheckDpadLayout()
    {
        if (InputSystem.GetNameOfBaseLayout("DPad") != "XRController")
            return;

        FixDpadLayout();
    }

    private static void FixDpadLayout()
    {
        Debug.Log("[Input System Fix] Restoring DPad layout.");

        List<InputDevice> devices = new();

        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.added)
                devices.Add(device);
        }

        InputSystem.RemoveLayout("DPad");
        InputSystem.RegisterLayout<DpadControl>("DPad");

        foreach (InputDevice device in devices)
        {
            if (!device.added)
            {
                InputSystem.AddDevice(device);
                InputSystem.ResetDevice(device);
                device.MakeCurrent();
            }
        }
    }
}
#endif