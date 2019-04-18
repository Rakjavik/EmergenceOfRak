using rak.creatures;
using System.Collections.Generic;
using UnityEngine;

namespace rak.world
{
    public class CreatureUtilities
    {
        private static Dictionary<float, List<object>> thingsByUpdateInterval = new Dictionary<float, List<object>>();

        private static void createKeyIfDoesntExist(float key)
        {
            if (!thingsByUpdateInterval.ContainsKey(
                        key))
            {
                thingsByUpdateInterval.Add(key,
                    new List<object>());
            }
        }
        public static void OptimizeUpdateTimes(List<Thing> things)
        {
            int size = things.Count;

            foreach (Thing thing in things)
            {
                if (thing is Creature)
                {
                    Creature creature = (Creature)thing;
                    createKeyIfDoesntExist(creature.getCreatureStats().updateEvery);
                    thingsByUpdateInterval[creature.getCreatureStats().updateEvery].Add(creature);

                    Part[] parts = creature.GetCreatureAgent().GetAllParts();
                    foreach (Part part in parts)
                    {
                        float updateInterval = part.UpdateEvery;
                        createKeyIfDoesntExist(updateInterval);
                        thingsByUpdateInterval[updateInterval].Add(part);
                    }
                }
            }

            Debug.Log("Dictionary complete");
            foreach (float interval in thingsByUpdateInterval.Keys)
            {
                float currentInterval = 0;
                float increment = interval / thingsByUpdateInterval[interval].Count;
                for(int count = 0; count < thingsByUpdateInterval[interval].Count; count++)
                {
                    bool StaggerTimeApplied;
                    if (thingsByUpdateInterval[interval][count] is Creature)
                    {
                        Creature creature = (Creature)thingsByUpdateInterval[interval][count];
                        creature.SetUpdateStaggerTime(currentInterval);
                        StaggerTimeApplied = true;
                    }
                    else if (thingsByUpdateInterval[interval][count] is Part)
                    {
                        Part part = (Part)thingsByUpdateInterval[interval][count];
                        part.SetUpdateStaggerTime(currentInterval);
                        StaggerTimeApplied = true;
                    }
                    else
                        StaggerTimeApplied = false;
                    if (StaggerTimeApplied)
                        currentInterval += increment;
                }
            }
            Debug.Log("Updates have been staggered");
        }
        public static Thing[] GetThingsWithinProximityOf(Thing requester, float distance,Thing[] allThings)
        {
            List<Thing> returnList = new List<Thing>();
            foreach (Thing thing in allThings)
            {
                if (thing == requester) continue;
                Vector3 thingMatchingY = new Vector3(thing.transform.position.x, 
                    requester.transform.position.y, thing.transform.position.z);
                if (Vector3.Distance(thingMatchingY, requester.transform.position) <= distance)
                    returnList.Add(thing);
            }
            // DEBUG //
            /*if (returnList.Count > 0)
            {
                foreach(Thing inRange in returnList)
                    Debug.LogWarning("Things in prox - " + inRange.name + 
                        " dist-" + Vector3.Distance(requester.transform.position,inRange.transform.position));
            }*/

            return returnList.ToArray();
        }
        public static GridSector[] GetPiecesOfTerrainCreatureCanSee(Creature requester,float distance, 
            RAKTerrain terrain)
        {
            List<GridSector> elementsWithinRange = new List<GridSector>();
            if (terrain == null) Debug.Break();
            GridSector[] elements = terrain.GetGridElements();
            foreach (GridSector element in elements)
            {
                if(Vector3.Distance(requester.transform.position,element.GetSectorPosition()) <= distance)
                {
                    elementsWithinRange.Add(element);
                }
            }
            if (terrain.neighbors == null)
                return elementsWithinRange.ToArray();
            for(int count = 0; count < terrain.neighbors.Length; count++)
            {
                Terrain neighbor = terrain.neighbors[count];
                if (neighbor == null) continue;
                RAKTerrain neighborTerrain = neighbor.gameObject.GetComponent<RAKTerrain>();
                GridSector[] sectors = neighborTerrain.GetGridElements();
                foreach (GridSector element in elements)
                {
                    if (Vector3.Distance(requester.transform.position, element.GetSectorPosition()) <= distance)
                    {
                        elementsWithinRange.Add(element);
                    }
                }
            }
            return elementsWithinRange.ToArray();
        }
        public enum RayCastDirection { LEFT,RIGHT,FORWARD,DOWN,VELOCITY,NONE }
    }
}
