using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.AI;
using rak;
using rak.world;
using rak.creatures;

public partial class RAKTerrainMaster : MonoBehaviour
{

    public bool debug = World.ISDEBUGSCENE;
    public bool forceGenerate = false;
    private void logDebug(string message)
    {
        if (debug)
        {
            Debug.Log("TerrainMaster - " + message);
        }
    }
    private static HexCell cell;
    private static Area area;
    private static World world;
    private RAKTerrain[] terrain;
    private int tileSize = 256;

    [Header("Tree Prefabs needed for Terrain Generation")]
    public GameObject[] treePrefabs;
    [Header("Ground Textures needed for Terrain Generation")]
    public Texture[] grassTextures;
    public float GetTerrainHeightAt(Vector2 position,RAKTerrain terrain)
    {
        return terrain.GetHeightAt(position);
    }
    private static RAKWeather sun;
    private static RAKBiome currentBiome;
    private int width;
    private int height;
    private int worldSize;

    private void InitializeDebugTerrain(World world,HexCell cell)
    {
        Debug.LogWarning("DEBUG MODE ENABLED");
        terrain = new RAKTerrain[1];
        terrain[0] = gameObject.GetComponentInChildren<RAKTerrain>();
        terrain[0].initialize(this);
    }
    public void Initialize(World world,HexCell cell)
    {
        if (debug)
        {
            InitializeDebugTerrain(world,cell);
            return;
        }
        RAKTerrainMaster.world = world;
        RAKTerrainMaster.cell = cell;
        DateTime startTime = DateTime.Now;
        width = tileSize + 1;
        height = tileSize + 1;
        worldSize = 16;
        terrain = new RAKTerrain[worldSize];
        bool saveDataAvail = IsSaveDataAvailable(cell,world.WorldName);
        if (forceGenerate) saveDataAvail = false;
        if (cell.GetChunkMaterial() == HexGridChunk.ChunkMaterial.GRASS)
            currentBiome = RAKBiome.getForestBiome();
        else
            currentBiome = RAKBiome.getForestBiome();
        #region TERRAIN GENERATION
        for (int count = 0; count < worldSize; count++)
        {
            
            TerrainData td;
            RAKTerrainSavedData savedTerrain = null;
            if (!saveDataAvail) //  generate //
            {
                td = generateTerrain(width, height, currentBiome.depth, currentBiome.scale, currentBiome.offsetX, currentBiome.offsetY);
                generateSplatPrototypes(td);
            }
            else
            {
                savedTerrain = RAKTerrainSavedData.loadTerrain(cell.GetCellSaveFileName(world.WorldName)+"T"+count + ".area");
                td = savedTerrain.generateTerrainDataFromFlatMap();
                Debug.LogWarning("Loading terrain from disk tdsize - " + td.size.x + "-" + td.size.y + "-" + td.size.z);
            }
            //generateSplatPrototypes(td);
            GameObject go = Terrain.CreateTerrainGameObject(td);
            go.transform.SetParent(transform);
            go.name = "Terrain" + count;
            logDebug("Terrain created - " + go.name);
            Terrain terrainComp = go.GetComponent<Terrain>();
            terrain[count] = go.AddComponent<RAKTerrain>();
            terrain[count].initialize(this);
            terrain[count].savedData = savedTerrain;
            mapPositions(count);
            terrain[count].setBiome(RAKBiome.getForestBiome());
        }
        setNeighbors();
        if (!saveDataAvail) fixGaps(16);
        for (int count = 0; count < terrain.Length; count++)
        {
            generateSplatMap(terrain[count].getTerrainData());
            if (!saveDataAvail)
            {
                generateNonTerrainDetailPrefabs(terrain[count].getTerrainComponenet());
            }
            else
            {
                Debug.Log("Loading terrain from disk");
                generateNonTerrainDetailsFromDisk(terrain[count]);
            }
        }
        #endregion
        float currentTime = Time.time;
        List<NavMeshSurface> surfaces = new List<NavMeshSurface>();
        surfaces.Add(gameObject.AddComponent<NavMeshSurface>());
        surfaces[0].collectObjects = CollectObjects.Children;
        RAKMeshBaker.Bake(surfaces.ToArray());
        Debug.LogWarning("Build nav mesh took - " + (Time.time - currentTime));

        if (!saveDataAvail)
            SaveTerrainData();
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
    private void generateNonTerrainDetailsFromDisk(RAKTerrain terrain)
    {
        RAKTerrainObjectSaveData[] objects = terrain.savedData.nonTerrainObjectsSaveData;
        List<RAKTerrainObject> loadedTerrainObjects = new List<RAKTerrainObject>();
        for (int count = 0; count < objects.Length; count++)
        {
            RAKTerrainObject terrainObject = RAKUtilities.getTerrainObjectPrefab(
                RAKUtilities.nonTerrainObjects[objects[count].prefabObjectIndex])
                .GetComponent<RAKTerrainObject>();
            GameObject prefab = (GameObject)Instantiate(terrainObject.gameObject, objects[count].position.getVector3(), Quaternion.identity);//, 2);
            prefab.transform.eulerAngles = objects[count].rotationEulers.getVector3();
            prefab.transform.SetParent(terrain.transform);
            loadedTerrainObjects.Add(terrainObject);
        }
        terrain.nonTerrainObjects = loadedTerrainObjects.ToArray();
    }
    private void setNeighbors()
    {
        for (int index = 0; index < terrain.Length; index++)
        {
            Terrain tLeft = null;
            Terrain tRight = null;
            Terrain tUp = null;
            Terrain tDown = null;
            int left = index - 1;
            int right = index + 1;
            int up = index + worldSize / 4;
            int down = index - worldSize / 4;
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
            if (left < 0 || left % 4 == 3)
            {
                left = -1;
            }
            else
            {
                tLeft = terrain[left].getTerrainComponenet();
            }
            if (right >= worldSize || right % 4 == 0)
            {
                right = -1;
            }
            else
            {
                tRight = terrain[right].getTerrainComponenet();
            }
            terrain[index].setNeighbors(tLeft, tRight, tUp, tDown);
        }
    }
    private void mapPositions(int index)
    {
        int y = index / (worldSize / 4);
        int x = index - y * (worldSize / 4);
        terrain[index].transform.position = new Vector3(x * tileSize, 0, y * tileSize);
    }
    private TerrainData generateTerrain(int width, int height, int depth, float scale, float offsetX, float offsetY)
    {
        TerrainData data = new TerrainData();
        data.heightmapResolution = width;
        data.size = new Vector3(width, depth, height);
        data.SetHeights(0, 0, generateHeights(width, height, scale, offsetX, offsetY));
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
                        Debug.LogWarning(debugString + " - " + terrainObject.name);
                        break;
                    }
                }
                if (locationValid)
                {
                    details[currentDetailCount] = (GameObject)Instantiate(prefab, new Vector3(x + terrain.transform.position.x, y + detailHeight, z + terrain.transform.position.z),
                        Quaternion.identity);//, 3);
                    details[currentDetailCount].transform.SetParent(terrain.transform);
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
                splatWeights[0] = 0.1f;

                // Texture[1] is stronger at higher altitudes //
                splatWeights[1] = height - .5f;

                // Texture[2] stronger on steeper terrain //
                splatWeights[2] = steepness / terrainData.heightmapHeight;

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
    private float[,] generateHeights(int width, int height, float scale, float offsetX, float offsetY)
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float assignedHeight = CalculateHeight(x, y, width, height, scale, offsetX, offsetY);
                heights[x, y] = assignedHeight;
            }
        }
        return heights;
    }
    private void fixGaps(int startSmoothingAt)
    {
        for (int terrainCount = 0; terrainCount < terrain.Length; terrainCount++) // Loop through each piece of terrain
        {
            Terrain singleTerrain = this.terrain[terrainCount].getTerrainComponenet();
            Terrain[] neighbors = terrain[terrainCount].neighbors;
            for (int nCount = 0; nCount < neighbors.Length; nCount++)
            {
                if (nCount != 0 && nCount != 3) continue;
                float[] seamStartPoints = new float[tileSize];
                float[,] thisTerrainsEdge = null;
                float[,] targetTerrainsEdge = null;

                Vector2 startPoint = Vector2.zero;// Start position to insert changed values
                Vector2 targetStartPoint = Vector2.zero;// Start position to insert changed values
                if (neighbors[nCount] != null)
                {
                    int height, width;
                    width = tileSize / startSmoothingAt;
                    height = tileSize;

                    // LEFT
                    if (nCount == 0)
                    {
                        startPoint = new Vector2(0, 0); // Where to insert new heights
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(0, 0, width, height);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(tileSize, 0, 1, height);
                    }
                    else if (nCount == 3)
                    {
                        // WIDTH/HEIGHT REVERSED //
                        startPoint = new Vector2(0, 0);
                        thisTerrainsEdge = singleTerrain.terrainData.GetHeights(0, 0, tileSize, width);
                        targetTerrainsEdge = neighbors[nCount].terrainData.GetHeights(0, tileSize, tileSize, 1);
                    }
                    // CALCULATE SEAMS
                    for (int countLong = 0; countLong < tileSize; countLong++)
                    {
                        if (nCount == 0)
                        {
                            seamStartPoints[countLong] = targetTerrainsEdge[countLong, 0];
                        }
                        else if (nCount == 3)
                        {
                            seamStartPoints[countLong] = targetTerrainsEdge[0, countLong];
                        }
                    }
                    if (nCount == 0)
                    {
                        seam(thisTerrainsEdge, seamStartPoints);
                    }
                    else if (nCount == 3)
                    {
                        thisTerrainsEdge = swapDimensions(thisTerrainsEdge);
                        seam(thisTerrainsEdge, seamStartPoints);
                        thisTerrainsEdge = swapDimensions(thisTerrainsEdge);
                    }
                    singleTerrain.terrainData.SetHeights((int)startPoint.x, (int)startPoint.y, thisTerrainsEdge);
                }
            }
            terrain[terrainCount].getTerrainComponenet().Flush();
        }
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
    private void seam(float[,] edge, float[] seamPoints)
    {
        int longSize = edge.GetLength(0);
        int shortSize = edge.GetLength(1);

        for (int longCount = 0; longCount < longSize; longCount++)
        {
            float step = (seamPoints[longCount] - edge[longCount, shortSize - 1]) / shortSize;
            for (int shortCount = 0; shortCount < shortSize; shortCount++)
            {
                if (shortCount > 0)
                    edge[longCount, shortCount] = edge[longCount, shortCount - 1] - step;
                else
                    edge[longCount, shortCount] = seamPoints[longCount];
            }
        }
    }
    private float CalculateHeight(int x, int y, int width, int height, float scale, float offsetX, float offsetY)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;
        float perlinResult = Mathf.PerlinNoise(xCoord, yCoord);
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
        returnVector.x = width*4;
        returnVector.z = height*4;
        returnVector.y = currentBiome.depth;
        return returnVector;
    }
    public int getSquareSize()
    {
        return (worldSize / 4) * height;
    }
    public RAKTerrain GetClosestTerrainToPoint(Vector3 point)
    {
        RAKTerrain closest = null;
        float closestDistance = 20000;
        foreach (RAKTerrain item in terrain)
        {
            float distance = Vector3.Distance(point, item.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = item;
            }
        }
        return closest;
    }

    [Serializable]
    public class RAKTerrainSavedData
    {
        private float[] alphaMapFlat;
        private float[] heightMapFlat;
        public static int bufferSize = 1024*4;
        public RAKVector3 tdSize;
        public RAKSplatPrototype[] splatPrototypes;
        public RAKTreeProtoType[] treeProtoTypes;
        public RAKTreeInstance[] trees;
        public RAKTerrainObjectSaveData[] nonTerrainObjectsSaveData;
        public RAKTerrainSavedData() { }
        public RAKTerrainSavedData(float[,,] alphaMap, float[,] heightMap, SplatPrototype[] splatPrototypes, TreePrototype[] treePrototypes, TreeInstance[] trees
            , RAKTerrainObject[] nonTerrainObjects,Vector3 terraindDataSize)
        {
            this.alphaMapFlat = new float[alphaMap.Length];
            this.tdSize = new RAKVector3(terraindDataSize); 
            int flatCount = 0;
            for (int x = 0; x < alphaMap.GetLength(0); x++)
            {
                for (int y = 0; y < alphaMap.GetLength(1); y++)
                {
                    for (int z = 0; z < alphaMap.GetLength(2); z++)
                    {
                        this.alphaMapFlat[flatCount] = alphaMap[x, y, z];
                        flatCount++;
                    }
                }
            }
            this.heightMapFlat = new float[heightMap.Length];
            flatCount = 0;
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    heightMapFlat[flatCount] = heightMap[x, y];
                    flatCount++;
                }
            }
            
            this.splatPrototypes = new RAKSplatPrototype[splatPrototypes.Length];
            this.treeProtoTypes = new RAKTreeProtoType[treePrototypes.Length];
            this.trees = new RAKTreeInstance[trees.Length];
            for (int count = 0; count < splatPrototypes.Length; count++)
            {
                this.splatPrototypes[count] = new RAKSplatPrototype(splatPrototypes[count].texture.name, splatPrototypes[count].tileSize);
            }
            for (int count = 0; count < treeProtoTypes.Length; count++)
            {
                this.treeProtoTypes[count] = new RAKTreeProtoType(treePrototypes[count]);
            }
            for (int count = 0; count < trees.Length; count++)
            {
                this.trees[count] = new RAKTreeInstance(trees[count]);
            }
            this.nonTerrainObjectsSaveData = new RAKTerrainObjectSaveData[nonTerrainObjects.Length];
            for (int count = 0; count < nonTerrainObjects.Length; count++)
            {
                this.nonTerrainObjectsSaveData[count] = RAKTerrainObjectSaveData.createSaveData(nonTerrainObjects[count]);
            }
        }
        public TerrainData generateTerrainDataFromFlatMap()
        {
            TerrainData td = new TerrainData();
            td.splatPrototypes = getSplatProtoTypes();
            float[,,] alphaMap = new float[(int)tdSize.x - 1, (int)tdSize.z - 1, splatPrototypes.Length];
            int flatCount = 0;
            for (int x = 0; x < alphaMap.GetLength(0); x++)
            {
                for (int z = 0; z < alphaMap.GetLength(1); z++)
                {
                    for (int y = 0; y < splatPrototypes.Length; y++)
                    {
                        alphaMap[x, z, y] = alphaMapFlat[flatCount];
                        flatCount++;
                    }
                }
            }
            td.SetAlphamaps(0, 0, alphaMap);
            float[,] heightMap = new float[(int)(tdSize.x), ((int)tdSize.z)];
            flatCount = 0;
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    heightMap[x, y] = heightMapFlat[flatCount];
                    flatCount++;
                }
            }
            td.heightmapResolution = heightMap.GetLength(0);
            td.size = new Vector3(tdSize.x, tdSize.y, tdSize.z);
            td.SetHeights(0, 0, heightMap);
            td.treePrototypes = getTreeProtoTypes();
            td.treeInstances = getTreeInstances();
            return td;
        }

        private static bool saveTerrainObject(RAKTerrainSavedData terrain, string path)
        {
            binarySerialize(path, terrain);
            return true;
        }
        public static bool saveTerrainBytes(RAKTerrain terrain, string path)
        {
            TerrainData data = terrain.getTerrainData();
            float[,,] alphaMap = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
            float[,] heightMap = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);
            SplatPrototype[] splats = data.splatPrototypes;
            Debug.LogWarning("Saving terrain");
            
            Vector3 terrainDataSize = terrain.terrain.terrainData.size;
            RAKTerrainSavedData saveData = new RAKTerrainSavedData(alphaMap, heightMap, splats, data.treePrototypes, 
                data.treeInstances, terrain.nonTerrainObjects,terrainDataSize);
            saveTerrainObject(saveData, path);
            terrain.savedData = saveData;
            return true;
        }
        public static void binarySerialize(string serializationFile, System.Object objectToSerialize)
        {
            //serialize
            using (Stream stream = File.Open(serializationFile, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, objectToSerialize);
            }
        }
        public static RAKTerrainSavedData loadTerrain(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (RAKTerrainSavedData)bformatter.Deserialize(stream);
            }
        }

        public bool appendToArray(bool isAlphaNotHeight,float[] data,int startIndex)
        {
            if (isAlphaNotHeight)
                Array.Copy(data, 0, alphaMapFlat, startIndex, data.Length);
            else
                Array.Copy(data, 0, heightMapFlat, startIndex, data.Length);
            if (isAlphaNotHeight)
                return startIndex + data.Length >= alphaMapFlat.Length;
            else
                return startIndex + data.Length >= heightMapFlat.Length+(tdSize.x-1*tdSize.z-1);// Height map has an extra element
        }

        public float[] getAlphaMapFlat()
        {
            return alphaMapFlat;
        }
        public float[] getHeightMapFlat()
        {
            return heightMapFlat;
        }
        private SplatPrototype[] getSplatProtoTypes()
        {
            SplatPrototype[] returnArray = new SplatPrototype[splatPrototypes.Length];
            for (int count = 0; count < splatPrototypes.Length; count++)
            {
                returnArray[count] = new SplatPrototype();
                returnArray[count].tileSize = splatPrototypes[count].getTileSize();
                returnArray[count].texture = splatPrototypes[count].getTexture();
            }
            return returnArray;
        }
        private TreePrototype[] getTreeProtoTypes()
        {
            TreePrototype[] returnArray = new TreePrototype[treeProtoTypes.Length];
            for (int count = 0; count < treeProtoTypes.Length; count++)
            {
                returnArray[count] = treeProtoTypes[count].getTreePrototype();
            }
            return returnArray;
        }
        private TreeInstance[] getTreeInstances()
        {
            TreeInstance[] returnArray = new TreeInstance[trees.Length];
            for (int count = 0; count < trees.Length; count++)
            {
                returnArray[count] = trees[count].getTreeInstance();
            }
            return returnArray;
        }
        [Serializable]
        public class RAKVector3
        {
            public float x, y, z;
            public RAKVector3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public RAKVector3(Vector3 vector3)
            {
                this.x = vector3.x;
                this.y = vector3.y;
                this.z = vector3.z;
            }
            public RAKVector3(Quaternion rotation)
            {
                Vector3 euler = rotation.eulerAngles;
                this.x = euler.x;
                this.y = euler.y;
                this.z = euler.z;
            }
            public Quaternion getRotation()
            {
                return Quaternion.Euler(x, y, z);
            }
            public Vector3 getVector3()
            {
                return new Vector3(x, y, z);
            }
            public RAKVector3() { }
        }
        [Serializable]
        public class RAKSplatPrototype
        {
            public string textureName;
            public RAKVector3 tileSize;

            public RAKSplatPrototype(string textureName, Vector2 tileSize)
            {
                this.textureName = textureName;
                this.tileSize = new RAKVector3(tileSize.x, tileSize.y, 0);
            }
            public Texture2D getTexture()
            {
                return (Texture2D)Resources.Load("Textures/" + textureName);
            }
            public Vector2 getTileSize()
            {
                return new Vector2(tileSize.x, tileSize.y);
            }
            public RAKSplatPrototype() { }
        }
        [Serializable]
        public class RAKTreeProtoType
        {
            public float bendFactor;
            public string prefabName;

            public RAKTreeProtoType(TreePrototype tree)
            {
                this.bendFactor = tree.bendFactor;
                this.prefabName = tree.prefab.name;
            }
            public TreePrototype getTreePrototype()
            {
                TreePrototype tree = new TreePrototype();
                tree.bendFactor = bendFactor;
                tree.prefab = (GameObject)Resources.Load("Prefabs/" + prefabName);
                return tree;
            }
            public RAKTreeProtoType() { }
        }
        [Serializable]
        public class RAKTreeInstance
        {
            public byte[] color = new byte[4];
            public byte[] lightingColor = new byte[4];
            public float heightScale;
            public float widthScale;
            public RAKVector3 position;
            public int protoypeIndex;

            public TreeInstance getTreeInstance()
            {
                TreeInstance instance = new TreeInstance();
                instance.color = new Color32(color[3], color[2], color[1], color[0]);
                instance.lightmapColor = new Color32(lightingColor[3], lightingColor[2], lightingColor[1], lightingColor[0]);
                instance.heightScale = heightScale;
                instance.widthScale = widthScale;
                instance.position = new Vector3(position.x, position.y, position.z);
                instance.prototypeIndex = protoypeIndex;
                return instance;
            }

            public RAKTreeInstance(TreeInstance tree)
            {
                color[0] = tree.color.a;
                color[1] = tree.color.b;
                color[2] = tree.color.g;
                color[3] = tree.color.r;
                lightingColor[0] = tree.lightmapColor.a;
                lightingColor[1] = tree.lightmapColor.b;
                lightingColor[2] = tree.lightmapColor.g;
                lightingColor[3] = tree.lightmapColor.r;
                this.heightScale = tree.heightScale;
                this.widthScale = tree.widthScale;
                this.position = new RAKVector3(tree.position.x, tree.position.y, tree.position.z);
                this.protoypeIndex = tree.prototypeIndex;
            }
            public RAKTreeInstance() { }
        }
    }
}