using UnityEngine;
using System.Collections;
using rak.creatures;
using Unity.Jobs;
using rak;

public class ThingAgent
{
    private ThingAnimationPart[] parts;

    public ThingAgent(Thing thing)
    {
        parts = CreatureConstants.GetPartsForThingAgent(thing);
    }

    public void ManualUpdate(float delta)
    {
        int numberOfParts = parts.Length;
        for(int count = 0; count < numberOfParts; count++)
        {
            parts[count].ManualUpdate(delta);
        }
    }
}
