using rak.creatures.memory;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ObserveJobFor : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<BlittableThing> allThings;
    [WriteOnly]
    public NativeArray<BlittableThing> thingsWithinReach;

    public Vector3 origin;
    public float observeDistance;

    public void Execute(int index)
    {
        float distanceFromThing = Vector3.Distance(allThings[index].position, origin);
        if (distanceFromThing <= observeDistance && distanceFromThing > 1)
        {
            thingsWithinReach[index] = allThings[index];
        }

    }
}

