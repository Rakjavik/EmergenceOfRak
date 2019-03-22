﻿using rak.creatures;
using rak.creatures.memory;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

namespace rak.world
{
    public class Area
    {
        private static List<Thing> allThings;
        private static Dictionary<System.Guid, Thing> thingMasterList;
        private static List<JobHandle> jobHandles;
        public static void AddJobHandle(JobHandle handle)
        {
            jobHandles.Add(handle);
        }
        private static NativeArray<BlittableThing> allThingsBlittableCache;
        private static readonly int MAX_CONCURRENT_THINGS = 10000;
        // How many entries in the cache before empty structs are placed //
        public static int AllThingsCacheEntriesFilled { get; private set; }
        private static List<Thing> _removeTheseThings = new List<Thing>();
        private static float updateSunEvery = .1f;
        private static float timeSinceUpdatedThings = 0;
        private static float updateThingsEvery = 1f;
        public static float MinimumHeight = -50;
        public static float MaximumHeight = 200;
        private static bool initialized = false;
        public static float AreaLocalTime { get; private set; }

        public static Thing GetThingByGUID(System.Guid guid)
        {
            if(thingMasterList.ContainsKey(guid))
                return thingMasterList[guid];
            return null;
        }

        public static string GetFriendlyLocalTime()
        {
            float timeInDay = AreaLocalTime % dayLength;
            string time = ((int)(timeInDay * .1f)).ToString();
            return "HH:" + time;
        }
        public static string GetElapsedNumberOfHours()
        {
            int elapsedDays = (int)(Time.time / dayLength);
            float timeInDay = AreaLocalTime % dayLength;
            int hourInDay = (int)(timeInDay * .1f);

            int elapsedHours = hourInDay + (int)(elapsedDays * dayLength*.1f);
            return elapsedHours.ToString();
        }
        public static readonly float dayLength = 240;

        public static void AddThingToAllThings(Thing thingToAdd)
        {
            allThings.Add(thingToAdd);
            thingMasterList.Add(thingToAdd.guid, thingToAdd);
            //Debug.LogWarning("Adding " + thingToAdd.name);
        }
        public static NativeArray<BlittableThing> GetBlittableThings()
        {
            return allThingsBlittableCache;
        }
        private static void updateAllBlittableThings()
        {
            long start = System.DateTime.Now.ToBinary();
            for(int count = 0; count < jobHandles.Count; count++)
            {
                jobHandles[count].Complete();
            }
            jobHandles = new List<JobHandle>();
            AllThingsCacheEntriesFilled = allThings.Count;
            for (int count = 0; count < AllThingsCacheEntriesFilled; count++)
            {
                allThingsBlittableCache[count] = allThings[count].GetBlittableThing();
            }
            Debug.Log("Area cache update time - " + (System.DateTime.Now.ToBinary() - start));
        }
        public static List<Thing> GetAllThings()
        {
            return allThings;
        }
        public static List<Creature> GetAllCreatures()
        {
            List<Creature> returnList = new List<Creature>();
            List<Thing> allThingsLocal = GetAllThings();
            foreach (Thing thing in allThingsLocal)
            {
                if(thing is Creature)
                {
                    returnList.Add((Creature)thing);
                }
            }
            return returnList;
        }

        private World world;
        private HexCell cell;
        private Vector3 areaSize;
        private bool debug;
        private GameObject thingContainer = null;
        private GameObject creatureContainer = null;
        private GameObject disabledContainer = null;
        private List<Tribe> tribesPresent;
        private List<Site> sitesPresent;
        private GameObject[] walls;
        private Transform sun;
        private float sinceLastSunUpdated = 0;
        
        public Area(HexCell cell,World world)
        {
            tribesPresent = new List<Tribe>();
            sitesPresent = new List<Site>();
            this.cell = cell;
            this.world = world;
            sun = world.transform;
            AreaLocalTime = 0;
            areaSize = Vector3.zero; // Initialize in method
            walls = new GameObject[4];
            debug = World.ISDEBUGSCENE;
        }
        private void InitializeDebug(Tribe tribe)
        {
            int NUMBEROFGNATS = 1;

            sitesPresent.Add(new Site("Home of DeGnats"));
            tribesPresent.Add(tribe);
            tribe.Initialize();
            Debug.LogWarning("DEBUG MODE ENABLED");
            areaSize = new Vector3(256, 10, 256);
            allThings = new List<Thing>();
            thingContainer = new GameObject("ThingContainer");
            creatureContainer = new GameObject("CreatureContainer");
            if (disabledContainer == null)
                disabledContainer = new GameObject("DisabledContainer");
            /*for (int i = 0; i < 0; i++)
            {
                Vector3 position = new Vector3(Random.Range(10f, 200), Random.Range(3f,15f), Random.Range(10f, 200));
                addThingToWorld("fruit",position,false);
                    
            }*/
            for (int i = 0; i < NUMBEROFGNATS; i++)
            {
                Vector3 position = new Vector3(Random.Range(10f, 200), Random.Range(3f, 15f), Random.Range(10f, 200));
                addCreatureToWorldDEBUG(BASE_SPECIES.Gnat.ToString(), position, false, tribe);
            }
            CreatureUtilities.OptimizeUpdateTimes(allThings);
            FruitTree[] trees = GameObject.FindObjectsOfType<FruitTree>();
            for(int count = 0; count < trees.Length; count++)
            {
                trees[count].initialize("FruitTree");
                AddThingToAllThings(trees[count]);
            }
            Debug.LogWarning("Updates balanced");
        }
        public void Initialize(Tribe tribe)
        {
            if (initialized)
            {
                Debug.LogError("Area called to initialize when already initialized");
                return;
            }
            initialized = true;
            jobHandles = new List<JobHandle>();
            allThingsBlittableCache = new NativeArray<BlittableThing>(MAX_CONCURRENT_THINGS,Allocator.Persistent,NativeArrayOptions.UninitializedMemory);
            thingMasterList = new Dictionary<System.Guid, Thing>();
            if (debug) // DEBUG
            {
                InitializeDebug(tribe);
                return;
            }
            areaSize = world.masterTerrain.GetSize();
            for(int count = 0; count < walls.Length; count++)
            {
                walls[count] = GameObject.Instantiate(RAKUtilities.getWorldPrefab("Wall"),null);
                walls[count].transform.GetChild(0).localScale = new Vector3(areaSize.x, 256, 1);
            }
            walls[0].transform.position = new Vector3(areaSize.x / 2, 128, 1);
            walls[1].transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            walls[1].transform.position = new Vector3(0,0,areaSize.z/2);
            walls[2].transform.position = new Vector3(areaSize.x/2, 0, areaSize.z);
            walls[3].transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            walls[3].transform.position = new Vector3(areaSize.x, 0, areaSize.z/2);
            if (thingContainer == null)
                thingContainer = new GameObject("ThingContainer");
            if(creatureContainer == null)
                creatureContainer = new GameObject("CreatureContainer");
            if(disabledContainer == null)
                disabledContainer = new GameObject("DisabledContainer");
            allThings = new List<Thing>();
            // Scan the TerrainObjects placed in map for Things //
            foreach (RAKTerrain terrain in world.masterTerrain.getTerrain())
            {
                foreach (RAKTerrainObject terrainObject in terrain.nonTerrainObjects)
                {
                    Thing thing = terrainObject.gameObject.GetComponent<Thing>();
                    if (thing != null)
                    {
                        thing.initialize(thing.name);
                        AddThingToAllThings(thing);
                    }
                }
            }
            int populationToCreate = tribe.GetPopulation();
            int MAXPOP = 200;
            if (populationToCreate > MAXPOP) populationToCreate = MAXPOP;
            Debug.LogWarning("Generating a population of - " + populationToCreate);
            for (int count = 0; count < populationToCreate; count++)
            {
                AddCreatureToWorld("Gnat");
            }
        }

        

        public Thing[] findConsumeable(CONSUMPTION_TYPE consumptionType)
        {
            List<Thing> things = new List<Thing>();
            List<Thing> allThingsLocal = GetAllThings();
            {
                foreach (Thing thing in allThingsLocal)
                {
                    if (thing.matchesConsumptionType(consumptionType))
                    {
                        things.Add(thing);
                    }
                }
            }
            return things.ToArray();
        }
        public void AddCreatureToWorld(string nameOfPrefab)
        {
            addCreatureToWorld(nameOfPrefab, GetRandomGridSector().GetRandomPositionInSector,false);
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
                float y = world.masterTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
                newThing.transform.position = new Vector3(x, y, z);
            }
            AddThingToAllThings(newThing.GetComponent<Thing>());
        }

        private void addCreatureToWorld(string nameOfPrefab, Vector3 position, bool generatePosition)
        { 
            GameObject thingObject = RAKUtilities.getCreaturePrefab(nameOfPrefab);
            GameObject newThing = Object.Instantiate(thingObject);
            newThing.transform.SetParent(creatureContainer.transform);
            newThing.GetComponent<Creature>().Initialize(nameOfPrefab,this,null);
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
                float y = world.masterTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
                newThing.transform.position = new Vector3(x, y, z);
            }
            AddThingToAllThings(newThing.GetComponent<Thing>());
        }
        public RAKTerrain GetClosestTerrainToPoint(Vector3 point)
        {
            return world.masterTerrain.GetClosestTerrainToPoint(point);
        }
        public void addThingToWorld(string nameOfPrefab)
        {
            addThingToWorld(nameOfPrefab, Vector3.zero,true);
        }
        public GameObject addThingToWorld(string nameOfPrefab,Vector3 position,bool generatePos)
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
            AddThingToAllThings(newThing.GetComponent<Thing>());
            return newThing;
        }
        public void RemoveThingFromWorld(Thing thing)
        {
            _removeTheseThings.Add(thing);
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
                float y = world.masterTerrain.GetTerrainHeightAt(targetSpawn, GetClosestTerrainToPoint(targetSpawn));
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
        public GridSector GetCurrentGridSector(Transform transform)
        {
            RAKTerrain closestTerrain = world.masterTerrain.GetClosestTerrainToPoint(transform.position);
            GridSector[] sectors = closestTerrain.GetGridElements();
            float closestDist = Mathf.Infinity;
            int indexOfClosest = -1;
            for (int count = 0; count < sectors.Length; count++)
            {
                float currentDistance = Vector3.Distance(transform.position, sectors[count].GetSectorPosition);
                if (currentDistance < closestDist)
                {
                    closestDist = currentDistance;
                    indexOfClosest = count;
                }
            }
            
            return sectors[indexOfClosest];
        }
        public GridSector GetRandomGridSector()
        {
            RAKTerrain[] terrain = world.masterTerrain.getTerrain();
            int index = Random.Range(0, terrain.Length);
            RAKTerrain chosenTerrain = terrain[index];
            GridSector[] sectorsInTerrain = chosenTerrain.GetGridElements();
            index = Random.Range(0, sectorsInTerrain.Length);
            return sectorsInTerrain[index];
        }

        private void dumpDisabledObjectsToDisk()
        {
            throw new System.NotImplementedException();
        }
        public int ActiveCreatureCount { get
            {
                return creatureContainer.transform.childCount;
            } }
        public int ActiveThingCount { get
            {
                return thingContainer.transform.childCount;
            } }
        public int DeathsByFlight { get
            {
                return getDeathsBy(Creature.CREATURE_DEATH_CAUSE.FlightCollision);
            }
        }
        public int DeathsByHunger { get
            {
                return getDeathsBy(Creature.CREATURE_DEATH_CAUSE.Hunger);
            } }
        private int getDeathsBy(Creature.CREATURE_DEATH_CAUSE cause)
        {
            int deaths = 0;
            for (int count = 0; count < disabledContainer.transform.childCount; count++)
            {
                Creature creature = disabledContainer.transform.GetChild(count).GetComponent<Creature>();
                if(creature != null)
                {
                    if (creature.CauseOfDeath == cause)
                        deaths++;
                }
            }
            return deaths;
        }

        #region MONO METHODS
        public void Update(float delta)
        {
            sinceLastSunUpdated += delta;
            // AREA OWNS THE MASTER LOCK ON ALL THINGS //
            foreach (Thing thing in allThings)
            {
                thing.ManualUpdate(delta);
                if (thing is Creature)
                {
                    Creature creature = (Creature)thing;
                    if (creature.GetCurrentState() == Creature.CREATURE_STATE.DEAD)
                    {
                        _removeTheseThings.Add(creature);
                        creature.DestroyAllParts();
                        Debug.LogWarning("Removed a dead creature");
                        DebugMenu.AppendDebugLine("Removed a dead creature", creature);
                    }
                }
            }
            foreach (Thing thing in _removeTheseThings)
            {
                allThings.Remove(thing);
                thing.gameObject.SetActive(false);
                thing.transform.SetParent(disabledContainer.transform);
            }
            _removeTheseThings = new List<Thing>();
            
            foreach (Tribe tribe in tribesPresent)
            {
                tribe.Update();
            }
            AreaLocalTime += delta;
            if (sinceLastSunUpdated > updateSunEvery)
            {
                float timeOfDay = (AreaLocalTime) % dayLength;
                sun.rotation = Quaternion.Euler(new Vector3((timeOfDay / dayLength) * 360, 0, 0));
                sinceLastSunUpdated = 0;
            }
            timeSinceUpdatedThings += Time.deltaTime;
            if (timeSinceUpdatedThings > updateThingsEvery)
            {
                updateAllBlittableThings();
                timeSinceUpdatedThings = 0;
            }
        }
        #endregion
    }
}
