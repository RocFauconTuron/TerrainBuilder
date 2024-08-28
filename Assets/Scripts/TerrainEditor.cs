using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;

public class TerrainEditor : MonoBehaviour
{
    int brushSize = 5;
    float strength = 0.1f;
    [SerializeField] AnimationCurve blendCurve;
    [SerializeField] float brushFallOff;

    [SerializeField] LayerMask layerMask;
    [SerializeField] Camera _camera;
    bool scaleUp;

    int width;
    int height;
    Terrain terrain;
    float terrainWidth;
    float terrainDepth;
    float terrainHeight;
    int terrainX;
    int terrainZ;
    float terrainY;
    float[,] heights;
    int startZ;
    int endZ;
    int startX;
    int endX;

    [SerializeField] MenuTerrain menuTerrainInfo;
    int mode;
    bool isDecalActive = false;
    [SerializeField] float decalMultiplierSize;
    [SerializeField] Transform decalPaint;
    DecalProjector decal;
    int textureIndex;
    Vector3 point;
    GameManager gameManager;
    public int treesPerBrush;
    public int treeIndex;

    float flat;
    public int flatForce;

    public int grassDensity;

    [SerializeField] int minHeight;
    float minHeightController;
    [SerializeField] float strengthMutliplierSmooth;
    bool canEditTerrain;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        GameEvents.current.onButtonTerrainEvent += StartTerrainMode;
        decal = decalPaint.GetComponent<DecalProjector>();
    }
    private void OnDisable()
    {
        GameEvents.current.onButtonTerrainEvent -= StartTerrainMode;
    }
    void StartTerrainMode()
    {
        menuTerrainInfo.GetComponent<Animator>().SetBool("terrainMode", true);
    }
    void Update()
    {
        if(!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            canEditTerrain = true;
        }
        else if(EventSystem.current.IsPointerOverGameObject())
        {
            canEditTerrain = false;
        }
        if (gameManager._gameState == gameState.terrain && canEditTerrain )
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetMouseButton(2))
            {
                decalPaint.gameObject.SetActive(false);
                return;
            }
            if (GetInfoTerrain() && Input.GetMouseButton(0))
            {
                switch (mode)
                {
                    case 0:
                        scaleUp = true;
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            scaleUp = false;
                        }
                        RaiseTerrain();
                        AutoPaintTerrain();
                        break;
                    case 1:
                        SmoothTerrain();
                        AutoPaintTerrain();
                        break;
                    case 2:
                        PaintTerrain();
                        break;
                    case 3:
                        PaintTrees();
                        break;
                    case 4:
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            PaintGrass(true);
                        }
                        else
                        {
                            PaintGrass(false);
                        }
                        break;
                    case 5:
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            GetHeightPointForFlat();
                        }
                        else
                        {
                            FlatTerrain();
                        }
                        break;
                    default:
                        break;
                }
            }
            //if (Input.GetMouseButtonUp(0) && (mode == 0||mode == 1))
            //{
            //    AutoPaintTerrain();
            //}

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                decalPaint.gameObject.SetActive(true);
                if (!isDecalActive)
                {

                    isDecalActive = true;
                }
                brushSize = menuTerrainInfo.GetSize();
                decal.size = new Vector3(brushSize* decalMultiplierSize, brushSize* decalMultiplierSize, 500);
                decalPaint.transform.position = new Vector3(hit.point.x, 475, hit.point.z);
            }
            else
            {
                if (isDecalActive)
                {
                    decalPaint.gameObject.SetActive(false);
                    isDecalActive = false;
                }
            }
        }
        
    }
    Vector3 worldPos;
    void AutoPaintTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

        // Obtener coordenadas del terreno donde el pincel toca
        float multiplier = terrainData.alphamapWidth / width + 1;
        int brushPaint = Mathf.RoundToInt(brushSize * multiplier);
        int sqrRadiusPaint = brushPaint * brushPaint;

        int terrainXPaint = (int)(((point.x - terrain.transform.position.x) / terrainWidth) * terrainData.alphamapWidth);
        int terrainZPaint = (int)(((point.z - terrain.transform.position.z) / terrainDepth) * terrainData.alphamapHeight);

        // Obtener la porción del mapa de alfa afectada por el pincel
        int startXPaint = Mathf.Clamp(terrainXPaint - brushPaint, 0, terrainData.alphamapWidth - 1);
        int startZPaint = Mathf.Clamp(terrainZPaint - brushPaint, 0, terrainData.alphamapHeight - 1);
        int endXPaint = Mathf.Clamp(terrainXPaint + brushPaint, 0, terrainData.alphamapWidth - 1);
        int endZPaint = Mathf.Clamp(terrainZPaint + brushPaint, 0, terrainData.alphamapHeight - 1);

        float[,,] alphaMap = terrainData.GetAlphamaps(startXPaint, startZPaint, endXPaint - startXPaint, endZPaint - startZPaint);

        int localTerrainX = terrainXPaint - startXPaint;
        int localTerrainZ = terrainZPaint - startZPaint;

        for (int i = 0; i < endZPaint - startZPaint; i++)
        {
            for (int j = 0; j < endXPaint - startXPaint; j++)
            {
                int sqrDstFromCentre = (j - localTerrainX) * (j - localTerrainX) + (i - localTerrainZ) * (i - localTerrainZ);

                if (sqrDstFromCentre <= sqrRadiusPaint)
                {
                    // Convertir las coordenadas de alphamap a mundo
                    float worldX = (startXPaint + j) / (float)terrainData.alphamapWidth * terrainData.size.x + terrain.transform.position.x;
                    float worldZ = (startZPaint + i) / (float)terrainData.alphamapHeight * terrainData.size.z + terrain.transform.position.z;
                    worldPos = new Vector3(worldX, 500f, worldZ); // Posición inicial de raycast (por encima del terreno)

                    // Lanzar raycast hacia abajo para obtener la normal del terreno
                    RaycastHit hit;
                    if (Physics.Raycast(worldPos, Vector3.down, out hit, 1000f, layerMask))
                    {
                        Vector3 normal = hit.normal;

                        // Vector hacia arriba (suelo)
                        Vector3 groundNormal = Vector3.up;
                        float dotProduct = Vector3.Dot(normal, groundNormal);

                        Debug.Log($"Dot Product: {dotProduct}");
                        Debug.Log("Pendiente mayor que 45 grados, aplicando terreno rocoso.");
                        alphaMap[i, j, 1] = 1 / dotProduct - 1;  // Asegúrate de aplicar correctamente el valor de alpha

                        //// Pintar si la pendiente es mayor a 45 grados
                        //if (dotProduct < 0.5)  // 0.7 es el coseno de 45 grados
                        //{
                        //    Debug.Log("Pendiente mayor que 45 grados, aplicando terreno rocoso.");
                        //    alphaMap[i, j, 1] = dotProduct/1-1;  // Asegúrate de aplicar correctamente el valor de alpha
                        //}
                        //else
                        //{
                        //    alphaMap[i, j, 1] = 0.0f;  // No pintar en superficies planas
                        //}
                    }
                }
            }
        }
        terrainData.SetAlphamaps(startXPaint, startZPaint, alphaMap);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(worldPos, Vector3.down);
    }
    public Vector3 GetTerrainNormal(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Convertir las coordenadas del mundo a coordenadas del heightmap
        int mapX = Mathf.FloorToInt((worldPos.x / terrainData.size.x) * terrainData.heightmapResolution);
        int mapZ = Mathf.FloorToInt((worldPos.z / terrainData.size.z) * terrainData.heightmapResolution);

        // Asegúrate de que las coordenadas estén dentro del rango
        mapX = Mathf.Clamp(mapX, 1, terrainData.heightmapResolution - 2);
        mapZ = Mathf.Clamp(mapZ, 1, terrainData.heightmapResolution - 2);

        // Obtener las alturas de los puntos vecinos
        float[,] heights = terrainData.GetHeights(mapX - 1, mapZ - 1, 3, 3);

        float heightL = heights[1, 0];
        float heightR = heights[1, 2];
        float heightD = heights[0, 1];
        float heightU = heights[2, 1];

        // Calcular la normal
        float terrainWidth = terrainData.size.x / (terrainData.heightmapResolution - 1);
        float terrainHeight = terrainData.size.z / (terrainData.heightmapResolution - 1);

        Vector3 normal = new Vector3(
            (heightL - heightR) / terrainWidth,
            2.0f,
            (heightD - heightU) / terrainHeight
        );

        normal.Normalize();

        return normal;
    }
    void PaintGrass(bool removeGrass)
    {
        TerrainData terrainData = terrain.terrainData;

        // Convertir las coordenadas del golpeo en coordenadas de detalle
        int detailResolution = terrainData.detailResolution;

        int grassX = Mathf.RoundToInt(((point.x - terrain.transform.position.x) / terrainData.size.x) * detailResolution);
        int grassZ = Mathf.RoundToInt(((point.z - terrain.transform.position.z) / terrainData.size.z) * detailResolution);

        //int brushSizeInDetails = Mathf.RoundToInt((brushSize / terrainData.size.x) * detailResolution);
        int brushSizeInDetails = brushSize;
        // Obtener el mapa de detalles existente
        int[,] detailLayer = terrainData.GetDetailLayer(
            Mathf.Clamp(grassX - brushSizeInDetails / 2, 0, detailResolution - 1),
            Mathf.Clamp(grassZ - brushSizeInDetails / 2, 0, detailResolution - 1),
            brushSizeInDetails, brushSizeInDetails, 0);

        // Añadir hierba en el área seleccionada
        for (int z = 0; z < detailLayer.GetLength(0); z++)
        {
            for (int x = 0; x < detailLayer.GetLength(1); x++)
            {
                // Calcular la distancia desde el centro del pincel
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(brushSizeInDetails / 2, brushSizeInDetails / 2));

                if (distance <= brushSizeInDetails / 2)
                {
                    if (removeGrass)
                    {
                        detailLayer[z, x] = 0;
                    }
                    else
                    {
                        detailLayer[z, x] = Mathf.Min(detailLayer[z, x] + grassDensity, 16); // 16 es el máximo permitido por celda
                    }
                }
            }
        }

        // Aplicar el mapa de detalles modificado al terreno
        terrainData.SetDetailLayer(
            Mathf.Clamp(grassX - brushSizeInDetails / 2, 0, detailResolution - 1),
            Mathf.Clamp(grassZ - brushSizeInDetails / 2, 0, detailResolution - 1),
            0, detailLayer);
    }
    void GetHeightPointForFlat()
    {
        flat = terrainY;
        menuTerrainInfo.SetFlat(flat);
    }
    void FlatTerrain()
    {
        flat = menuTerrainInfo.GetFlat();

        int localTerrainX = terrainX - startX;
        int localTerrainZ = terrainZ - startZ;
        int sqrRadius = brushSize * brushSize;

        for (int i = 0; i < endZ - startZ; i++)
        {
            for (int j = 0; j < endX - startX; j++)
            {
                int sqrDstFromCentre = (j - localTerrainX) * (j - localTerrainX) + (i - localTerrainZ) * (i - localTerrainZ);

                if (sqrDstFromCentre <= sqrRadius)
                {
                    heights[i, j] = Mathf.Lerp(heights[i, j], flat, flatForce * Time.deltaTime);
                }
            }
        }
        terrain.terrainData.SetHeights(startX, startZ, heights);
    }
    void PaintTrees()
    {
        for (int i = 0; i < treesPerBrush; i++)
        {
            // Calcular una posición aleatoria dentro del radio del pincel
            Vector2 randomPoint = Random.insideUnitCircle * brushSize * 10;
            float treeX = point.x + randomPoint.x;
            float treeZ = point.z + randomPoint.y;

            // Asegurarse de que las coordenadas estén dentro del terreno
            treeX = Mathf.Clamp(treeX, terrain.transform.position.x, terrain.transform.position.x + terrain.terrainData.size.x);
            treeZ = Mathf.Clamp(treeZ, terrain.transform.position.z, terrain.transform.position.z + terrain.terrainData.size.z);

            // Obtener la altura del terreno en esa posición
            float treeY = terrain.SampleHeight(new Vector3(treeX, 0, treeZ)) + terrain.transform.position.y;

            // Crear una nueva instancia de árbol
            TreeInstance treeInstance = new TreeInstance
            {
                position = new Vector3((treeX - terrain.transform.position.x) / terrain.terrainData.size.x, // Normalizar coordenada X
                                       (treeY - terrain.transform.position.y) / terrain.terrainData.size.y, // Normalizar coordenada Y
                                       (treeZ - terrain.transform.position.z) / terrain.terrainData.size.z), // Normalizar coordenada Z
                widthScale = 1,
                heightScale = 1,
                rotation = Random.Range(0, 360), // Rotación aleatoria
                color = Color.white,
                lightmapColor = Color.white,
                prototypeIndex = treeIndex // Índice del tipo de árbol a pintar
            };

            // Añadir el árbol al terreno
            terrain.AddTreeInstance(treeInstance);
        }

        // Refrescar el terreno para que los nuevos árboles sean visibles
        terrain.Flush();
    }
    void PaintTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

        // Get the terrain coordinates where the mouse hits
        float multiplier = terrainData.alphamapWidth / width + 1;
        int brushPaint = Mathf.RoundToInt(brushSize * multiplier);
        int sqrRadius = brushPaint * brushPaint;

        terrainX = (int)(((point.x - terrain.transform.position.x) / terrainWidth) * terrainData.alphamapWidth);
        terrainZ = (int)(((point.z - terrain.transform.position.z) / terrainDepth) * terrainData.alphamapHeight);

        // Only get the portion of the alpha map affected by the brush
        int startX = Mathf.Clamp(terrainX - brushPaint, 0, terrainData.alphamapWidth - 1);
        int startZ = Mathf.Clamp(terrainZ - brushPaint, 0, terrainData.alphamapHeight - 1);
        int endX = Mathf.Clamp(terrainX + brushPaint, 0, terrainData.alphamapWidth - 1);
        int endZ = Mathf.Clamp(terrainZ + brushPaint, 0, terrainData.alphamapHeight - 1);

        float[,,] alphaMap = terrainData.GetAlphamaps(startX, startZ, endX - startX, endZ - startZ);

        int localTerrainX = terrainX - startX;
        int localTerrainZ = terrainZ - startZ;

        for (int i = 0; i < endZ - startZ; i++)
        {
            for (int j = 0; j < endX - startX; j++)
            {
                int sqrDstFromCentre = (j - localTerrainX) * (j - localTerrainX) + (i - localTerrainZ) * (i - localTerrainZ);

                if (sqrDstFromCentre <= sqrRadius)
                {
                    float distance = Vector2.Distance(new Vector2(j, i), new Vector2(localTerrainX, localTerrainZ));
                    float strengthPaint = Mathf.Clamp01(1 - (distance / brushPaint)) * strength/3;

                    for (int t = 0; t < terrainData.alphamapLayers; t++)
                    {
                        if (t == textureIndex)
                        {
                            alphaMap[i, j, t] += strength * Time.deltaTime*3 ;
                        }
                        //else
                        //{
                        //    alphaMap[i, j, t] *= strength * Time.deltaTime; ;
                        //}
                    }
                }
            }
        }

        // Apply the modified alpha map back to the terrain
        terrainData.SetAlphamaps(startX, startZ, alphaMap);
    }

    void SmoothTerrain()
    {
        int localTerrainX = terrainX - startX;
        int localTerrainZ = terrainZ - startZ;
        int sqrRadius = brushSize * brushSize;

        for (int i = 0; i < endZ - startZ; i++)
        {
            for (int j = 0; j < endX - startX; j++)
            {
                int sqrDstFromCentre = (j - localTerrainX) * (j - localTerrainX) + (i - localTerrainZ) * (i - localTerrainZ);

                if (sqrDstFromCentre <= sqrRadius)
                {
                    float averageHeight = 0f;
                    int count = 0;

                    //int newX = j + (int)terrainX - brushSize;
                    //int newZ = i + (int)terrainZ - brushSize;
                    //newX = Mathf.Clamp(newX, 0, width - 1);
                    //newZ = Mathf.Clamp(newZ, 0, height - 1);

                    // Loop through the neighboring vertices within the brush area
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                        {
                            int neighborX = Mathf.Clamp(i + offsetX, 0, endX-startX-1);
                            int neighborZ = Mathf.Clamp(j + offsetZ, 0, endZ-startZ-1);

                            averageHeight += heights[neighborX, neighborZ];
                            count++;
                        }
                    }

                    // Calculate the smoothed height
                    averageHeight /= count;
                    heights[i, j] = Mathf.Lerp(heights[i, j], averageHeight, strength * Time.deltaTime * strengthMutliplierSmooth);
                }
            }
        }
        terrain.terrainData.SetHeights(startX, startZ, heights);
    }
    bool GetInfoTerrain()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            point = hit.point;

            terrain = hit.transform.GetComponent<Terrain>();
            width = terrain.terrainData.heightmapResolution;
            height = terrain.terrainData.heightmapResolution;
            terrainWidth = terrain.terrainData.size.x;
            terrainDepth = terrain.terrainData.size.z;
            terrainHeight = terrain.terrainData.size.y;

            // Convert the vertex position to terrain coordinates
            terrainX = (int)(((hit.point.x - terrain.transform.position.x) / terrainWidth) * width);
            terrainZ = (int)(((hit.point.z - terrain.transform.position.z) / terrainDepth) * height);

            // Ensure terrain coordinates are within bounds
            terrainX = Mathf.Clamp(terrainX, 0, width - 1);
            terrainZ = Mathf.Clamp(terrainZ, 0, height - 1);

            // Get the corresponding height on the terrain
            terrainY = terrain.transform.InverseTransformPoint(hit.point).y / terrainHeight;


            // Only get the portion of the alpha map affected by the brush
            startX = Mathf.Clamp(terrainX - brushSize, 0, width - 1);
            startZ = Mathf.Clamp(terrainZ - brushSize, 0, height - 1);
            endX = Mathf.Clamp(terrainX + brushSize, 0, width - 1);
            endZ = Mathf.Clamp(terrainZ + brushSize, 0, height - 1);
            heights = terrain.terrainData.GetHeights(startX, startZ, endX - startX, endZ - startZ);

            strength = menuTerrainInfo.GetStrenght();
            mode = menuTerrainInfo.GetMode();
            textureIndex = menuTerrainInfo.brushIndex;

            minHeightController = minHeight / terrainHeight;

            return true;
        }
        return false;
    }
	// Smoothly raise terrain to target height at given point 
	void RaiseTerrain()
	{
        int localTerrainX = terrainX - startX;
        int localTerrainZ = terrainZ - startZ;
        int sqrRadius = brushSize * brushSize;

        for (int i = 0; i < endZ - startZ; i++)
        {
            for (int j = 0; j < endX - startX; j++)
            {
                int sqrDstFromCentre = (j - localTerrainX) * (j - localTerrainX) + (i - localTerrainZ) * (i - localTerrainZ);

                if (sqrDstFromCentre <= sqrRadius)
                {
                    // Cal culate brush weight with exponential falloff from centre 
                    float dstFromCentre = Mathf.Sqrt(sqrDstFromCentre);
                    float t = dstFromCentre / brushSize;
                    float brushWeight = Mathf.Exp(-t * t / brushFallOff);
                    brushWeight = Mathf.Exp(-t * blendCurve.Evaluate(t));

                    if (scaleUp)
                    {
                        heights[i, j] += strength * Time.deltaTime;
                    }
                    else
                    {
                        heights[i, j] -= strength * Time.deltaTime;
                        //if (minHeightController < heightMap[brushX, brushY] - strength * Time.deltaTime)
                        //{
                        //    heightMap[brushX, brushY] -= strength * Time.deltaTime;
                        //}
                        //else
                        //{
                        //    heightMap[brushX, brushY] = minHeightController;
                        //}
                    }
                }
            }
        }
        terrain.terrainData.SetHeights(startX, startZ, heights);      
    }
}
