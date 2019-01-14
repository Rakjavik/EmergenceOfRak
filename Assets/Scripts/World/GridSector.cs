using rak;
using UnityEngine;

public class Grid
{
    public static readonly Vector2 ELEMENT_SIZE = new Vector2(64, 64);

    private GridSector[] elements;

    public Grid(RAKTerrain terrain)
    {
        Vector3 terrainSize = terrain.terrain.terrainData.size;
        Debug.LogWarning(terrainSize);
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
        float y = 0;
        Vector3 start = new Vector3(worldPositionStart.x, y, worldPositionStart.y);
        Vector3 end = new Vector3(worldPositionEnd.x, y, worldPositionEnd.y);
        Debug.DrawLine(start, new Vector3(end.x,y,start.z), Color.yellow, 30);
        Debug.DrawLine(end, new Vector3(end.x, y, start.z), Color.yellow, 30);
    }

    public Vector2 gridPosition { get; private set; }
    private RAKTerrain parentTerrain = null;
    public Vector3 GetRandomPositionInSector { get
        {
            float x = Random.Range(0, Grid.ELEMENT_SIZE.x);
            float z = Random.Range(0, Grid.ELEMENT_SIZE.y);
            float y = parentTerrain.GetHeightAt(new Vector2(worldPositionStart.x + x, worldPositionStart.y + z));
            return new Vector3(worldPositionStart.x+x, y+3, worldPositionStart.y+z);
        } }
    public Vector3 GetSectorPosition { get
        {
            Vector3 position = new Vector3(worldPositionStart.x+(Grid.ELEMENT_SIZE.x/2), 6, worldPositionStart.y + 
                (Grid.ELEMENT_SIZE.y / 2));
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
