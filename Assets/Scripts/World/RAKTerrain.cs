using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DigitalRuby.RainMaker;
using UnityEngine.AI;

[Serializable]
public class RAKTerrain : MonoBehaviour
{
    private RAKTerrainMaster terrainMaster;
    public Terrain terrain;
    public Terrain[] neighbors { get; set; }
    public RAKTerrainObject[] nonTerrainObjects { get; set; }
    public RAKTerrainMaster.RAKTerrainSavedData savedData { get; set; }

    private RAKTree[] rakTrees;
    private RAKTerrainMaster.RAKBiome biome;
    

    public void initialize(RAKTerrainMaster terrainMaster)
    {
        this.terrainMaster = terrainMaster;
        terrain = GetComponent<Terrain>();
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
    public void setNeighbors(Terrain terrainLeft,Terrain terrainRight,Terrain terrainUp,Terrain terrainDown)
    {
        terrain.SetNeighbors(terrainLeft, terrainUp, terrainRight, terrainDown);
        neighbors = new Terrain[] { terrainLeft, terrainRight, terrainUp, terrainDown };
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
    public float GetHeightAt(Vector2 position)
    {
        float height = terrain.terrainData.GetInterpolatedHeight(position.x, position.y);
        //Debug.LogWarning("Interpolated height - " + height + " at position " + position);
        return height;
    }
    private void createPlayerRainWithIntensity(float intensity,RAKPlayer player)
    {
        RainScript rain = Instantiate(terrainMaster.getPlayerRainPrefab(), player.transform).GetComponent<RainScript>();
        rain.FollowCamera = player.GetComponent<Camera>();
        rain.RainIntensity = intensity;
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