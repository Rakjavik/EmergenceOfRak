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
        public static Thing[] GetThingsWithinProximityOf(Thing requester, float distance)
        {
            List<Thing> returnList = new List<Thing>();
            foreach (Thing thing in Area.GetAllThings())
            {
                if (thing == requester) continue;
                if (Vector3.Distance(thing.transform.position, requester.transform.position) <= distance)
                    returnList.Add(thing);
            }
            return returnList.ToArray();
        }
        public static GridSector[] GetPiecesOfTerrainCreatureCanSee(Creature requester,float distance, 
            RAKTerrain terrain)
        {
            List<GridSector> elementsWithinRange = new List<GridSector>();
            GridSector[] elements = terrain.GetGridElements();
            foreach (GridSector element in elements)
            {
                if(Vector3.Distance(requester.transform.position,element.GetSectorPosition) <= distance)
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
                    if (Vector3.Distance(requester.transform.position, element.GetSectorPosition) <= distance)
                    {
                        elementsWithinRange.Add(element);
                    }
                }
            }
            return elementsWithinRange.ToArray();
        }
        public enum RayCastDirection { LEFT,RIGHT,FORWARD,DOWN,VELOCITY }
    }
}
