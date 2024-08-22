using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Linq;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class RoadBuilder : MonoBehaviour
{
    List<Vector3> verticesP1;
    List<Vector3> verticesP2;
    List<Vector3> upVectors;
    public List<Vector3> centerVectors;


    public float roadWidth;
    float anteriorWidth = -1;
    public Vector3 startPos;
    public Vector3 endPos;

    [SerializeField] Material mat;
    [SerializeField] float distanceIntersection;
    [SerializeField] GameObject decalCrosswalk;
    List<GameObject> crosswalks = new List<GameObject>();

    int numInterseccion;
    [SerializeField] SplineContainer splineContainer;
    public AgentParameters agentParameter;
    AgentParameters anteriorAgentParameter;
    public string tipo;

    public int endIndexIntersection;
    public int startIndexIntersection;
    int anteriorendIndexIntersection;
    int anteriorstartIndexIntersection;
    [SerializeField] bool showRoad;

    [Range(1f, 50.0f)]
    public float sliderResolution;
    float anteriorSliderResolution = 0;
    List<int> resolutions;

    public float uvTiling = 300;
    float anteriorUvTiling = -1;

    public float uvTilingWidth = 1;
    float anteriorUvTilingWidth = -1;
    [SerializeField] LineRenderer lineRenderer;

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = new Mesh();
        GetVertices();
    }
    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }
    private void OnSplineChanged(Spline arg1, int arg2, SplineModification arg3)
    {
        for (int i = 0; i < splineContainer.Splines.Count; i++)
        {
            if (arg1 == splineContainer.Splines[i])
            {
                GetVertices();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(anteriorSliderResolution != sliderResolution || uvTiling != anteriorUvTiling || uvTilingWidth != anteriorUvTilingWidth || anteriorWidth != roadWidth)
        {
            GetVertices();
            anteriorSliderResolution = sliderResolution;
            anteriorUvTiling = uvTiling;
            anteriorUvTilingWidth = uvTilingWidth;
            anteriorWidth = roadWidth;
        }
        if(endIndexIntersection != anteriorendIndexIntersection && agentParameter != null)
        {
            agentParameter.endIndexIntersection = endIndexIntersection;
            anteriorendIndexIntersection = endIndexIntersection;
        }
        if (startIndexIntersection != anteriorstartIndexIntersection && agentParameter != null)
        {
            agentParameter.startIndexIntersection = startIndexIntersection;           
            anteriorstartIndexIntersection = startIndexIntersection;
        }
        if(anteriorAgentParameter != agentParameter)
        {
            anteriorAgentParameter = agentParameter;
            tipo = agentParameter.type;

            startIndexIntersection = agentParameter.startIndexIntersection;
            anteriorstartIndexIntersection = startIndexIntersection;

            endIndexIntersection = agentParameter.endIndexIntersection;
            anteriorendIndexIntersection = endIndexIntersection;
            roadWidth = agentParameter.width;
        }
    }
    private void GetVertices()
    {
        if(splineContainer.Spline.ToArray().Length == 0)
        {
            Destroy(gameObject);
        }
        if (agentParameter == null)
        {
            agentParameter = new AgentParameters();
        }
        verticesP1 = new List<Vector3>();
        verticesP2 = new List<Vector3>();
        centerVectors = new List<Vector3>();
        upVectors = new List<Vector3>();
        resolutions = new List<int>();

        int numSplines = SplineRoadSample.NumSplines(splineContainer);

        Vector3 p1;
        Vector3 p2;

        Vector3 up;
        SplineRoadSample.SampleHighway(0, 0, out p1, splineContainer);
        startPos = p1;
        SplineRoadSample.SampleHighway(0, 1, out p1, splineContainer);
        endPos = p1;

        for (int j = 0; j < numSplines; j++)
        {
            int newResolution = 0;

            for (int i = 0; i < splineContainer.Splines[j].Knots.Count(); i++)
            {
                Vector3 start = splineContainer.Splines[j].Knots.ToArray()[i].Position;
                Vector3 end;
                if(i+1 >= splineContainer.Splines[j].Knots.Count())
                {
                    end = splineContainer.Splines[j].Knots.ToArray()[0].Position;
                }
                else
                {
                    end = splineContainer.Splines[j].Knots.ToArray()[i+1].Position;
                }
                newResolution += (int)Vector3.Distance(start, end);
            }
            newResolution = Mathf.RoundToInt(newResolution/ sliderResolution);
            
            float step = 1f / newResolution;
            resolutions.Add(newResolution);
            for (int i = 0; i < newResolution; i++)
            {
                float t = step * i;

                SplineRoadSample.SampleRoadWidth(j,t, out p1, out p2, out up, splineContainer,roadWidth);

                p1 = transform.InverseTransformPoint(p1);
                p2 = transform.InverseTransformPoint(p2);

                verticesP1.Add(p1);
                verticesP2.Add(p2);
                upVectors.Add(up);
                upVectors.Add(up);

                SplineRoadSample.SampleHighway(j, t, out Vector3 center, splineContainer);
                centerVectors.Add(center);
            }
            SplineRoadSample.SampleRoadWidth(j, 1f, out p1, out p2,out up, splineContainer, roadWidth);

            p1 = transform.InverseTransformPoint(p1);
            p2 = transform.InverseTransformPoint(p2);

            verticesP1.Add(p1);
            verticesP2.Add(p2);
            upVectors.Add(up);
            upVectors.Add(up);
        }
       
        if(agentParameter != null)
        {
            SplineRoadSample.SampleHighway(0, 0, out p1, splineContainer);
            SplineRoadSample.SampleHighway(0, 1, out p2, splineContainer);
            agentParameter.endPos = p2;
            agentParameter.startPos = p1;
            agentParameter.direction = (p2 - p1).normalized;
            agentParameter.invertedDir = (p1 - p2).normalized;
            agentParameter.lenght = Vector3.Distance(p1, p2);
            agentParameter.road = gameObject;
            agentParameter.splineContainer = splineContainer;
            agentParameter.width = roadWidth;
        }
        if (showRoad)
        {
            BuildRoad();
            DrawLines();
        }
    }
    void DrawLines()
    {
        if(lineRenderer!= null)
        {
            lineRenderer.positionCount = centerVectors.Count - 1;
            for (int i = 0; i < centerVectors.Count; i++)
            {
                lineRenderer.SetPosition(i, transform.InverseTransformPoint(centerVectors[i]));
            }
        }
    }

    private void BuildRoad()
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> trisRoad = new List<int>();
        List<int> trisIntersection = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        numInterseccion = -1;

        int offset = 0;
        float uvOffset = 0;

        int numSplines = SplineRoadSample.NumSplines(splineContainer);

        for (int i = 0; i < crosswalks.Count; i++)
        {
            DestroyImmediate(crosswalks[i]);
        }
        crosswalks.Clear();


        for (int j = 0; j < numSplines; j++)
        {
            int splineOffset = 0;
            int resolutionOffset = 0;
            for (int i = j-1; i >= 0; i--)
            {
                splineOffset += resolutions[i];
                resolutionOffset += resolutions[i];
            }
            splineOffset += j;
            for (int i = 1; i < resolutions[j]+1; i++)
            {
                int vertOffset = splineOffset + i;
                Vector3 p1 = verticesP1[vertOffset - 1];
                Vector3 p2 = verticesP2[vertOffset - 1];
                Vector3 p3 = verticesP1[vertOffset];
                Vector3 p4 = verticesP2[vertOffset];

                Vector3 u1 = upVectors[vertOffset - 1];
                Vector3 u2 = upVectors[vertOffset - 1];
                Vector3 u3 = upVectors[vertOffset];
                Vector3 u4 = upVectors[vertOffset];

                offset = 4 * resolutionOffset;
                offset += 4 * (i - 1);

                int t1 = offset + 0;
                int t2 = offset + 2;
                int t3 = offset + 3;
                int t4 = offset + 3;
                int t5 = offset + 1;
                int t6 = offset + 0;

                verts.AddRange(new List<Vector3> { p1, p2, p3, p4 });
                trisRoad.AddRange(new List<int> { t1, t2, t3, t4, t5, t6 });

                float distance = Vector3.Distance(p1, p3) / uvTiling;
                float uvDistance = uvOffset + distance;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset,  roadWidth / -uvTilingWidth), new Vector2(uvOffset, roadWidth / uvTilingWidth), new Vector2(uvDistance, roadWidth / -uvTilingWidth), new Vector2(uvDistance, roadWidth / uvTilingWidth) });
                normals.AddRange(new List<Vector3> { u1, u2, u3, u4 });
                uvOffset += distance;
            }
        }       

        MeshFilter mf = GetComponent<MeshFilter>();
       
        if(mf.sharedMesh == null)
        {
            mf.sharedMesh = new Mesh();
        }
        Mesh mesh = mf.sharedMesh;
        mesh.Clear();
        mesh.subMeshCount = 2;
        mesh.SetVertices(verts);
        mesh.SetTriangles(trisRoad,0);
        mesh.SetTriangles(trisIntersection, 1);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);


        MeshCollider collider;
        if (GetComponent<MeshCollider>() == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }
        else
        {
            collider = GetComponent<MeshCollider>();
        }
        try
        {
            collider.sharedMesh = mesh;
        }
        catch (System.Exception)
        {

            throw;
        }
        GetComponent<MeshRenderer>().material = mat;
    }      
}

