using rak.world;
using Unity.Mathematics;
using UnityEngine;

public struct Grid
{
    public static readonly int ELEMENT_SIZE_DIVIDER = 2;
    public static float2 CurrentElementSize;


    private GridSector[] elements;
    private int numberOfXElements;
    private int numberOfZElements;
    private float3 terrainSize;

    public Grid(RAKTerrain terrain)
    {
        if (terrain == null)
        {
            elements = new GridSector[0];
            numberOfXElements = 0;
            numberOfZElements = 0;
            terrainSize = float3.zero;
        }
        else
        {
            terrainSize = terrain.terrain.terrainData.size;
            CurrentElementSize = new float2((int)(terrainSize.x / ELEMENT_SIZE_DIVIDER),
                (int)(terrainSize.z / ELEMENT_SIZE_DIVIDER));
            numberOfXElements = (int)((terrainSize.x) / CurrentElementSize.x);
            numberOfZElements = (int)((terrainSize.z) / CurrentElementSize.y);
            elements = new GridSector[numberOfXElements * numberOfZElements];
            int elementCount = 0;
            float3 terrainPosition = terrain.transform.position;
            for (int x = 0; x < numberOfXElements; x++)
            {
                for (int z = 0; z < numberOfZElements; z++)
                {
                    float3 elementWorldPosition = new float3(
                        terrain.transform.position.x + (x * CurrentElementSize.x),
                        0,
                        terrain.transform.position.z + (z * CurrentElementSize.y));
                    float3 elementWorldPositionEnd = new float3(
                        elementWorldPosition.x + CurrentElementSize.x,
                        0,
                        elementWorldPosition.z + CurrentElementSize.y);
                    elements[elementCount] = new GridSector(new Coord2(x, z), elementWorldPosition, elementWorldPositionEnd,
                        terrain.name, terrain);
                    elementCount++;
                }
            }
        }
        Debug.Log("Grid generation complete");
    }

    public static Grid Empty { get
        {
            return new Grid(null);
        } }
    public bool IsEmpty()
    {
        if (elements.Length == 0)
            return true;
        return false;
    }

    public GridSector GetGridSectorAt(float3 globalPosition)
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
        return GridSector.Empty;
    }
    public GridSector[] GetGridElements() { return elements; }
}
