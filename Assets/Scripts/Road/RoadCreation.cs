using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RoadCreation : MonoBehaviour
{
    [SerializeField] Camera camera;
    [SerializeField] LayerMask _layerMask;
    bool firstClick = true;
    [SerializeField] GameObject roadPrefab;
    GameObject road;
    int knotIndex;
    List<Spline> splinesList;
    List<Transform> roadsList;
    [SerializeField] float snapDistance;
    Vector3 hitPos;
    bool isSnaping = false;
    GameManager gameManager;
    Vector3 tangentIn;
    Vector3 tangentOut;
    Vector3 showPos;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        splinesList = new List<Spline>();
        roadsList = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager._gameState == gameState.building)
        {
            if (Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.LeftAlt))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray mRay = camera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(mRay, out hit, Mathf.Infinity, _layerMask))
                {
                    if (!isSnaping)
                    {
                        hitPos = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                    }
                    if (firstClick)
                    {
                        hitPos = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                        road = GameObject.Instantiate(roadPrefab, hitPos, Quaternion.identity);
                        firstClick = false;
                        splinesList.Add(new Spline());
                        roadsList.Add(road.transform);

                        knotIndex = 1;
                        AddRoad(knotIndex, hitPos);
                    }
                    else
                    {
                        for (int i = 0; i < splinesList.Count; i++)
                        {
                            for (int j = 0; j < splinesList[i].ToArray().Length - 1; j++)
                            {
                                Vector3 pos = roadsList[i].TransformPoint(splinesList[i].ToArray()[j].Position);
                                if (hitPos == pos)
                                {
                                    firstClick = true;
                                    if (hitPos == roadsList[roadsList.Count - 1].TransformPoint(splinesList[splinesList.Count - 1].ToArray()[0].Position))
                                    {
                                        splinesList[splinesList.Count - 1].Closed = true;
                                        //splinesList[splinesList.Count - 1].RemoveAt[]
                                    }
                                    break;
                                }
                            }
                        }

                        if (!firstClick)
                        {
                            knotIndex++;
                            AddRoad(knotIndex, hitPos);
                        }

                    }
                }
            }
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray mRay = camera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(mRay, out hit, Mathf.Infinity, _layerMask))
                {
                    Vector3 hitPos = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                    Vector3 roadEnPos = road.transform.InverseTransformPoint(hitPos);
                    SplineContainer sc = road.GetComponent<SplineContainer>();
                    tangentIn = -((float3)roadEnPos - sc.Spline.ToArray()[knotIndex].Position);
                    tangentOut = ((float3)roadEnPos - sc.Spline.ToArray()[knotIndex].Position);
                    ShowRoad(knotIndex, showPos);
                }

            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                firstClick = true;
                splinesList[splinesList.Count - 1].RemoveAt(knotIndex);
                knotIndex--;
            }
            else if (!firstClick)
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray mRay = camera.ScreenPointToRay(mousePosition);
                RaycastHit hit;
                isSnaping = false;
                if (Physics.Raycast(mRay, out hit, Mathf.Infinity, _layerMask))
                {
                    showPos = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                    if (Input.GetKey(KeyCode.V))
                    {
                        for (int i = 0; i < splinesList.Count; i++)
                        {
                            for (int j = 0; j < splinesList[i].ToArray().Length - 1; j++)
                            {
                                isSnaping = true;
                                Vector3 otherPos = roadsList[i].TransformPoint(splinesList[i].ToArray()[j].Position);
                                if (Vector3.Distance(showPos, otherPos) < snapDistance)
                                {
                                    showPos = otherPos;
                                }
                            }
                        }
                    }
                    ShowRoad(knotIndex, showPos);
                }
            }
        }
        
    }
    void AddRoad(int knotIndex,Vector3 hitPos)
    {
        Vector3 roadEnPos = road.transform.InverseTransformPoint(hitPos);
        SplineContainer sc = road.GetComponent<SplineContainer>();
        sc.Spline.SetTangentMode(TangentMode.Mirrored);
        BezierKnot knot0 = sc.Spline.ToArray()[0];

        knot0.Position = roadEnPos;
        sc.Spline.Add(knot0);
        splinesList[splinesList.Count - 1] = sc.Spline;
    }
    void ShowRoad(int knotIndex, Vector3 hitPos)
    {
        Vector3 roadEnPos = road.transform.InverseTransformPoint(hitPos);
        SplineContainer sc = road.GetComponent<SplineContainer>();
        BezierKnot knot0 = sc.Spline.ToArray()[0];
        knot0.Position = roadEnPos;
        knot0.TangentIn = tangentIn;
        knot0.TangentOut = tangentOut;
        sc.Spline.SetKnot(knotIndex, knot0);
    }
}
