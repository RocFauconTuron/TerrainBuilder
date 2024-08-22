using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEditor;

[ExecuteInEditMode]
public static class SplineRoadSample
{   
    public static int NumSplines(SplineContainer splineContainer)
    {
        return splineContainer.Splines.Count;
    }

    public static void SampleRoadWidth(int splineIndex, float t, out Vector3 p1, out Vector3 p2, out Vector3 _upVector, SplineContainer splineContainer, float roadWidth)
    {
        float3 pos;
        float3 upVector;
        float3 forward;

        splineContainer.Evaluate(splineIndex,t, out pos, out forward, out upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = pos + (right * roadWidth);
        p2 = pos + (-right * roadWidth);
        _upVector = upVector;
    }
    public static void SampleRoadWidthForward(int splineIndex, float t, out Vector3 p1, out Vector3 p2, out Vector3 _upVector, out Vector3 _forward,SplineContainer splineContainer, float roadWidth)
    {
        float3 pos;
        float3 upVector;
        float3 forward;

        splineContainer.Evaluate(splineIndex, t, out pos, out forward, out upVector);

        float3 right = Vector3.Cross(forward, upVector).normalized;
        p1 = pos + (right * roadWidth);
        p2 = pos + (-right * roadWidth);
        _upVector = upVector;
        _forward = forward;
    }
    public static void SampleHighway(int splineIndex, float t, out Vector3 p1, SplineContainer splineContainer)
    {
        float3 pos;
        float3 upVector;
        float3 forward;

        splineContainer.Evaluate(splineIndex, t, out pos, out forward, out upVector);
        p1 = pos;
    }
    public static Vector3 GetForwardKnot(int splineIndex, float t, SplineContainer splineContainer)
    {
        float3 pos;
        float3 upVector;
        float3 forward;

        splineContainer.Evaluate(splineIndex, t, out pos, out forward, out upVector);
        return forward;
    }
    public static bool ProvaLineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        intersection = Vector3.zero;
        // Calculate the determinants
        float det = lineVec1.x * lineVec2.z - lineVec1.z * lineVec2.x;
        if (Mathf.Approximately(det, 0f))
        {
            // The vectors are parallel, so no intersection
            return false;
        }

        // Calculate the parameters for the intersection point
        float t = ((linePoint2.x - linePoint1.x) * lineVec2.z - (linePoint2.z - linePoint1.z) * lineVec2.x) / det;
        float u = ((linePoint2.x - linePoint1.x) * lineVec1.z - (linePoint2.z - linePoint1.z) * lineVec1.x) / det;

        // Check if the intersection point is within the line segments
        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            // Calculate the intersection point
            intersection = linePoint1 + t * lineVec1;
            return true;
        }

        // The intersection point is outside the line segments
        return false;
    }

}
