using rak.creatures;
using rak.creatures.memory;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using rak.ecs.ThingComponents;
using Unity.Mathematics;

namespace rak.world
{
    public class Area
    {
        // DEBUG METHOD FOR VR //
        public static void SetDestinationForAllCreatureAgents(Vector3 dest)
        {
            for (int count = 0; count < allThings.Count; count++)
            {
                if(allThings[count] is Creature)
                {
                    Creature creature = (Creature)allThings[count];
                    creature.GetCreatureAgent().SetDestination(dest);
                }
            }
        }
        private static List<Thing> allThings;
        private static List<CreatureAgent> agents;
        private static Dictionary<System.Guid, Thing> thingMasterList;
        private static List<JobHandle> jobHandles;
        public static void AddJobHandle(JobHandle handle)
        {
            jobHandles.Add(handle);
        }
        private static NativeArray<BlittableThing> allThingsBlittableCache;
        public static void WorldDisabled()
        {
            allThingsBlittableCache.Dispose();
        }

        private static readonly int MAX_CONCURRENT_THINGS = 10000;
        private static readonly int MAKE_CREATURES_INVISIBLE_IF_THIS_FAR_FROM_CAMERA = 128;
        private static readonly int MAX_VISIBLE_CREATURES = 40;
        private static int MAXPOP = 30;
        public static readonly int KEEP_CREATURES_VISIBLE_FOR_SECONDS_AFTER_OUT_OF_VIEW = 5;
        public static float dayLength = 240;

        // How many entries in the cache before empty structs are placed //
        public static int AllThingsCacheEntriesFilled { get; private set; }
        private static List<Thing> _removeTheseThings = new List<Thing>();
        private static float updateSunEvery = .1f;
        private static float timeSinceUpdatedThings = 0;
        private static float updateThingsEvery = 1f;
        private static float timeSinceLastCreatureDistanceSightCheck = 0;
        public static float MinimumHeight = -50;
        public static float MaximumHeight = 200;
        private static bool initialized = false;
        private static Camera mainCamera;
        public static float AreaLocalTime { get; private set; }
        public static int NumberOfVisibleThings { get; private set; }

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
        public static World.Time_Of_Day GetTimeOfDay()
        {
            float timeInDay = AreaLocalTime % dayLength;
            float timePerPeriod = dayLength / 4;
            if (timeInDay < timePerPeriod) return World.Time_Of_Day.SunRise;
            else if (timeInDay < timePerPeriod * 2) return World.Time_Of_Day.Midday;
            else if (timeInDay < timePerPeriod * 3) return World.Time_Of_Day.SunSet;
            else return World.Time_Of_Day.Night;
        }
        public static string GetElapsedNumberOfHours()
        {
            int elapsedDays = (int)(Time.time / dayLength);
            float timeInDay = AreaLocalTime % dayLength;
            int hourInDay = (int)(timeInDay * .1f);

            int elapsedHours = hourInDay + (int)(elapsedDays * dayLength*.1f);
            return elapsedHours.ToString();
        }
        
        public static void AddThingToAllThings(Thing thingToAdd)
        {
            allThings.Add(thingToAdd);
            if (thingToAdd == null)
                Debug.LogError("NULL");
            thingMasterList.Add(thingToAdd.guid, thingToAdd);
            if(thingToAdd is Creature)
            {
                Creature creature = (Creature)thingToAdd;
                if (creature.HasAgent())
                    agents.Add(creature.GetCreatureAgent());
            }
            Unity.Entities.World world = Unity.Entities.World.Active;
            Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(thingToAdd.gameObject, world);
            NativeArray<Entity> entities = new NativeArray<Entity>(1,Allocator.Temp);
            world.EntityManager.Instantiate(sourceEntity, entities);
            entities.Dispose();
            thingToAdd.AddECSComponents();
        }
        public static NativeArray<BlittableThing> GetBlittableThings()
        {
            return allThingsBlittableCache;
        }
        private static void updateAllBlittableThingsCache()
        {
            int jobHandlesLength = jobHandles.Count;
            for(int count = 0; count < jobHandlesLength; count++)
            {
                jobHandles[count].Complete();
            }
            jobHandles = new List<JobHandle>();
            AllThingsCacheEntriesFilled = allThings.Count;
            for (int count = 0; count < AllThingsCacheEntriesFilled; count++)
            {
                allThingsBlittableCache[count] = allThings[count].GetBlittableThing();
            }
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
        private static int batchSize = 20;
        private static float batchDelta = 0;
        private static int currentThingIndex = 0;
        private static int currentBatch = 0;
        
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
            MAXPOP = 1;
            //dayLength = 360;

        }
        public void Initialize(Tribe tribe)
        {
            
            if (initialized)
            {
                Debug.LogError("Area called to initialize when already initialized");
                return;
            }
            jobHandles = new List<JobHandle>();
            allThingsBlittableCache = new NativeArray<BlittableThing>(MAX_CONCURRENT_THINGS,Allocator.Persistent,NativeArrayOptions.UninitializedMemory);
            thingMasterList = new Dictionary<System.Guid, Thing>();
            mainCamera = Camera.main;
            if (debug) // DEBUG
            {
                InitializeDebug(tribe);
                //return;
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
            agents = new List<CreatureAgent>();
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
            
            populationToCreate = MAXPOP;
            Debug.LogWarning("Generating a population of - " + populationToCreate);
            for (int count = 0; count < populationToCreate; count++)
            {
                AddCreatureToWorld("Gnat");
            }
            /*NativeArray<Entity> entities = new NativeArray<Entity>(allThings.Count, Allocator.Temp);
            for(int count = 0; count < allThings.Count; count++)
            {
                EntityManager.Instantiate(allThings[count].gameObject, entities);
                allThings[count].AddECSComponents();
                allThings[count].entityIndex = entities[count].Index;
            }*/
            initialized = true;
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
            addCreatureToWorld(nameOfPrefab, Vector3.zero,true);
        }

        private void addCreatureToWorld(string nameOfPrefab, Vector3 position, bool generatePosition)
        { 
            GameObject thingObject = RAKUtilities.getCreaturePrefab(nameOfPrefab);
            GameObject newThing = Object.Instantiate(thingObject);
            newThing.transform.SetParent(creatureContainer.transform);
            Creature creature = newThing.GetComponent<Creature>();
            if (creature == null) creature = newThing.GetComponentInChildren<Creature>();
            creature.Initialize(nameOfPrefab,this,null);
            newThing.transform.localPosition = Vector3.zero;
            newThing.transform.rotation = Quaternion.identity;
            if(!generatePosition)
            {
                newThing.transform.position = position;
            }
            else
            {
                Vector3 randomPosition = GetRandomGridSector().GetRandomPositionInSector();
                newThing.transform.position = new Vector3(randomPosition.x, 10, randomPosition.z);
            }
            Thing thing = newThing.GetComponent<Thing>();
            if (thing == null)
                thing = newThing.GetComponentInChildren<Thing>();
            AddThingToAllThings(thing);
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
                float x = UnityEngine.Random.Range(startPosition.x-maxDistance, startPosition.x+maxDistance);
                float z = UnityEngine.Random.Range(startPosition.z - maxDistance, startPosition.z + maxDistance);
                Vector2 targetSpawn = new Vector2(x, z);
                float y = RAKTerrainMaster.GetTerrainHeightAt(targetSpawn, RAKTerrainMaster.GetTerrainAtPoint(targetSpawn));
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
        public static GridSector GetCurrentGridSector(Transform transform)
        {
            Vector3 position = transform.position;
            RAKTerrain terrain = RAKTerrainMaster.GetTerrainAtPoint(position);
            GridSector[] sectors = terrain.GetGridElements();
            for(int sectorCount = 0; sectorCount < sectors.Length; sectorCount++)
            {
                if(position.x > sectors[sectorCount].WorldPositionStart.x && 
                    position.x < sectors[sectorCount].WorldPositionEnd.x)
                {
                    if (position.z > sectors[sectorCount].WorldPositionStart.z &&
                    position.z < sectors[sectorCount].WorldPositionEnd.z)
                    {
                        return sectors[sectorCount];
                    }
                }
            }
            return GridSector.Empty;
        }
        public GridSector GetRandomGridSector()
        {
            RAKTerrain[] terrain = world.masterTerrain.getTerrain();
            int index = UnityEngine.Random.Range(0, terrain.Length);
            RAKTerrain chosenTerrain = terrain[index];
            GridSector[] sectorsInTerrain = chosenTerrain.GetGridElements();
            index = UnityEngine.Random.Range(0, sectorsInTerrain.Length);
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
        public void FixedUpdate(float delta)
        {
            EntityManager manager = Unity.Entities.World.Active.EntityManager;
            updateEntityRaycasting(manager);
        }
        public void update(float delta)
        {
            if (initialized)
            {
                masterUpdate(delta);
            }
        }
        private void updateEntityRaycasting(EntityManager manager)
        {
            List<RaycastCommand> raycastCommands = new List<RaycastCommand>();
            Dictionary<int, Entity> creatureIndexMap = new Dictionary<int, Entity>();
            Dictionary<int, CreatureUtilities.RayCastDirection> indexDirectionMap = new Dictionary<int, CreatureUtilities.RayCastDirection>();
            int allThingsLength = allThings.Count;
            List<Creature> creaturesGettingUpdated = new List<Creature>();
            for (int count = 0; count < allThingsLength; count++)
            {
                if (!(allThings[count] is Creature))
                    continue;
                Entity entity = allThings[count].goEntity.Entity;
                Agent agentData = manager.GetComponentData<Agent>(entity);
                if (agentData.RequestRaycastUpdateDirectionLeft == 1)
                {
                    Transform transform = allThings[count].transform;
                    float3 rayDirection = -transform.right;
                    RaycastCommand command = new RaycastCommand
                    {
                        direction = rayDirection,
                        distance = 20,
                        from = transform.position,
                        maxHits = 1
                    };
                    raycastCommands.Add(command);
                    creatureIndexMap.Add(raycastCommands.Count - 1, entity);
                    indexDirectionMap.Add(raycastCommands.Count - 1, CreatureUtilities.RayCastDirection.LEFT);
                    Debug.LogWarning("Requesting Left update");
                }
                if (agentData.RequestRaycastUpdateDirectionRight == 1)
                {
                    Transform transform = allThings[count].transform;
                    float3 rayDirection = transform.right;
                    RaycastCommand command = new RaycastCommand
                    {
                        direction = rayDirection,
                        distance = 20,
                        from = transform.position,
                        maxHits = 1
                    };
                    raycastCommands.Add(command);
                    creatureIndexMap.Add(raycastCommands.Count - 1, entity);
                    indexDirectionMap.Add(raycastCommands.Count - 1, CreatureUtilities.RayCastDirection.RIGHT);
                    Debug.LogWarning("Requesting Right update");
                }
                if (agentData.RequestRaycastUpdateDirectionDown == 1)
                {
                    Transform transform = allThings[count].transform;
                    float3 rayDirection = Vector3.down;
                    RaycastCommand command = new RaycastCommand
                    {
                        direction = rayDirection,
                        distance = 20,
                        from = transform.position-transform.up,
                        maxHits = 1
                    };
                    raycastCommands.Add(command);
                    creatureIndexMap.Add(raycastCommands.Count - 1, entity);
                    indexDirectionMap.Add(raycastCommands.Count - 1, CreatureUtilities.RayCastDirection.DOWN);
                    //Debug.LogWarning("Requesting Down update");
                }
                if (agentData.RequestRaycastUpdateDirectionForward == 1)
                {
                    Transform transform = allThings[count].transform;
                    float3 rayDirection = transform.forward;
                    RaycastCommand command = new RaycastCommand
                    {
                        direction = rayDirection,
                        distance = 20,
                        from = transform.position+transform.forward,
                        maxHits = 1
                    };
                    raycastCommands.Add(command);
                    creatureIndexMap.Add(raycastCommands.Count - 1, entity);
                    indexDirectionMap.Add(raycastCommands.Count - 1, CreatureUtilities.RayCastDirection.FORWARD);
                    //Debug.LogWarning("Requesting Forward update");
                }
            }
            int rayCastCommandSize = raycastCommands.Count;
            if (rayCastCommandSize > 0)
            {
                RaycastHit[] hits = new RaycastHit[rayCastCommandSize];
                for (int count =0; count < rayCastCommandSize; count++)
                {
                    RaycastCommand command = raycastCommands[count];
                    RaycastHit hit;
                    if(Physics.Raycast(command.from, command.direction, out hit, command.distance))
                    {
                        hits[count] = hit;
                        Debug.DrawLine(command.from, hit.point, Color.yellow, .5f);
                    }
                }
                
                for (int count = 0; count < hits.Length; count++)
                {
                    CreatureUtilities.RayCastDirection direction = indexDirectionMap[count];
                    Entity entity = creatureIndexMap[count];
                    Agent agentData = manager.GetComponentData<Agent>(entity);
                    if (hits[count].distance == 0)
                    {
                        if (direction == CreatureUtilities.RayCastDirection.DOWN)
                        {
                            agentData.RequestRaycastUpdateDirectionDown = 0;
                            agentData.DistanceFromGround = float.MaxValue;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.FORWARD)
                        {
                            agentData.RequestRaycastUpdateDirectionForward = 0;
                            agentData.DistanceFromFirstZHit = float.MaxValue;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.LEFT)
                        {
                            agentData.RequestRaycastUpdateDirectionLeft = 0;
                            agentData.DistanceFromLeft = float.MaxValue;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.RIGHT)
                        {
                            agentData.RequestRaycastUpdateDirectionRight = 0;
                            agentData.DistanceFromRight = float.MaxValue;
                        }
                    }
                    else
                    {
                        if (direction == CreatureUtilities.RayCastDirection.LEFT)
                        {
                            agentData.DistanceFromLeft = hits[count].distance;
                            agentData.RequestRaycastUpdateDirectionLeft = 0;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.RIGHT)
                        {
                            agentData.DistanceFromRight = hits[count].distance;
                            agentData.RequestRaycastUpdateDirectionRight = 0;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.DOWN)
                        {
                            agentData.DistanceFromGround = hits[count].distance;
                            agentData.RequestRaycastUpdateDirectionDown = 0;
                        }
                        else if (direction == CreatureUtilities.RayCastDirection.FORWARD)
                        {
                            agentData.DistanceFromFirstZHit = hits[count].distance;
                            agentData.RequestRaycastUpdateDirectionForward = 0;
                        }

                    }
                    manager.SetComponentData(entity, agentData);
                }
            }
        }
        
        private void masterUpdate(float delta)
        {
            
            sinceLastSunUpdated += delta;
            batchDelta += delta;
            timeSinceLastCreatureDistanceSightCheck += delta;
            // DO THESE EVERY UPDATE //
            NumberOfVisibleThings = 0;
            int lengthOfArray = agents.Count;
            for (int count = 0; count < lengthOfArray; count++)
            {
                bool visible = agents[count].creature.Visible;
                if (visible) NumberOfVisibleThings++;
                if (agents[count].Active)
                    agents[count].Update(delta, visible);
            }
            AreaLocalTime += delta;
            if (sinceLastSunUpdated > updateSunEvery)
            {
                float timeOfDay = (AreaLocalTime) % dayLength;
                sun.rotation = Quaternion.Euler(new Vector3((timeOfDay / dayLength) * 360, 0, 0));
                sinceLastSunUpdated = 0;
            }
            
            // DO THESE BEFORE STARTING BATCHES, BUT NOT DURING BATCH PROCESSING //
            if (currentThingIndex == 0)
            {
                lengthOfArray = _removeTheseThings.Count;
                for (int count = 0; count < lengthOfArray; count++)
                {
                    Thing singleThing = _removeTheseThings[count];
                    allThings.Remove(singleThing);
                    singleThing.Deactivate();
                    singleThing.transform.SetParent(disabledContainer.transform);
                }
                _removeTheseThings = new List<Thing>();

                foreach (Tribe tribe in tribesPresent)
                {
                    tribe.Update();
                }
                
                timeSinceUpdatedThings += Time.deltaTime;
                if (timeSinceUpdatedThings > updateThingsEvery)
                {
                    updateAllBlittableThingsCache();
                    timeSinceUpdatedThings = 0;
                }
            }
            // THING UPDATES //
            int startIndex = currentThingIndex;
            lengthOfArray = allThings.Count;
            for (int count = currentThingIndex;count < lengthOfArray; count++)
            {
                Vector3 cameraPosition = mainCamera.transform.position;
                Thing thing = allThings[count];
                thing.ManualUpdate(batchDelta);
                if (thing is Creature)
                {
                    Creature creature = (Creature)thing;
                    creature.ManualCreatureUpdate(batchDelta);
                    if (creature.GetCurrentState() == Creature.CREATURE_STATE.DEAD)
                    {
                        _removeTheseThings.Add(creature);
                    }
                    if (creature.InView)
                    {
                        float distanceFromCamera = Vector3.Distance(cameraPosition, creature.transform.position);
                        //Debug.LogWarning("Distance from cam " + distanceFromCamera);
                        if (distanceFromCamera > MAKE_CREATURES_INVISIBLE_IF_THIS_FAR_FROM_CAMERA)
                        {
                            if (creature.Visible)
                            {
                                creature.SetVisible(false);
                            }
                        }
                        else
                        {
                            if (!creature.Visible)
                            {
                                if(NumberOfVisibleThings < MAX_VISIBLE_CREATURES)
                                    creature.SetVisible(true);
                            }
                        }
                        
                        timeSinceLastCreatureDistanceSightCheck = 0;
                    }
                }
                currentThingIndex++;
                if(currentThingIndex-startIndex >= batchSize)
                {
                    currentBatch++;
                    break;
                }
                if(currentThingIndex == allThings.Count)
                {
                    //Debug.LogWarning("All batches complete # - " + currentBatch + " batchdelta - " + batchDelta);
                    currentThingIndex = 0;
                    currentBatch = 0;
                    batchDelta = 0;
                    break;
                }
            }
        }
    }
}
