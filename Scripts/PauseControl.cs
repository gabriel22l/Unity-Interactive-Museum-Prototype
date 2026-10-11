using UnityEngine;
using System;
public class PauseControl: IPauseControl
{
    private bool allowPause = true;
    public PauseControl(bool allowPause)
    {
        this.allowPause = allowPause;
    }
    public void RequestPause(bool pause)
    {
        if(pause && !allowPause)
            return;
        Time.timeScale = pause ? 0 : 1;
    }
}