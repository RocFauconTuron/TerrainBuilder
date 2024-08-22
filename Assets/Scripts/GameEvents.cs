using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;
    private void Awake()
    {
        current = this;
    }
    public event Action onButtonExpandEvent;
    public void ButtonExpandEvent()
    {
        if(onButtonExpandEvent != null)
        {
            onButtonExpandEvent();
        }
    }
    public event Action onButtonBuildEvent;
    public void ButtonBuildEvent()
    {
        if (onButtonBuildEvent != null)
        {
            onButtonBuildEvent();
        }
    }
    public event Action onButtonTerrainEvent;
    public void ButtonTerrainEvent()
    {
        if (onButtonTerrainEvent != null)
        {
            onButtonTerrainEvent();
        }
    }

}
