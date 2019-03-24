using rak.world;
using UnityEngine;

public class Grid
{
    public static readonly int ELEMENT_SIZE_DIVIDER = 2;
    public static Vector2 CurrentElementSize;


    private GridSector[] elements;
    private int numberOfXElements;
    private int numberOfZElements;
    private Vector3 terrainSize;

    public Grid(RAKTerrain terrain)
    {

        terrainSize = terrain.terrain.terrainData.size;
        CurrentElementSize = new Vector2((int)(terrainSize.x/ELEMENT_SIZE_DIVIDER),
            (int)(terrainSize.z/ELEMENT_SIZE_DIVIDER));
        numberOfXElements = (int)((terrainSize.x) / CurrentElementSize.x);
        numberOfZElements = (int)((terrainSize.z) / CurrentElementSize.y);
        elements = new GridSector[numberOfXElements * numberOfZElements];
        int elementCount = 0;
        Vector3 terrainPosition = terrain.transform.position;
        for(int x = 0; x < numberOfXElements; x++)
        {
            for(int z = 0; z < numberOfZElements; z++)
            {
                Vector3 elementWorldPosition = new Vector3(
                    terrain.transform.position.x + (x * CurrentElementSize.x),
                    0,
                    terrain.transform.position.z + (z * CurrentElementSize.y));
                Vector3 elementWorldPositionEnd = new Vector3(
                    elementWorldPosition.x+CurrentElementSize.x,
                    0,
                    elementWorldPosition.z+CurrentElementSize.y);
                elements[elementCount] = new GridSector(new Vector2(x,z),elementWorldPosition,elementWorldPositionEnd,
                    terrain.name,terrain);
                elementCount++;
            }
        }
        Debug.Log("Grid generation complete");
    }
    public GridSector GetGridSectorAt(Vector3 globalPosition)
    {
        float globalX = globalPosition.x;
        float globalZ = globalPosition.z;
        for(int count = 0; count < elements.Length; count++)
        {
            if(globalX > elements[count].WorldPositionStart.x && globalX < elements[count].WorldPositionEnd.x)
            {
                if (globalZ > elements[count].WorldPositionStart.z && globalZ < elements[count].WorldPositionEnd.z)
                {
                    return elements[count];
                }
            }
        }
        return null;
    }
    public GridSector[] GetGridElements() { return elements; }
}
