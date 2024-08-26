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
                        ModifyTerrain();
                        break;
                    case 1:
                        SmoothTerrain();
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
        int brushWidth = Mathf.RoundToInt((brushSize / terrainWidth) * width);
        int brushHeight = Mathf.RoundToInt((brushSize / terrainDepth) * height);      

        int sqrRadius = brushSize * brushSize;
        // Smooth the heights within the brush area
        for (int x = -brushSize; x < brushSize; x++)
        {
            for (int z = -brushSize; z < brushSize; z++)
            {
                int sqrDstFromCentre = x * x + z * z;
                // Check i f point is inside brush radius 
                if (sqrDstFromCentre <= sqrRadius)
                {
                    float averageHeight = 0f;
                    int count = 0;

                    int newX = x + (int)terrainX;
                    int newZ = z + (int)terrainZ;
                    newX = Mathf.Clamp(newX, 0, width - 1);
                    newZ = Mathf.Clamp(newZ, 0, height - 1);

                    heights[newZ, newX] = Mathf.Lerp(heights[newZ, newX], flat, flatForce * Time.deltaTime);
                }

            }
        }


        //Apply the modified heights back to the terrain
        terrain.terrainData.SetHeights(0, 0, heights);
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
                            alphaMap[i, j, t] += strengthPaint;
                        }
                        else
                        {
                            alphaMap[i, j, t] *= (1 - strengthPaint);
                        }
                    }
                }
            }
        }

        // Apply the modified alpha map back to the terrain
        terrainData.SetAlphamaps(startX, startZ, alphaMap);
    }

    void SmoothTerrain()
    {
        int brushWidth = Mathf.RoundToInt((brushSize / terrainWidth) * width);
        int brushHeight = Mathf.RoundToInt((brushSize / terrainDepth) * height);

        // Get the area affected by the brush
        int startX = Mathf.Clamp(terrainX - brushWidth / 2, 0, width - 1);
        int startZ = Mathf.Clamp(terrainZ - brushHeight / 2, 0, height - 1);
        int endX = Mathf.Clamp(terrainX + brushWidth / 2, 0, width);
        int endZ = Mathf.Clamp(terrainZ + brushHeight / 2, 0, height);


        int sqrRadius = brushSize * brushSize;
        // Smooth the heights within the brush area
        for (int x = -brushSize; x < brushSize; x++)
        {
            for (int z = -brushSize; z < brushSize; z++)
            {
                int sqrDstFromCentre = x * x + z * z;
                // Check i f point is inside brush radius 
                if (sqrDstFromCentre <= sqrRadius)
                {
                    float averageHeight = 0f;
                    int count = 0;

                    int newX = x + (int)terrainX;
                    int newZ = z + (int)terrainZ;
                    newX = Mathf.Clamp(newX, 0, width - 1);
                    newZ = Mathf.Clamp(newZ, 0, height - 1);

                    // Loop through the neighboring vertices within the brush area
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                        {
                            int neighborX = Mathf.Clamp(newX + offsetX, 0, width - 1);
                            int neighborZ = Mathf.Clamp(newZ + offsetZ, 0, height - 1);

                            averageHeight += heights[neighborZ, neighborX];
                            count++;
                        }
                    }

                    // Calculate the smoothed height
                    averageHeight /= count;
                    heights[newZ, newX] = Mathf.Lerp(heights[newZ, newX], averageHeight, strength * Time.deltaTime * strengthMutliplierSmooth);
                }
                
            }
        }
        

        //Apply the modified heights back to the terrain
        terrain.terrainData.SetHeights(0, 0, heights);
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
            heights = terrain.terrainData.GetHeights(0, 0, width, height);

            strength = menuTerrainInfo.GetStrenght();
            mode = menuTerrainInfo.GetMode();
            textureIndex = menuTerrainInfo.brushIndex;

            minHeightController = minHeight / terrainHeight;

            return true;
        }
        return false;
    }
    void ModifyTerrain()
    {        
        if (scaleUp)
        {
            heights[(int)terrainZ, (int)terrainX] += strength * Time.deltaTime;
        }
        else
        {
            heights[(int)terrainZ, (int)terrainX] -= strength * Time.deltaTime;
        }
        RaiseTerrain(heights, brushSize, (int)terrainZ, (int)terrainX, heights[(int)terrainZ, (int)terrainX], terrain);
        terrain.terrainData.SetHeights(0, 0, heights);
    }
	// Smoothly raise terrain to target height at given point 
	void RaiseTerrain(float[,] heightMap, int radius, int centreX, int centreY, float tergetHeight,Terrain terrain)
	{
        int sqrRadius = radius * radius;
		// Loop over brush bounding box 
		for (int offsetY = -radius; offsetY <= radius; offsetY++)
		{
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    int sqrDstFromCentre = offsetX * offsetX + offsetY * offsetY;
                    // Check i f point is inside brush radius 
                    if (sqrDstFromCentre <= sqrRadius)
                    {
                        // Cal culate brush weight with exponential falloff from centre 
                        float dstFromCentre = Mathf.Sqrt(sqrDstFromCentre);
                        float t = dstFromCentre / radius;
                        float brushWeight = Mathf.Exp(-t * t / brushFallOff);
                        brushWeight = Mathf.Exp(-t * blendCurve.Evaluate(t));

                        // Rai se terrain 
                        int brushX = centreX + offsetX;
                        int brushY = centreY + offsetY;

                        if (brushX >= 0 && brushY >= 0 && brushX < width && brushY < height)
                        {
                        if (scaleUp)
                        {
                            heightMap[brushX, brushY] += strength * Time.deltaTime;
                        }
                        else
                        {
                            if (minHeightController < heightMap[brushX, brushY] - strength * Time.deltaTime)
                            {
                                heightMap[brushX, brushY] -= strength * Time.deltaTime;
                            }
                            else
                            {
                                heightMap[brushX, brushY] = minHeightController;
                            }
                        }
                        }
                    }
                }           
		}
	}
}
