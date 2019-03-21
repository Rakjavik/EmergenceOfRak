using UnityEngine;
using System.Collections;
using Unity.Jobs;
using rak;
using Unity.Collections;
using System.Collections.Generic;
using rak.creatures.memory;
using rak.world;
using System;

public struct ObserveJob : IJob
{
    public float observeDistance;
    public Vector3 origin;
    [ReadOnly]
    public NativeArray<BlittableThing> allThings;
    [WriteOnly]
    public NativeArray<BlittableThing> thingsWithinReach;

    public void Execute()
    {
        int withinReachIndex = 0;
        for(int count = 0; count < allThings.Length; count++)
        {
            BlittableThing thing = allThings[count];
            float distanceFromThing = Vector3.Distance(thing.position, origin);
            if (distanceFromThing <= observeDistance && distanceFromThing > 1)
            {
                thingsWithinReach[withinReachIndex] = allThings[count];
                withinReachIndex++;
                if (withinReachIndex == thingsWithinReach.Length)
                {
                    break;
                }
            }   
        }
    }
}
