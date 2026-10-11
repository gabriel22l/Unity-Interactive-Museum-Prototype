using System;
using UnityEngine;

public interface IUIPageManager
{
    event Action OnPageChanged;
    event Action OnUIClosed;
    GameObject CurrentPage { get; }
}
