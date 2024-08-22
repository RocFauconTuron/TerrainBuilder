using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] GameObject newCubePrefab;
    [SerializeField] Camera camera;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] Material cubeMat;
    List<CubeInfo> cubesList;
    public TerrainData terrainData;
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        cubesList = transform.GetComponentsInChildren<CubeInfo>().ToList();
        for (int i = 0; i < cubesList.Count; i++)
        {
            cubesList[i].InitializeList();
        }
    }
    private void OnEnable()
    {
        GameEvents.current.onButtonExpandEvent += ExpandMapMode;
    }
    private void OnDisable()
    {
        GameEvents.current.onButtonExpandEvent -= ExpandMapMode;
    }
    private void Update()
    {
        if (gameManager._gameState == gameState.expand)
        {
            if(Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftAlt))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray mRay = camera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(mRay, out hit, Mathf.Infinity, _layerMask))
                {
                    hit.transform.gameObject.AddComponent<CubeInfo>();
                    CubeInfo hitCubeInfo = hit.transform.GetComponent<CubeInfo>();
                    hitCubeInfo.enabled = true;
                    hitCubeInfo.InitializeList();
                    hit.transform.gameObject.layer = 3;
                    cubesList = transform.GetComponentsInChildren<CubeInfo>().ToList();

                    for (int i = 0; i < cubesList.Count; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if(cubesList[i].neightborList[j].indexX == hitCubeInfo.indexXCube && cubesList[i].neightborList[j].indexZ == hitCubeInfo.indexZCube)
                            {
                                if(cubesList[i].neightborList[j].cubeNeightbor != null)
                                {
                                    if (cubesList[i].neightborList[j].cubeNeightbor.GetComponent<CubeInfo>() == null)
                                    {
                                        Destroy(cubesList[i].neightborList[j].cubeNeightbor);
                                    }
                                }                             
                                cubesList[i].neightborList[j] = new CubeInfo.neightborCubeInfo { neightborPos = cubesList[i].neightborList[j].neightborPos, created = true,indexX = cubesList[i].neightborList[j].indexX, indexZ = cubesList[i].neightborList[j].indexZ,cubeNeightbor = null };                               
                            }
                        }
                    }

                    StartCoroutine(FadeOutCube(hit.transform.GetComponent<CubeInfo>()));
                    hit.transform.GetComponent<MeshRenderer>().material = cubeMat;
                    hit.transform.GetComponent<Animator>().SetTrigger("appear");
                    StartCoroutine(FadeInNewCubes(hit.transform.GetComponent<CubeInfo>()));
                }
            }          
        }
    }
    void ExpandMapMode()
    {
        cubesList = transform.GetComponentsInChildren<CubeInfo>().ToList();
        for (int i = 0; i < cubesList.Count; i++)
        {
            StartCoroutine(FadeInNewCubes(cubesList[i]));
        }
    }
    IEnumerator FadeInNewCubes(CubeInfo cube)
    {
        yield return new WaitForSeconds(1f);
        List<LineRenderer> lines = new List<LineRenderer>();
        for (int i = 0; i < 4; i++)
        {
            if (!cube.neightborList[i].created)
            {
                GameObject newCube = GameObject.Instantiate(newCubePrefab, cube.neightborList[i].neightborPos, Quaternion.identity);
                newCube.transform.parent = transform;
                cube.neightborList[i] = new CubeInfo.neightborCubeInfo { neightborPos = cube.neightborList[i].neightborPos, created = false, indexX = cube.neightborList[i].indexX, indexZ = cube.neightborList[i].indexZ,cubeNeightbor = newCube }; ;
                LineRenderer[] linesNew = newCube.GetComponentsInChildren<LineRenderer>();
                for (int j = 0; j < linesNew.Length; j++)
                {
                    lines.Add(linesNew[j]);
                }                
            }          
        }

        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(-0.06f, 0.03f, elapsedTime / 1f);
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].material.SetFloat("_heightRectangle",alpha);
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator FadeOutNewCubes(CubeInfo cube)
    {
        List<LineRenderer> lines = new List<LineRenderer>();
        for (int i = 0; i < 4; i++)
        {
            if (!cube.neightborList[i].created)
            {
                GameObject newCube = GameObject.Instantiate(newCubePrefab, cube.neightborList[i].neightborPos, Quaternion.identity);
                LineRenderer[] linesNew = newCube.GetComponentsInChildren<LineRenderer>();
                for (int j = 0; j < linesNew.Length; j++)
                {
                    lines.Add(linesNew[j]);
                }
            }
        }

        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.03f, -0.06f, elapsedTime / 1f);
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].material.SetFloat("_heightRectangle", alpha);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeOutCube(CubeInfo cube)
    {
        yield return new WaitForSeconds(1f);
        List<LineRenderer> lines = cube.GetComponentsInChildren<LineRenderer>().ToList();
        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.03f, -0.06f, elapsedTime / 1f);
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].material.SetFloat("_heightRectangle", alpha);
            }

            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < lines.Count; i++)
        {
            Destroy(lines[i].gameObject);
        }
        yield return new WaitForEndOfFrame();
        Terrain terrain = cube.transform.GetChild(0).GetComponent<Terrain>();
        terrain.enabled = true;
        terrain.terrainData = Instantiate(terrainData);
        cube.transform.GetChild(0).GetComponent<TerrainCollider>().enabled = true;
        cube.transform.GetChild(0).GetComponent<TerrainCollider>().terrainData = cube.transform.GetChild(0).GetComponent<Terrain>().terrainData;
        CubeInfo info = cube.transform.GetComponent<CubeInfo>();
        Terrain left = null;
        if (info.neightBoorLeft!= null)
        {
            left= info.neightBoorLeft.GetComponentInChildren<Terrain>();
        }
        Terrain right = null;
        if (info.neightBoorRight != null)
        {
            right = info.neightBoorRight.GetComponentInChildren<Terrain>();
        }
        Terrain up = null;
        if (info.neightBoorUp != null)
        {
            up = info.neightBoorUp.GetComponentInChildren<Terrain>();
        }
        Terrain down = null;
        if (info.neightBoorDown != null)
        {
            down = info.neightBoorDown.GetComponentInChildren<Terrain>();
        }
        terrain.SetNeighbors(left, up, right, down);
    }
    //void RaiseTerrain(float[,] heightMap, int radius, int centreX, int centreY, float tergetHeight,Terrain terrain)
    //{
    //    Vector3 terrainScale = terrain.transform.lossyScale;
    //    float deltaHeight = tergetHeight;
    //    bool isTerrainLower;
    //    if (deltaHeight > heightMap[centreX, centreY])
    //    {
    //        isTerrainLower = true;
    //    }
    //    else
    //    {
    //        isTerrainLower = false;
    //        deltaHeight = Mathf.Abs(deltaHeight);
    //    }
    //    int sqrRadius = radius * radius;
    //    // Loop over brush bounding box 
    //    for (int offsetY = -radius; offsetY <= radius; offsetY++)
    //    {
    //        for (int offsetX = -radius; offsetX <= radius; offsetX++)
    //        {
    //            int sqrDstFromCentre = offsetX * offsetX + offsetY * offsetY;
    //            // Check i f point is inside brush radius 
    //            if (sqrDstFromCentre <= sqrRadius)
    //            {
    //                // Cal culate brush weight with exponential falloff from centre 
    //                float dstFromCentre = Mathf.Sqrt(sqrDstFromCentre);
    //                float t = dstFromCentre / radius;
    //                float brushWeight = Mathf.Exp(-t * t / brushFallOff);
    //                brushWeight = Mathf.Exp(-t * blendCurve.Evaluate(t));

    //                // Rai se terrain 
    //                int brushX = centreX + offsetX;
    //                int brushY = centreY + offsetY;
    //                //for (int i = 0; i < vertices.Length - 2; i++)
    //                //{
    //                //	if (IsPointInTriangle(new Vector2(brushX, brushY), new Vector2(vertices[i].x, vertices[i].z), new Vector2(vertices[i + 1].x, vertices[i + 1].z), new Vector2(vertices[i + 2].x, vertices[i + 2].z)))
    //                //	{
    //                //		// Get the corresponding height on the terrain
    //                //		float terrainY = vertices[i].y / terrainHeight;
    //                //		heightsEdit[brushX, brushY] = deltaHeight * brushWeight - 0.001f;
    //                //	}
    //                //}

    //                if (deltaHeight * brushWeight - 0.001f > heightsEdit[brushX, brushY] && isTerrainLower && insideObject[(int)brushX, (int)brushY] == false)
    //                {
    //                    heightsEdit[brushX, brushY] = deltaHeight * brushWeight - 0.001f;

    //                    RaycastHit hit;
    //                    Vector3 terrainSizeWorld = new Vector3(brushY * terrainScale.x * stepX, 2000, brushX * terrainScale.z * stepY);
    //                    // Does the ray intersect any objects excluding the player layer
    //                    if (Physics.Raycast(terrainSizeWorld, Vector3.down, out hit, 3000, layerMask))
    //                    {
    //                        heightsEdit[(int)brushX, (int)brushY] = (hit.point.y / terrainHeight) - 0.001f;
    //                        insideObject[(int)brushX, (int)brushY] = true;
    //                    }

    //                }
    //                else if (deltaHeight / brushWeight - 0.001f < heightsEdit[brushX, brushY] && !isTerrainLower)
    //                {
    //                    heightsEdit[brushX, brushY] = deltaHeight / brushWeight - 0.001f;
    //                }

    //            }
    //        }
    //    }
    //}

}
