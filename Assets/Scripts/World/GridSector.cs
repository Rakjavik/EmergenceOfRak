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
        this.WorldPositionStart = worldPositionStart;
        this.WorldPositionEnd = worldPositionEnd;
        this.parentTerrain = terrain;
        float y = 0;
        Vector3 start = new Vector3(
            worldPositionStart.x%Grid.CurrentElementSize.x, y, worldPositionStart.y % Grid.CurrentElementSize.y);
        Vector3 end = new Vector3(
            worldPositionEnd.x % Grid.CurrentElementSize.x, y, worldPositionEnd.y % Grid.CurrentElementSize.y);
        Debug.LogWarning("Getting heights for sector - " + name);
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionEnd.x,0,WorldPositionStart.z),
            Color.yellow, 1);
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionStart.x, 0, WorldPositionEnd.z),
            Color.yellow, 1);
    }

    public Vector3 GetRandomPositionInSector { get
        {
            float x = Random.Range(0, Grid.CurrentElementSize.x);
            float z = Random.Range(0, Grid.CurrentElementSize.y);
            float y = parentTerrain.GetHeightAt(new Vector2(WorldPositionStart.x + x, WorldPositionStart.y + z));
            return new Vector3(WorldPositionStart.x+x, y, WorldPositionStart.y+z);
        } }
    public Vector3 GetSectorPosition { get
        {
            Vector3 position = new Vector3(WorldPositionStart.x+(Grid.CurrentElementSize.x/2), 6, WorldPositionStart.y + 
                (Grid.CurrentElementSize.y / 2));
            //Debug.LogWarning("Position of sector - " + heightY);
            return position;
        } }
    
    public float GetTerrainHeightFromGlobalPos(Vector3 position)
    {
        return parentTerrain.GetHeightAt(position);
    }
    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(WorldPositionStart, WorldPositionEnd,.5f);
    }
}
