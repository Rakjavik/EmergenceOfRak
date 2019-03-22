using rak.creatures.memory;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ObserveJobFor : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<BlittableThing> allThings;
    [WriteOnly]
    public NativeArray<MemoryInstance> memories;

    public Vector3 origin;
    public float observeDistance;

    public void Execute(int index)
    {
        float distanceFromThing = Vector3.Distance(allThings[index].position, origin);
        if (distanceFromThing <= observeDistance && distanceFromThing > 1)
        {
            memories[index] = new MemoryInstance(Verb.SAW,allThings[index].GetGuid(), false);
        }
        else
        {
            memories[index] = MemoryInstance.GetNewEmptyMemory();
        }
    }
}

