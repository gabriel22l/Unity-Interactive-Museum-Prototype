using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ApplicationController
{
    #region Resolution
    public static List<Resolution> GetAvailableResolutions()
    {
        HashSet<string> seen = new HashSet<string>();
        List<Resolution> result = new List<Resolution>();

        Resolution[] allRes = Screen.resolutions;
        Array.Reverse(allRes);

        DisplayInfo displayInfo = Screen.mainWindowDisplayInfo;
        float nativeAspect = (float)displayInfo.width / displayInfo.height;
        const float aspectTolerance = 0.02f;

        foreach (Resolution r in allRes)
        {
            float aspect = (float)r.width / r.height;
            if (Mathf.Abs(aspect - nativeAspect) > aspectTolerance) continue;

            string key = $"{r.width}x{r.height}";
            if (!seen.Add(key)) continue;

            result.Add(r);
        }

        return result;
    }

    public static void SetResolution(Resolution resolution)
    {
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
    }

    public static Resolution GetNativeResolution()
    {
        DisplayInfo displayInfo = Screen.mainWindowDisplayInfo;
        return new Resolution
        {
            width = displayInfo.width,
            height = displayInfo.height,
            refreshRateRatio = displayInfo.refreshRate
        };
    }
    public static Resolution GetCurrentResolution()
    {
        return new Resolution
        {
            width = Screen.width,
            height = Screen.height,
            refreshRateRatio = Screen.currentResolution.refreshRateRatio
        };
    }
    #endregion

    #region Scene Management
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    #endregion
    
    public static void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}
