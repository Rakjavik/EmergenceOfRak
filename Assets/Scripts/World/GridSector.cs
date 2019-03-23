using rak;
using UnityEngine;

public class GridSector
{
    public Vector2 relativePositionStart { get; private set; }
    public Vector2 relativeWorldPositionEnd { get; private set; }
    public string name { get; private set; }
    public Vector2 gridPosition { get; private set; }

    private float[,] terrainHeights;
    private RAKTerrain parentTerrain = null;

    public GridSector(Vector2 gridPosition, Vector2 relativePositionStart, 
        Vector2 worldPositionEnd,string terrainName,RAKTerrain terrain)
    {
        name = (terrainName + "-" + gridPosition.x + "-" + gridPosition.y);
        this.gridPosition = gridPosition;
        this.relativePositionStart = relativePositionStart;
        this.relativeWorldPositionEnd = worldPositionEnd;
        this.parentTerrain = terrain;
        float y = 0;
        Vector3 start = new Vector3(
            relativePositionStart.x%Grid.CurrentElementSize.x, y, relativePositionStart.y % Grid.CurrentElementSize.y);
        Vector3 end = new Vector3(
            worldPositionEnd.x % Grid.CurrentElementSize.x, y, worldPositionEnd.y % Grid.CurrentElementSize.y);
        Debug.LogWarning("Start-End -- " + start + "-" + end);
        terrainHeights = terrain.terrain.terrainData.GetHeights((int)relativePositionStart.x, (int)relativePositionStart.y,
            (int)Grid.CurrentElementSize.x, (int)Grid.CurrentElementSize.y);
        Debug.DrawLine(start, new Vector3(end.x,y,start.z), Color.yellow, 3);
        Debug.DrawLine(end, new Vector3(end.x, y, start.z), Color.yellow, 3);
    }
    public float GetTerrainHeightFromGlobalPos(Vector3 globalPosition)
    {
        Vector2 flatten = new Vector2(
            globalPosition.x%RAKTerrainMaster.TileSize, globalPosition.z % RAKTerrainMaster.TileSize);
        Vector2 diff = flatten - relativePositionStart;
        int x = (int)diff.x;
        int y = (int)diff.y;
        if(x >= terrainHeights.GetLength(0) || y >= terrainHeights.GetLength(1) || y < 0 || x < 0)
        {
            Debug.LogWarning("X-Y - " + x + "-" + y);
            return 0;
        }
        return terrainHeights[x, y];
    }
    public Vector3 GetRandomPositionInSector { get
        {
            float x = Random.Range(0, Grid.CurrentElementSize.x);
            float z = Random.Range(0, Grid.CurrentElementSize.y);
            float y = parentTerrain.GetHeightAt(new Vector2(relativePositionStart.x + x, relativePositionStart.y + z));
            return new Vector3(relativePositionStart.x+x, y, relativePositionStart.y+z);
        } }
    public Vector3 GetSectorPosition { get
        {
            Vector3 position = new Vector3(relativePositionStart.x+(Grid.CurrentElementSize.x/2), 6, relativePositionStart.y + 
                (Grid.CurrentElementSize.y / 2));
            //Debug.LogWarning("Position of sector - " + heightY);
            return position;
        } }
    

    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(relativePositionStart, relativeWorldPositionEnd,.5f);
    }
}
