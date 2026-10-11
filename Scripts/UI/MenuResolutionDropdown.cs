using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class MenuResolutionDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    private List<Resolution> resolutions;
    private List<string> options = new List<string>();

    private void Awake()
    {
        resolutions = ApplicationController.GetAvailableResolutions();
        foreach (Resolution r in resolutions)
        {
            options.Add($"{r.width}x{r.height}");
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        Resolution current = ApplicationController.GetCurrentResolution();
        int index = options.FindIndex(0, s => s == $"{current.width}x{current.height}");
        dropdown.value = index >= 0 ? index : 0;
    }

    private void OnEnable()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDisable()
    {
        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int value)
    {
        ApplicationController.SetResolution(resolutions[value]);
    }
}