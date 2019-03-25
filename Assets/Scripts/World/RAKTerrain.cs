using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DigitalRuby.RainMaker;
using UnityEngine.AI;
using rak;
using rak.world;

[Serializable]
public class RAKTerrain : MonoBehaviour
{
    public Terrain terrain;
    public Terrain[] neighbors { get; private set; }
    public RAKTerrain[] rakNeighbors;
    public RAKTerrainObject[] nonTerrainObjects { get; set; }
    public RAKTerrainMaster.RAKTerrainSavedData savedData { get; set; }
    public Guid guid { get; private set; }

    private RAKTree[] rakTrees;
    private RAKTerrainMaster.RAKBiome biome;
    private Grid grid;
    private RAKTerrainMaster terrainMaster;
    private Dictionary<Guid, GridSector> sectorsList;

    public GridSector GetGridSectorByGUID(Guid guid)
    {
        return sectorsList[guid];
    }
    public void AddGridSectorHash(GridSector sector)
    {
        sectorsList.Add(sector.guid, sector);
    }
    public void initialize(RAKTerrainMaster terrainMaster)
    {
        Debug.LogWarning("Initialize " + name);
        this.terrainMaster = terrainMaster;
        terrain = GetComponent<Terrain>();
        rakNeighbors = new RAKTerrain[4];
        neighbors = new Terrain[4];
        grid = Grid.Empty;
        guid = Guid.NewGuid();
        sectorsList = new Dictionary<Guid, GridSector>();
    }
    
    public GridSector GetSectorAtPos(Vector3 position)
    {
        GridSector[] sectors = GetGridElements();
        for (int sectorCount = 0; sectorCount < sectors.Length; sectorCount++)
        {
            if (position.x > sectors[sectorCount].WorldPositionStart.x &&
                position.x < sectors[sectorCount].WorldPositionEnd.x)
            {
                if (position.z > sectors[sectorCount].WorldPositionStart.z &&
                position.z < sectors[sectorCount].WorldPositionEnd.z)
                {
                    return sectors[sectorCount];
                }
            }
        }
        return GridSector.Empty;
    }
    public GridSector[] GetThisGridAndNeighborGrids()
    {
        List<GridSector> sectors = new List<GridSector>(GetGridElements());
        for(int count = 0; count < rakNeighbors.Length; count++)
        {
            if (rakNeighbors[count] == null) continue;
            GridSector[] neighborSectors = rakNeighbors[count].GetGridElements();
            for (int sectorCount = 0; sectorCount < neighborSectors.Length; sectorCount++)
            {
                if(!neighborSectors[sectorCount].IsEmpty())
                {
                    sectors.Add(neighborSectors[sectorCount]);
                }
            }
        }
        return sectors.ToArray();
    }
    public void SetNeighbor(RAKTerrain neighbor,int direction)
    {
        if (neighbor != null)
        {
            Debug.LogWarning("Direction-" + direction);
            rakNeighbors[direction] = neighbor;
            Terrain neighborTerrain = neighbor.getTerrainComponenet();
            if (neighborTerrain != null)
                this.neighbors[direction] = neighbor.getTerrainComponenet();
            else
                Debug.LogWarning(direction + " has null terrain component");
        }
    }
    public void InitializeGrid()
    {
        grid = new Grid(this);
    }
    public void generateTreeGOs()
    {
        TreeInstance[] trees = terrain.terrainData.treeInstances;
        this.rakTrees = new RAKTree[trees.Length];
        for (int count = 0; count < trees.Length; count++)
        {
            this.rakTrees[count] = new RAKTree(this);
            Vector3 thisTreePos = Vector3.Scale(trees[count].position, terrain.terrainData.size) + transform.position;
            this.rakTrees[count].worldPoint = thisTreePos;
        }
    }
    private void setNeighbors(RAKTerrain terrainLeft, RAKTerrain terrainRight, RAKTerrain terrainUp, RAKTerrain terrainDown)
    {
        rakNeighbors = new RAKTerrain[] { terrainLeft, terrainRight, terrainUp, terrainDown };
        terrain.SetNeighbors(terrainLeft.terrain, terrainUp.terrain, terrainRight.terrain, terrainDown.terrain);
        neighbors = new Terrain[] { terrainLeft.terrain, terrainRight.terrain, terrainUp.terrain, terrainDown.terrain };
        Debug.Log(name + " Neighbors LRUD - LEFT " + terrainLeft + "-RIGHT " + terrainRight + "-UP " + terrainUp + "-DOWN" + terrainDown);
    }
    
    public TerrainData getTerrainData()
    {
        return GetComponent<Terrain>().terrainData;
    }
    public void setTerrainData(TerrainData data)
    {
        terrain.terrainData = data;
    }
    public Terrain getTerrainComponenet()
    {
        return terrain;
    }
    public void updateWorldPositions()
    {
        foreach (RAKTree tree in rakTrees)
        {
            tree.worldPoint = transform.position + tree.worldPoint;
        }
    }
    public void generateDetails(int numberOfDetails,GameObject[] detailPrefabs)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.SetDetailResolution(terrainData.heightmapWidth, terrainData.heightmapHeight);
        DetailPrototype[] prototypes = new DetailPrototype[detailPrefabs.Length];
        for (int count = 0; count < prototypes.Length; count++)
        {
            DetailPrototype prototype = new DetailPrototype();
            prototype.prototype = detailPrefabs[count];
            prototype.renderMode = DetailRenderMode.VertexLit;
            prototype.usePrototypeMesh = true;
            prototype.minWidth = 1f;
            prototype.maxWidth = 4f;
            prototype.minHeight = 1f;
            prototype.maxHeight = 4f;
            prototypes[count] = prototype;
        }
        terrainData.detailPrototypes = prototypes;
        float maxSteepness = 15;
        int fails = 0;
        int[,] detailMap = new int[terrainData.heightmapWidth-1, terrainData.heightmapHeight-1];
        for (int count = 0; count < numberOfDetails; count++)
        {
            int protoIndex = UnityEngine.Random.Range(0, detailPrefabs.Length);
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
            
            if (isTooCloseToOtherDetail(new Vector2(xPosition, zPosition), detailMap))
            {
                valid = false;
            }
            if (valid)
            {
                Debug.Log("Placing detail - " + xPosition + "-" + zPosition);
                detailMap[(int)xPosition,(int) zPosition] = 1;
            }
            else
            {
                count--;
                fails++;
                if (fails > 50)
                {
                    Debug.LogError("Can't find a spot for a detail, screw this, im going home");
                    break;
                }
            }
        }
        terrainData.SetDetailLayer(0, 0, 0, detailMap);
    }
    private bool isTooCloseToOtherDetail(Vector2 newDetail, int[,] otherDetails)
    {
        float minDistance = 5f;
        float closest = 5000;
        for(int x = 0; x < otherDetails.GetLength(0); x++)
        {
            for(int y = 0; y < otherDetails.GetLength(1);y++)
            {
                float distance = Vector2.Distance(newDetail, new Vector2(x,y));
                if (distance < closest && otherDetails[x,y] == 1)
                {
                    closest = distance;
                }
            }
            
        }
        return closest < minDistance;
    }
    public void setBiome(RAKTerrainMaster.RAKBiome biome)
    {
        this.biome = biome;
    }
    public RAKTerrainMaster.RAKBiome getBiome()
    {
        return biome;
    }
    public float GetHeightAt(Vector3 globalPosition)
    {
        int x = (int)(globalPosition.x % RAKTerrainMaster.TileSize);
        int z = (int)(globalPosition.z % RAKTerrainMaster.TileSize);
        float height = terrain.terrainData.GetHeight(x, z);
        return height;
    }
    private void createPlayerRainWithIntensity(float intensity,RAKPlayer player)
    {
        RainScript rain = Instantiate(terrainMaster.getPlayerRainPrefab(), player.transform).GetComponent<RainScript>();
        rain.FollowCamera = player.GetComponent<Camera>();
        rain.RainIntensity = intensity;
    }
    public GridSector[] GetGridElements()
    {
        return grid.GetGridElements();
    }
    public Vector3 GetCenterOfTerrain()
    {
        Vector3 returnVector = new Vector3(transform.position.x + RAKTerrainMaster.TileSize / 2, 0,
            transform.position.z + RAKTerrainMaster.TileSize / 2);
        return returnVector;
    }
}

public class RAKTree
{
    public Vector3 worldPoint { get; set; }

    private RAKTerrain terrain;

    public RAKTree(RAKTerrain terrain)
    {
        this.terrain = terrain;
    }
}