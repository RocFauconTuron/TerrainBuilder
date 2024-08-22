using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class AgentParameters
{
    public Vector3 startPos;
    public Vector3 endPos;
    public Vector3 direction;
    public float lenght;
    public GameObject road;
    public SplineContainer splineContainer;
    public float width;
    public string type;
    public int indexPoint;
    public Vector3 invertedDir;
    public float acumulativeDistance;
    public int startIndexIntersection;
    public int endIndexIntersection;
    public AgentParameters child;
}
