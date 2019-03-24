using rak;
using rak.world;
using UnityEngine;

public class GridSector
{
    public Vector3 WorldPositionStart { get; private set; }
    public Vector3 WorldPositionEnd { get; private set; }
    public string name { get; private set; }
    public Vector2 gridPosition { get; private set; }

    private RAKTerrain parentTerrain = null;

    public GridSector(Vector2 gridPosition, Vector3 worldPositionStart, 
        Vector3 worldPositionEnd,string terrainName,RAKTerrain terrain)
    {
        name = (terrainName + "-" + gridPosition.x + "-" + gridPosition.y);
        this.gridPosition = new Vector2((int)gridPosition.x,(int)gridPosition.y);
        this.WorldPositionStart = new Vector3(worldPositionStart.x,worldPositionStart.y,worldPositionStart.z);
        this.WorldPositionEnd = new Vector3(worldPositionEnd.x, worldPositionEnd.y, worldPositionEnd.z); ;
        this.parentTerrain = terrain;
        float y = 0;
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionEnd.x,0,WorldPositionStart.z),
            Color.yellow, 1);
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionStart.x, 0, WorldPositionEnd.z),
            Color.yellow, 1);
    }

    public Vector3 GetSectorPosition()
    { 
        Vector3 position = new Vector3(WorldPositionStart.x+(Grid.CurrentElementSize.x/2), 0, WorldPositionStart.z + 
            (Grid.CurrentElementSize.y / 2));
        //Debug.LogWarning("Position of sector terrain - " + parentTerrain.transform.position);
        //Debug.LogWarning("Returning position of " + position);
        return position;
    }
    
    public float GetTerrainHeightFromGlobalPos(Vector3 position)
    {
        return parentTerrain.GetHeightAt(position);
    }
    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(WorldPositionStart, WorldPositionEnd,.5f);
    }
}
