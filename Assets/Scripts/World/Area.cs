using rak.creatures;
using rak.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace rak.world
{
    public class Area
    {
        public static List<Thing> GetAllThings()
        {
            return allThings;
        }
        private static List<Thing> allThings;
        private World world;
        private HexCell cell;
        private Vector3 areaSize;
        private bool debug = true;
        private GameObject thingContainer = null;
        private GameObject creatureContainer;
        private List<Tribe> tribesPresent;
        private List<Site> sitesPresent;

        public Area(HexCell cell,World world)
        {
            tribesPresent = new List<Tribe>();
            sitesPresent = new List<Site>();
            this.cell = cell;
            this.world = world;
            areaSize = Vector3.zero; // Initialize in method
        }
        private void InitializeDebug(Tribe tribe)
        {
            sitesPresent.Add(new Site("Home of DeGnats"));
            tribesPresent.Add(tribe);
            Debug.LogWarning("DEBUG MODE ENABLED");
            areaSize = new Vector3(256, 10, 256);
            allThings = new List<Thing>();
            thingContainer = new GameObject("ThingContainer");
            creatureContainer = new GameObject("CreatureContainer");
            for (int i = 0; i < 100; i++)
            {
                Vector3 position = new Vector3(Random.Range(-1, 70), Random.Range(3,15), Random.Range(-20, 50));
                addThingToWorld("fruit",position,false);
                if(i % 4 == 0)
                    addCreatureToWorldDEBUG(BASE_SPECIES.Gnat.ToString(), new Vector3(13, 6, 7), true, tribe);
            }
            //addCreatureToWorldDEBUG(BASE_SPECIES.Gagk.ToString(),new Vector3(13,6,7),false,tribe);
            //addThingToWorld("fruit", new Vector3(60,4.5f,32), false);
            //addThingToWorld("fruit", new Vector3(28.75f, 4.5f, 100), false);
            //addCreatureToWorldDEBUG(BASE_SPECIES.Gnat.ToString(), new Vector3(19, 6, 31), false, tribe);
            //addCreatureToWorldDEBUG("Gnat", new Vector3(31, 6, 22), false, tribe);
            //addCreatureToWorldDEBUG("Gnat", new Vector3(125, 6, 100), false, tribe);
            //addCreatureToWorldDEBUG("Gnat", new Vector3(119, 6, 131), false, tribe);

        }
        public void Initialize(Tribe tribe)
        {
            if (debug)
            {
                InitializeDebug(tribe);
                return;
            }
            if(thingContainer == null)
            {
                thingContainer = new GameObject("ThingContainer");
            }
            areaSize = world.currentTerrain.GetSize();
            allThings = new List<Thing>();
            int populationToCreate = tribe.GetPopulation();
            Debug.LogWarning("Generating a population of - " + populationToCreate);
            if (populationToCreate > 10) populationToCreate = 10;
            for (int count = 0; count < populationToCreate; count++)
            {
                addCreatureToWorld("Gnat");
            }
            
            for (int i = 0; i < populationToCreate*50; i++)
            {
                addThingToWorld("fruit");
            }

        }

        public void Update(float delta)
        {
            List<Thing> inactiveThings = new List<Thing>();
            foreach(Thing thing in allThings)
            {
                thing.ManualUpdate(delta);
                if (thing is Creature)
                {
                    Creature creature = (Creature)thing;
                    if (creature.GetCurrentState() == Creature.CREATURE_STATE.DEAD)
                    {
                        inactiveThings.Add(creature);
                        creature.DestroyAllParts();
                        GameObject.Destroy(creature.gameObject);
                        Debug.LogWarning("Removed a dead creature");
                    }
                }
            }
            foreach(Thing thing in inactiveThings)
            {
                removeThingFromWorld(thing);
            }
            
        }

        public Thing[] findConsumeable(CONSUMPTION_TYPE consumptionType)
        {
            List<Thing> things = new List<Thing>();
            foreach(Thing thing in allThings)
            {
                if(thing.matchesConsumptionType(consumptionType))
                {
                    things.Add(thing);
                }
            }
            return things.ToArray();
        }
        public void addCreatureToWorld(string nameOfPrefab)
        {
            addCreatureToWorld(nameOfPrefab, Vector3.zero,true);
        }

        private void addCreatureToWorldDEBUG(string nameOfPrefab, Vector3 position, 
            bool generatePosition,Tribe tribe)
        {
            GameObject thingObject = RAKUtilities.getCreaturePrefab(nameOfPrefab);
            GameObject newThing = Object.Instantiate(thingObject);
            newThing.transform.SetParent(creatureContainer.transform);
            newThing.GetComponent<Creature>().Initialize(nameOfPrefab, this, tribe);
            newThing.transform.localPosition = Vector3.zero;
            newThing.transform.rotation = Quaternion.identity;
            if (!generatePosition)
            {
                newThing.transform.position = position;
            }
            else
            {
                float x = Random.Range(0, areaSize.x);
                float z = Random.Range(0, areaSize.z);
                Vector2 targetSpawn = new Vector2(x, z);
                float y = world.currentTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
                newThing.transform.position = new Vector3(x, y, z);
            }
            allThings.Add(newThing.GetComponent<Thing>());
        }

        private void addCreatureToWorld(string nameOfPrefab, Vector3 position, bool generatePosition)
        { 
            GameObject thingObject = RAKUtilities.getCreaturePrefab(nameOfPrefab);
            GameObject newThing = Object.Instantiate(thingObject);
            newThing.transform.SetParent(creatureContainer.transform);
            newThing.GetComponent<Creature>().Initialize(nameOfPrefab,this);
            newThing.transform.localPosition = Vector3.zero;
            newThing.transform.rotation = Quaternion.identity;
            if(!generatePosition)
            {
                newThing.transform.position = position;
            }
            else
            {
                float x = Random.Range(0, areaSize.x);
                float z = Random.Range(0, areaSize.z);
                Vector2 targetSpawn = new Vector2(x, z);
                float y = world.currentTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
                newThing.transform.position = new Vector3(x, y, z);
            }
            allThings.Add(newThing.GetComponent<Thing>());
        }
        private RAKTerrain GetClosestTerrainToPoint(Vector3 point)
        {
            return world.currentTerrain.GetClosestTerrainToPoint(point);
        }
        public void addThingToWorld(string nameOfPrefab)
        {
            addThingToWorld(nameOfPrefab, Vector3.zero,true);
        }
        public void addThingToWorld(string nameOfPrefab,Vector3 position,bool generatePos)
        {
            GameObject thingObject = RAKUtilities.getThingPrefab(nameOfPrefab);
            GameObject newThing = UnityEngine.Object.Instantiate(thingObject);
            newThing.transform.SetParent(thingContainer.transform);
            newThing.GetComponent<Thing>().initialize("fruit");
            if (!generatePos)
            {
                newThing.transform.position = position;
            }
            else
            {
                newThing.transform.position = GetRandomPositionOnNavMesh();
            }
            allThings.Add(newThing.GetComponent<Thing>());
        }
        public bool removeThingFromWorld(Thing thing)
        {
            return allThings.Remove(thing);
        }
        public Vector3 GetRandomPositionOnNavMesh(Vector3 startPosition,float maxDistance)
        {
            int maxTries = 100;
            int currentTry = 0;
            while (true)
            {
                float x = Random.Range(startPosition.x-maxDistance, startPosition.x+maxDistance);
                float z = Random.Range(startPosition.z - maxDistance, startPosition.z + maxDistance);
                Vector2 targetSpawn = new Vector2(x, z);
                float y = world.currentTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
                Vector3 randomPosition = new Vector3(x, y, z);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPosition, out hit, maxDistance, NavMesh.AllAreas))
                {
                    return hit.position;
                }
                currentTry++;
                if (currentTry >= maxTries) break;
            }
            Debug.LogError("Couldn't find a valid random position on nav mesh");
            return Vector3.zero;
        }
        public Vector3 GetRandomPositionOnNavMesh()
        {
            return GetRandomPositionOnNavMesh(Vector3.zero, 1000);
        }
    }
}
