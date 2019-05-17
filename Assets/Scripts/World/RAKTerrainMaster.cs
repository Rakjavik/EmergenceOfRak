using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using rak;
using rak.world;
using System.Collections;

public partial class RAKTerrainMaster : MonoBehaviour
{
    public enum NEIGHDIRECTION { Left,Right,Up,Down}
    public enum HeightGenerationType { Perlin, Blocky }

    public bool debug = World.ISDEBUGSCENE;
    public bool forceGenerate = false; // Will generate the terrain even if save data is available
    private void logDebug(string message)
    {
        if (debug)
        {
            Debug.Log("TerrainMaster - " + message);
        }
    }
    private static HexCell cell; // Hexcell that represents the terrain being generated
    private static Area area; // Area holds all variables related to the local representation of the HexCell
    private static World world;
    private static RAKTerrain[] terrain; // Terrain objects for this Area

    [Header("Tree Prefabs needed for Terrain Generation")]
    public GameObject[] treePrefabs;
    [Header("Ground Textures needed for Terrain Generation")]
    public Texture[] grassTextures;
    [Header("Material to override default")]
    public Material TerrainMaterial;
    public static float GetTerrainHeightAt(Vector2 position,RAKTerrain terrain)
    {
        return terrain.GetHeightAt(position);
    }
    private static RAKWeather sun;
    private static RAKBiome currentBiome;
    private static Dictionary<System.Guid, RAKTerrain> terrainList = new Dictionary<Guid, RAKTerrain>();
    public static RAKTerrain GetTerrainByGuid(Guid guid)
    {
        return terrainList[guid];
    }
    public static int TileSize = 256; // Size of each Terrain piece
    private int width = TileSize+1; // Terrain width/height needs a plus one due to Unity being weird
    private int height = TileSize+1; // // Terrain width/height needs a plus one due to Unity being weird
    private static int worldSize = 16; // Number of total terrain objects
    public static bool Initialized = false;

    private void InitializeDebugTerrain(World world,HexCell cell)
    {
        Debug.LogWarning("DEBUG MODE ENABLED");
        worldSize = 4;
    }
    public void Initialize(World world,HexCell cell)
    {
        if (debug)
        {
            InitializeDebugTerrain(world,cell);
        }
        // Set static data //
        RAKTerrainMaster.world = world;
        RAKTerrainMaster.cell = cell;
        DateTime startTime = DateTime.Now;
        terrain = new RAKTerrain[worldSize];
        // Check if files are already present for this cell //
        bool saveDataAvail = IsSaveDataAvailable(cell,world.WorldName);
        if (forceGenerate) saveDataAvail = false;
        if (cell.GetChunkMaterial() == HexGridChunk.ChunkMaterial.GRASS)
            currentBiome = RAKBiome.getForestBiome();
        else
            currentBiome = RAKBiome.getForestBiome();
        
        #region TERRAIN GENERATION
        // Load single terrain loop //
        for (int count = 0; count < worldSize; count++)
        {
            TerrainData td;
            RAKTerrainSavedData savedTerrain = null;
            // If no save data available, go ahead and generate a new terrain //
            if (!saveDataAvail) //  generate //
            {
                if (count > 0)
                    currentBiome.GetNewOffsets();
                td = generateTerrain(width, height, currentBiome.depth, currentBiome.scale, currentBiome.offsetX, 
                    currentBiome.offsetY,HeightGenerationType.Blocky);
                generateSplatPrototypes(td);
            }
            // Save data is available //
            else
            {
                savedTerrain = RAKTerrainSavedData.loadTerrain(cell.GetCellSaveFileName(world.WorldName)+"T"+count + ".area");
                td = savedTerrain.generateTerrainDataFromFlatMap();
                Debug.LogWarning("Loading terrain from disk tdsize - " + td.size.x + "-" + td.size.y + "-" + td.size.z);
            }
            // TerrainData generation complete, finish setting up Unity objects //
            GameObject go = Terrain.CreateTerrainGameObject(td);
            go.isStatic = false;
            go.transform.SetParent(transform);
            go.name = "Terrain" + count;
            logDebug("Terrain created - " + go.name);
            Terrain terrainComp = go.GetComponent<Terrain>();
            terrain[count] = go.AddComponent<RAKTerrain>();
            terrain[count].initialize(this);
            terrain[count].savedData = savedTerrain;
            mapPositions(count,(int)Mathf.Sqrt(worldSize));
            terrain[count].setBiome(RAKBiome.getForestBiome());
            terrainList.Add(terrain[count].guid, terrain[count]);
        }
        // Tell Unity which terrain objects are next to each other //
        setNeighbors();
        // If we are generating new world terrains, fix the sides and seam together //
        if (!saveDataAvail) fixGaps(16);
        // Generate the terrain details //
        for (int count = 0; count < terrain.Length; count++)
        {
            generateSplatMap(terrain[count].getTerrainData());
            // If no save data, generate new details //
            if (!saveDataAvail)
            {
                generateNonTerrainDetailPrefabs(terrain[count].getTerrainComponenet());
            }
            // Save data avail, load details from disk //
            else
            {
                Debug.Log("Loading terrain from disk");
                generateNonTerrainDetailsFromDisk(terrain[count]);
            }
        }
        #endregion
        
        float currentTime = Time.time;
        
        // Save to disk if needed //
        if (!saveDataAvail)
            SaveTerrainData();
            
        // Finish loading //
        for(int count = 0; count < terrain.Length; count++)
        {
            RAKTerrain singleTerrain = terrain[count];
            singleTerrain.InitializeGrid();
        }
        RAKTerrainMaster.generateCreatures(cell,world);
        RAKTerrainMaster.sun = gameObject.AddComponent<RAKWeather>();
        RAKTerrainMaster.sun.start(transform, this, sun.gameObject);
        TimeSpan difference = startTime.Subtract(DateTime.Now);
        Debug.LogWarning("LOAD TIME - " + difference.TotalMilliseconds);
    }
    private static void generateCreatures(HexCell cell,World world)
    {
        area = new Area(cell, world);
        cell.SetArea(area);
        Debug.Log("Area Initialized");
    }
    // Create new unity objects from data from disk //
    private void generateNonTerrainDetailsFromDisk(RAKTerrain terrain)
    {
        // Get all objects from save //
        RAKTerrainObjectSaveData[] objects = terrain.savedData.nonTerrainObjectsSaveData;
        List<RAKTerrainObject> loadedTerrainObjects = new List<RAKTerrainObject>();
        for (int count = 0; count < objects.Length; count++)
        {
            // Retrieve the prefab and Instantiate //
            Debug.LogWarning(objects[count].prefabObjectIndex);
            if(objects[count].prefabObjectIndex == -1)
            {
                Debug.LogWarning("Object # " + count + " returned -1");
                continue;
            }
            string prefabName = RAKUtilities.nonTerrainObjects[objects[count].prefabObjectIndex];
            RAKTerrainObject terrainObject = RAKUtilities.getTerrainObjectPrefab(prefabName).GetComponent<RAKTerrainObject>();
            GameObject prefab = (GameObject)Instantiate(terrainObject.gameObject, objects[count].position.getVector3(), Quaternion.identity);//, 2);
            prefab.transform.eulerAngles = objects[count].rotationEulers.getVector3();
            prefab.transform.SetParent(terrain.transform);
            loadedTerrainObjects.Add(terrainObject);
        }
        terrain.nonTerrainObjects = loadedTerrainObjects.ToArray();
    }
    
    // Sets the neighbors for all of our Unity Terrain Objects //
    private void setNeighbors()
    {
        for (int index = 0; index < terrain.Length; index++)
        {
            Terrain[] neighborTerrain = new Terrain[4];
            Terrain tUp = neighborTerrain[(int)NEIGHDIRECTION.Up];
            Terrain tDown = neighborTerrain[(int)NEIGHDIRECTION.Down];
            Terrain tLeft = neighborTerrain[(int)NEIGHDIRECTION.Left];
            Terrain tRight = neighborTerrain[(int)NEIGHDIRECTION.Right];
            int left = index - 1;
            int right = index + 1;
            int worldSquareRoot = (int)Mathf.Sqrt(worldSize);
            int up = index + worldSquareRoot;
            int down = index - worldSquareRoot;
            if (up >= worldSize)
            {
                up = -1;
            }
            else
            {
                tUp = terrain[up].getTerrainComponenet();
            }
            if (down < 0)
            {
                down = -1;
            }
            else
            {
                tDown = terrain[down].getTerrainComponenet();
            }
            if (left < 0 || left % worldSquareRoot == worldSquareRoot-1)
            {
                left = -1;
            }
            else
            {
                tLeft = terrain[left].getTerrainComponenet();
            }
            if (right >= worldSize || right % worldSquareRoot == 0)
            {
                right = -1;
            }
            else
            {
                tRight = terrain[right].getTerrainComponenet();
            }
            int[] loop = new int[4];
            loop[(int)NEIGHDIRECTION.Down] = down;
            loop[(int)NEIGHDIRECTION.Up] = up;
            loop[(int)NEIGHDIRECTION.Left] = left;
            loop[(int)NEIGHDIRECTION.Right] = right;
            for (int count = 0; count < loop.Length; count++)
            {
                if(loop[count] != -1 && terrain[loop[count]] != null)
                    terrain[index].SetNeighbor(terrain[loop[count]],count);
            }
        }
    }
    private void mapPositions(int index, int columnCount)
    {
        if (worldSize == 1)
        {
            terrain[index].transform.position = Vector3.zero;
        }
        else
        {
            int y = index / (worldSize / columnCount);
            int x = index - y * (worldSize / columnCount);
            terrain[index].transform.position = new Vector3(x * TileSize, 0, y * TileSize);
        }
    }
    private TerrainData generateTerrain(int width, int height, int depth, float scale, float offsetX, float offsetY,
        HeightGenerationType generationType)
    {
        TerrainData data = new TerrainData();
        data.heightmapResolution = width;
        data.size = new Vector3(width, depth, height);
        data.SetHeights(0, 0, generateHeights(width, height, scale, offsetX, offsetY,generationType));
        return data;
    }
    private int getRandomTerrainObject(int[] percentages)
    {
        for (int count = 0; count < percentages.Length; count++)
        {
            if (UnityEngine.Random.Range(0, 100) < percentages[count])
            {
                return count;
            }
        }
        return 0;
    }
    // Generate details for a RAKTerrain object //
    private void generateNonTerrainDetailPrefabs(Terrain terrain)
    {
        RAKTerrain rakTerrain = terrain.GetComponent<RAKTerrain>();
        TerrainData data = terrain.terrainData;
        RAKBiome biome = rakTerrain.getBiome();
        int sum = 0;
        Array.ForEach(biome.objectCounts, delegate (int i) { sum += i; });
        int totalNumberOfObjects = sum;
        GameObject[] details = new GameObject[totalNumberOfObjects];
        int currentDetailCount = 0;
        for (int currentObjectCount = 0; currentObjectCount < biome.objectCounts.Length; currentObjectCount++)
        {
            GameObject prefab = null;
            try
            {
                prefab = RAKUtilities.getTerrainObjectPrefab(biome.prefabNames[currentObjectCount]);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Unable to get prefab - " + biome.prefabNames[currentObjectCount]);
                continue;
            }
            RAKTerrainObject terrainObject = prefab.GetComponent<RAKTerrainObject>();
            terrainObject.UpdatePropertiesBasedOnName(prefab.name);
            int maxTries = terrainObject.numberOfTimesToSearchForLocation;
            float maxSteepness = terrainObject.maxSteepness;
            float minDistFromEdge = terrainObject.minDistFromEdge;
            float minimiumDistanceBetweenObjects = terrainObject.minimiumDistanceBetweenObjects;
            float heightOffset = terrainObject.heightOffset;
            MeshFilter meshFilter = terrainObject.GetComponent<MeshFilter>();
            float detailHeight,detailWidthX, detailWidthZ;
            if (meshFilter == null)
            {
                detailHeight = 1;
                detailWidthX = 1;
                detailWidthZ = 1;
            }
            else
            {
                detailHeight = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * prefab.transform.localScale.y;
                detailWidthX = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * prefab.transform.localScale.x;
                detailWidthZ = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.size.z * prefab.transform.localScale.z;
            }
            // Sometimes we need to force a width if the bottom of the object is skinny (trees) //
            if (terrainObject.sizeOverride != Vector3.zero)
            {
                detailWidthX = terrainObject.sizeOverride.x;
                detailWidthZ = terrainObject.sizeOverride.z;
                detailHeight = terrainObject.sizeOverride.y;
            }
            float widthXNorm = (detailWidthX / data.heightmapWidth) * 100;
            float widthZNorm = (detailWidthZ / data.heightmapHeight) * 100;
            string debugString = "";
            for (int count = 0; count < biome.objectCounts[currentObjectCount]; count++)
            {
                bool locationValid = false;
                float x = 0;
                float z = 0;
                float y = 0;
                int tries = 0;
                while (!locationValid)
                {
                    locationValid = true;
                    tries++;
                    x = UnityEngine.Random.Range(0, width);
                    z = UnityEngine.Random.Range(0, height);
                    float zNormed = (float)z / (float)data.heightmapHeight;
                    float xNormed = (float)x / (float)data.heightmapWidth;
                    y = data.GetInterpolatedHeight(xNormed, zNormed) - detailHeight - heightOffset;
                    if (x < detailWidthX + minDistFromEdge || x > width - detailWidthX - minDistFromEdge)
                    {
                        locationValid = false;
                        debugString = "Too close to edgeX - " + x;
                        continue;
                    }
                    if (z < detailWidthZ + minDistFromEdge || z > height - detailWidthZ - minDistFromEdge)
                    {
                        locationValid = false;
                        debugString = "Too close to edgeZ - " + z;
                        continue;
                    }
                    locationValid = true;
                    for (int otherObjectsCount = 0; otherObjectsCount < details.Length; otherObjectsCount++)
                    {
                        if (details[otherObjectsCount] != null)
                        {
                            if (Vector2.Distance(new Vector2(terrain.transform.position.x + x, terrain.transform.position.z + z), new Vector2(details[otherObjectsCount].transform.position.x, details[otherObjectsCount].transform.position.z))
                                < details[otherObjectsCount].GetComponent<RAKTerrainObject>().minimiumDistanceBetweenObjects)
                            {
                                locationValid = false;
                                debugString = "Too close to other object - " + (Vector2.Distance(new Vector2(terrain.transform.position.x + x, terrain.transform.position.z + z), new Vector2(details[otherObjectsCount].transform.position.x, details[otherObjectsCount].transform.position.z)) +
                                    " - " + (details[otherObjectsCount].GetComponent<RAKTerrainObject>().minimiumDistanceBetweenObjects));
                                break;
                            }
                        }
                    }
                    if (locationValid)
                    {
                        for (int countX = (int)-widthXNorm; countX < widthXNorm; countX++)
                        {
                            for (int countZ = (int)-widthZNorm; countZ < widthZNorm; countZ++)
                            {
                                float steepness = data.GetSteepness((x + countX) / data.alphamapWidth, (z + countZ) / data.alphamapHeight);
                                if (steepness > maxSteepness)
                                {
                                    locationValid = false;
                                    debugString = "Too steep - " + steepness + " - " + maxSteepness;
                                }
                            }
                        }
                    }
                    if (tries >= maxTries)
                    {
                        //Debug.LogWarning(debugString + " - " + terrainObject.name);
                        break;
                    }
                }
                if (locationValid)
                {
                    details[currentDetailCount] = (GameObject)Instantiate(prefab, new Vector3
                        (x + terrain.transform.position.x, y + detailHeight, z + terrain.transform.position.z),
                        Quaternion.identity);//, 3);
                    details[currentDetailCount].gameObject.name = prefab.name;
                    details[currentDetailCount].transform.SetParent(terrain.transform);
                    //Debug.LogWarning("Instantiated - " + details[currentDetailCount].name);
                    //details[currentDetailCount].GetComponent<RAKTerrainObject>().setPrefabObjectName(prefab.name);
                    //details[currentDetailCount].transform.position = new Vector3(x + terrain.transform.position.x, y + detailHeight, z + terrain.transform.position.z);
                    if (!terrainObject.freeRotate)
                    {
                        details[currentDetailCount].transform.rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));
                    }
                    else
                    {
                        details[currentDetailCount].transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 360),
                            UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360)));
                    }
                }
                currentDetailCount++;
            }
        }
        if (details != null)
        {
            List<RAKTerrainObject> nonTerrainObjects = new List<RAKTerrainObject>();
            for (int count = 0; count < details.Length; count++)
            {
                if (details[count] != null)
                {
                    RAKTerrainObject terrainObject = details[count].GetComponent<RAKTerrainObject>();
                    nonTerrainObjects.Add(terrainObject);
                }
            }
            rakTerrain.nonTerrainObjects = nonTerrainObjects.ToArray();
        }
    }
    private void generateSplatMap(TerrainData terrainData)
    {
        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalise x/y coordinates to range 0-1 
                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                //float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth)) / terrainData.heightmapHeight;
                float height = terrainData.GetHeights(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth), 1, 1)[0, 0];
                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);

                // Calculate the steepness of the terrain
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT

                // Texture[0] has constant influence
                splatWeights[0] = .8f;

                // Texture[1] is stronger at higher altitudes //
                splatWeights[1] = height/2.5f;

                // Texture[2] stronger on steeper terrain //
                splatWeights[2] = steepness/40;

                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
                // Subtract result from 1.0 to give greater weighting to flat surfaces
                //splatWeights[2] = 1.0f - Mathf.Clamp01(steepness * steepness / (terrainData.heightmapHeight / 5.0f));

                // Texture[3] increases with height but only on surfaces facing positive Z axis 
                //splatWeights[3] = Mathf.Clamp01(height*normal.z);

                // High Elevation //
                //splatWeights[4] = Mathf.Clamp01(height/terrainData.heightmapHeight + .3f);
                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = 0;
                foreach (float n in splatWeights)
                {
                    z += n;
                }

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {

                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }
    private void generateSplatPrototypes(TerrainData data)
    {
        SplatPrototype[] tex = new SplatPrototype[grassTextures.Length];
        for (int count = 0; count < tex.Length; count++)
        {
            tex[count] = new SplatPrototype();
            tex[count].texture = (Texture2D)grassTextures[count];

            tex[count].tileSize = new Vector2(8, 8);
        }
        data.splatPrototypes = tex;
    }
    private void generateTrees(TerrainData terrainData, int numberOfTrees)
    {
        float maxSteepness = 1;
        float minSize = .2f;
        float maxSize = 4f;
        float minDistBetweenTrees = 50;
        int fails = 0;
        List<TreeInstance> unityTrees = new List<TreeInstance>();
        for (int count = 0; count < numberOfTrees; count++)
        {
            TreeInstance treeInstance = new TreeInstance();

            treeInstance.prototypeIndex = UnityEngine.Random.Range(0, treePrefabs.Length);
            treeInstance.color = new Color32(193, 193, 193, 255);
            float scale = UnityEngine.Random.Range(minSize, maxSize);
            treeInstance.heightScale = scale;
            treeInstance.widthScale = scale;
            treeInstance.lightmapColor = Color.white;
            Vector3 position = new Vector3(UnityEngine.Random.Range(0f, 1f), 0, UnityEngine.Random.Range(0f, 1f));
            float xPosition = Mathf.RoundToInt(position.x * terrainData.heightmapWidth);
            float zPosition = Mathf.RoundToInt(position.z * terrainData.heightmapHeight);
            if (xPosition > terrainData.heightmapWidth - 1)
            {
                xPosition = terrainData.heightmapWidth - 1;
            }
            if (zPosition > terrainData.heightmapHeight - 1)
            {
                zPosition = terrainData.heightmapHeight - 1;
            }
            bool valid = true;
            if (terrainData.GetSteepness(xPosition, zPosition) > maxSteepness)
            {
                valid = false;
            }
            position.y = terrainData.GetHeights(Mathf.RoundToInt(xPosition), Mathf.RoundToInt(zPosition), 1, 1)[0, 0];
            treeInstance.position = position;
            if (isTooCloseToOtherTree(treeInstance, unityTrees.ToArray()))
            {
                valid = false;
            }
            if (valid)
            {
                unityTrees.Add(treeInstance);
            }
            else
            {
                count--;
                fails++;
                if (fails > 50)
                {
                    Debug.LogError("Can't find a spot for a tree, screw this, im going home");
                    return;
                }
            }
        }
        terrainData.treeInstances = unityTrees.ToArray();
    }
    private bool isTooCloseToOtherTree(TreeInstance newTree, TreeInstance[] trees)
    {
        float minDistance = .1f;
        float closest = 5000;
        foreach (TreeInstance tree in trees)
        {
            float distance = Vector3.Distance(tree.position, newTree.position);
            if (distance < closest)
            {
                closest = distance;
            }
        }
        //Debug.Log("Closest tree to this - " + (float)closest);
        return closest < minDistance;
    }
    private float[,] generateHeights(int width, int height, float scale, float offsetX, float offsetY,HeightGenerationType type)
    {
        float[,] heights;
        heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float assignedHeight = 0;
                if (type == HeightGenerationType.Perlin)
                    assignedHeight = CalculateHeightPerlin(x, y, width, height, scale, offsetX, offsetY);
                else if (type == HeightGenerationType.Blocky)
                    assignedHeight = CalculateHeightBlocky(x, y, width, height, scale, offsetX, offsetY,2);
                heights[x, y] = assignedHeight;
            }
        }
        return heights;
    }
    private void fixGaps(int startSmoothingAt)
    {
        StartCoroutine(fixTerrainHeights(startSmoothingAt));
    }
    private IEnumerator fixTerrainHeights(int startSmoothingAt)
    { 
        for (int terrainCount = 0; terrainCount < terrain.Length; terrainCount++) // Loop through each piece of terrain
        {
            Terrain singleTerrain = RAKTerrainMaster.terrain[terrainCount].getTerrainComponenet();
            Terrain[] neighbors = terrain[terrainCount].neighbors;
            for (int nCount = 0; nCount < neighbors.Length; nCount++)
            {
                if (nCount == 3) continue;
                float[] targetTerrainsClosestRow = new float[TileSize];
                float[,] thisTerrainsEdge = null;
                float[,] targetTerrainsEdge = null;

                Vector2 startPoint = Vector2.zero;// Start position to insert changed values
                Vector2 targetStartPoint = Vector2.zero;// Start position to insert changed values
                if (neighbors[nCount] != null)
                {
                    int width;
                    width = TileSize / startSmoothingAt;

                    // LEFT
                    if (nCount == 0)
                    {
                        startPoint = new Vector2(0, 0);
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(0, 0, width, TileSize);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(TileSize - width, 0, width, TileSize);
                    }
                    // RIGHT
                    else if (nCount == 1)
                    {
                        startPoint = new Vector2(TileSize-width+1, 0);
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(TileSize-width, 0, width, TileSize);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(0, 0, width, TileSize);
                    }
                    // UP
                    else if (nCount == 2)
                    {
                        startPoint = new Vector2(0, TileSize-width+1);
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(0, TileSize - width, TileSize, width);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(0, 0, TileSize, width);
                    }
                    // DOWN
                    else if (nCount == 3)
                    {
                        startPoint = new Vector2(0, 0);
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(0, 0, TileSize, width);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(0, TileSize-width, TileSize, width);
                    }
                    // CALCULATE SEAMS
                    for (int countLong = 0; countLong < TileSize; countLong++)
                    {
                        if (nCount == 0) // LEFT
                        {
                            targetTerrainsClosestRow[countLong] = targetTerrainsEdge[countLong,width-1];
                        }
                        else if (nCount == 1) // RIGHT
                        {
                            // xy reversed in array
                            // Meet half way between the current terrain and the target terrain
                            targetTerrainsClosestRow[countLong] = thisTerrainsEdge[countLong, 0] +
                            (targetTerrainsEdge[countLong, 0] - thisTerrainsEdge[countLong,0])/2;

                        }
                        else if (nCount == 2) // UP
                        {
                            //targetTerrainsClosestRow[countLong] = thisTerrainsEdge[0,countLong] +
                            //(targetTerrainsEdge[0,countLong] - thisTerrainsEdge[0,countLong]) / 2;
                            targetTerrainsClosestRow[countLong] = targetTerrainsEdge[0, countLong];
                        }
                        else if (nCount == 3) // DOWN
                        {
                            targetTerrainsClosestRow[countLong] = targetTerrainsEdge[width-1, countLong];
                        }
                    }
                    if (nCount == 0) // LEFT
                    {
                        seam(thisTerrainsEdge, targetTerrainsClosestRow, true);
                        Debug.LogWarning("Left seam on Terrain" + terrainCount);
                    }
                    else if (nCount == 1) // RIGHT
                    {
                        seam(thisTerrainsEdge, targetTerrainsClosestRow,false);
                        Debug.LogWarning("Right seam on Terrain" + terrainCount);
                    }
                    else if (nCount == 2) // UP
                    {
                        thisTerrainsEdge = swapDimensions(thisTerrainsEdge);
                        seam(thisTerrainsEdge, targetTerrainsClosestRow,false);
                        thisTerrainsEdge = swapDimensions(thisTerrainsEdge);
                        Debug.LogWarning("Up seam on Terrain" + terrainCount);
                    }
                    else if (nCount == 3) // DOWN
                    {
                        seam(thisTerrainsEdge, targetTerrainsClosestRow,false);
                        Debug.LogWarning("Down seam on Terrain" + terrainCount);
                    }
                    singleTerrain.terrainData.SetHeights((int)startPoint.x, (int)startPoint.y, thisTerrainsEdge);
                    terrain[terrainCount].getTerrainComponenet().Flush();
                    //Initialized = true;
                    yield return null;
                }
            }
        }
        Initialized = true;
    }
    private float[,] swapDimensions(float[,] array)
    {
        float[,] newArray = new float[array.GetLength(1), array.GetLength(0)];
        for (int x = 0; x < newArray.GetLength(0); x++)
        {
            for (int y = 0; y < newArray.GetLength(1); y++)
            {
                newArray[x, y] = array[y, x];
            }
        }
        return newArray;
    }
    private void seam(float[,] edge, float[] destSeamPoints,bool startAtEnd)
    {
        int longSize = edge.GetLength(0);
        int shortSize = edge.GetLength(1);

        for(int longCount = 0; longCount < longSize; longCount++)
        {
            float yStart;
            float yDest = destSeamPoints[longCount];
            if (!startAtEnd)
            {
                yStart = edge[longCount, 0];
            }
            else
            {
                yStart = edge[longCount, shortSize-1];
            }
            
            float step = (yDest - yStart) / shortSize;
            if (!startAtEnd) {
                
                for (int shortCount = 0; shortCount < shortSize; shortCount++)
                {
                    if (shortCount != shortSize - 1)
                        edge[longCount, shortCount] = yStart + step * shortCount;
                    else
                        edge[longCount, shortCount] = yDest;
                }
            }
            else
            {
                for (int shortCount = 0; shortCount < shortSize; shortCount++)
                {
                    if (shortCount != shortSize - 1)
                        edge[longCount,shortSize-shortCount-1] = yStart + step * shortCount;
                    else
                        edge[longCount, 0] = yDest;
                }
            }
        }

    }
    private float CalculateHeightPerlin(int x, int y, int width, int height, float scale, float offsetX, float offsetY)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;
        float perlinResult = Mathf.PerlinNoise(xCoord, yCoord);
        return perlinResult;
    }
    private float CalculateHeightBlocky(int x, int y, int width, int height, float scale, float offsetX, float offsetY,int steps)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;
        float perlinResult = Mathf.PerlinNoise(xCoord, yCoord);
        perlinResult = (float)Math.Round(perlinResult*steps, 1)/steps;
        return perlinResult;
    }
    public bool IsSaveDataAvailable(HexCell cell, string worldName)
    {
        string fileName = cell.GetCellSaveFileName(worldName) + "T0.area";
        Debug.LogWarning("Looking for save data in - " + fileName);
        return File.Exists(fileName);
    }
    private void SaveTerrainData()
    {
        string filePath = cell.GetCellSaveFileName(world.WorldName);
        Debug.LogWarning("Trying ot write terrain data to " + filePath);
        for (int count = 0; count < terrain.Length; count++)
        {
            RAKTerrainSavedData.saveTerrainBytes(terrain[count], filePath + "T" + count + ".area");
        }
    }

    public RAKTerrain[] getTerrain()
    {
        return terrain;
    }
    public static RAKWeather.WeatherType getCurrentWeather()
    {
        return RAKTerrainMaster.sun.getCurrentWeather();
    }
    public GameObject getPlayerRainPrefab()
    {
        return sun.getPlayerRainPrefab();
    }
    public Vector3 GetSize()
    {
        Vector3 returnVector = new Vector3();
        returnVector.x = width*Mathf.Sqrt(worldSize);
        returnVector.z = height* Mathf.Sqrt(worldSize);
        returnVector.y = currentBiome.depth;
        return returnVector;
    }
    public int getSquareSize()
    {
        return (int)(worldSize / Mathf.Sqrt(worldSize)) * height;
    }
    public static RAKTerrain GetTerrainAtPoint(Vector3 point)
    {
        int maxXZ = TileSize * (terrain.Length / (int)Mathf.Sqrt(worldSize));
        for (int count = 0; count < terrain.Length; count++)
        {
            Vector3 terrainPos = terrain[count].transform.position;
            if (point.x < 0)
                point.x = 0;
            else if (point.x > maxXZ)
                point.x = maxXZ;
            if (point.z < 0)
                point.z = 0;
            else if (point.z > maxXZ)
                point.z = maxXZ;

            if (point.x >= terrainPos.x && point.x <= terrainPos.x + TileSize)
            {
                if (point.z >= terrainPos.z && point.z <= terrainPos.z + TileSize)
                {
                    return terrain[count];
                }
            }
        }
        Debug.LogError("Null return for terrain request, point - " + point);
        return null;
    }
}
