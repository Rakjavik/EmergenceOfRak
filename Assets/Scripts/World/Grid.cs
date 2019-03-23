using rak.world;
using UnityEngine;

public class Grid
{
    public static readonly int ELEMENT_SIZE_DIVIDER = 2;
    public static Vector2 CurrentElementSize;


    private static GridSector[] elements;
    private static int numberOfXElements;
    private static int numberOfZElements;
    private static Vector3 terrainSize;

    public Grid(RAKTerrain terrain)
    {

        terrainSize = terrain.terrain.terrainData.size;
        CurrentElementSize = new Vector2(terrainSize.x/ELEMENT_SIZE_DIVIDER,terrainSize.z/ELEMENT_SIZE_DIVIDER);
        numberOfXElements = (int)(terrainSize.x / CurrentElementSize.x);
        numberOfZElements = (int)(terrainSize.z / CurrentElementSize.y);
        elements = new GridSector[numberOfXElements * numberOfZElements];
        int elementCount = 0;
        Vector3 terrainPosition = terrain.transform.position;
        for(int x = 0; x < numberOfXElements; x++)
        {
            for(int z = 0; z < numberOfZElements; z++)
            {
                Vector2 elementWorldPosition = new Vector2(x * CurrentElementSize.x,
                    z * CurrentElementSize.y);
                Vector2 elementWorldPositionEnd = new Vector2(
                    elementWorldPosition.x + CurrentElementSize.x, elementWorldPosition.y + CurrentElementSize.y);
                elements[elementCount] = new GridSector(new Vector2(x, z),
                    elementWorldPosition, elementWorldPositionEnd,terrain.name,terrain);
                elementCount++;
            }
        }
        Debug.Log("Grid generation complete");
    }
    public static GridSector GetGridSectorAt(Vector3 globalPosition)
    {
        Vector2 relativePosition = new Vector2(globalPosition.x % RAKTerrainMaster.TileSize,
            globalPosition.z % RAKTerrainMaster.TileSize);
        int xElement = (int)(relativePosition.x / relativePosition.x);
        int yElement = (int)(relativePosition.y / relativePosition.y);
        int index = (xElement * numberOfZElements) + yElement;
        Debug.LogWarning(xElement + "-" + yElement);
        return elements[index];
    }
    public GridSector[] GetGridElements() { return elements; }
}
