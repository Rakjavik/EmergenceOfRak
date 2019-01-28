﻿using rak;
using UnityEngine;

// A Grid is created for an Area to dissect it into sections //
public class Grid
{
    // Size of each sector X,Y //
    public static readonly Vector2 ELEMENT_SIZE = new Vector2(50, 50);

    private GridSector[] elements;
    
    public Grid(RAKTerrain terrain)
    {
        // Get the Unity Terrain object size //
        Vector3 terrainSize = terrain.terrain.terrainData.size;
        int numberOfXElements = (int)(terrainSize.x / ELEMENT_SIZE.x);
        int numberOfZElements = (int)(terrainSize.z / ELEMENT_SIZE.y);
        elements = new GridSector[numberOfXElements * numberOfZElements];
        int elementCount = 0;
        Vector3 terrainPosition = terrain.transform.position;
        for(int x = 0; x < numberOfXElements; x++)
        {
            for(int z = 0; z < numberOfZElements; z++)
            {
                Vector2 elementWorldPosition = new Vector2(terrainPosition.x + x * ELEMENT_SIZE.x,
                    terrainPosition.z + z * ELEMENT_SIZE.y);
                Vector2 elementWorldPositionEnd = new Vector2(
                    elementWorldPosition.x + ELEMENT_SIZE.x, elementWorldPosition.y + ELEMENT_SIZE.y);
                elements[elementCount] = new GridSector(new Vector2(x, z),
                    elementWorldPosition, elementWorldPositionEnd,terrain.name,terrain);
                elementCount++;
            }
        }
        Debug.Log("Grid generation complete");
    }

    public GridSector[] GetGridElements() { return elements; }
}

// Section of a grid //
public class GridSector
{
    public GridSector(Vector2 gridPosition, Vector2 worldPositionStart, 
        Vector2 worldPositionEnd,string terrainName,RAKTerrain terrain)
    {
        name = (terrainName + "-" + gridPosition.x + "-" + gridPosition.y);
        this.gridPosition = gridPosition;
        this.worldPositionStart = worldPositionStart;
        this.worldPositionEnd = worldPositionEnd;
        this.parentTerrain = terrain;
    }

    public Vector2 gridPosition { get; private set; }
    private RAKTerrain parentTerrain = null;
    public Vector3 GetSectorPosition { get
        {
            Vector3 position = new Vector3(worldPositionStart.x, 6, worldPositionStart.y);
            //Debug.LogWarning("Position of sector - " + heightY);
            return position;
        } }
    public Vector2 worldPositionStart { get; private set; }
    public Vector2 worldPositionEnd { get; private set; }
    public string name { get; private set; }

    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(worldPositionStart, worldPositionEnd,.5f);
    }
}
