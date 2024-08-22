using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGameButtons : MonoBehaviour
{
    bool isBuild;
    bool isExpand;
    bool isTerrain;
    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonExpand()
    {
        if (!isExpand)
        {
            GameEvents.current.ButtonExpandEvent();
            isBuild = false;
            isTerrain = false;
            isExpand = true;
        }

    }
    public void ButtonBuild()
    {
        if (!isBuild)
        {
            GameEvents.current.ButtonBuildEvent();
            isBuild = true;
            isTerrain = false;
            isExpand = false;
        }
    }
    public void ButtonTerrain()
    {
        if (!isTerrain)
        {
            GameEvents.current.ButtonTerrainEvent();
            isBuild = false;
            isTerrain = true;
            isExpand = false;
            gameManager._gameState = gameState.terrain;
        }
    }
}
