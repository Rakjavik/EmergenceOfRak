using rak.creatures.memory;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/*public struct ObserveJobFor : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<BlittableThing> allThings;

    public NativeArray<MemoryInstance> memories;

    public float3 origin;
    public float observeDistance;
    public long timestamp;

    public void Execute(int index)
    {
        float distanceFromThing = Vector3.Distance(allThings[index].position, origin);
        if (distanceFromThing <= observeDistance && distanceFromThing > 1)
        {
        }
        else
        {
            memories[index].MakeEmpty();
        }
    }
}

public struct GetTerrainAtPointJob : IJobParallelFor
{
    [ReadOnly]
    public Vector3 point;
    [ReadOnly]
    public NativeArray<Vector3> terrainPositions;
    [ReadOnly]
    public int maxXZ;
    [ReadOnly]
    public int tileSize;
    [WriteOnly]
    public int terrainIndex;

    public void Execute(int index)
    {
        Vector3 terrainPos = terrainPositions[index];
        if (point.x < 0)
            point.x = 0;
        else if (point.x > maxXZ)
            point.x = maxXZ;
        if (point.z < 0)
            point.z = 0;
        else if (point.z > maxXZ)
            point.z = maxXZ;

        if (point.x >= terrainPos.x && point.x <= terrainPos.x + tileSize)
        {
            if (point.z >= terrainPos.z && point.z <= terrainPos.z + tileSize)
            {
                terrainIndex = index;
            }
        }
    }
    }*/

