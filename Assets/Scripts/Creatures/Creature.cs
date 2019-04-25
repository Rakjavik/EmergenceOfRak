using rak.creatures.memory;
using rak.ecs.ThingComponents;
using rak.world;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace rak.creatures
{

    public class Creature : Thing
    {
        public enum CREATURE_STATE { IDLE, MOVE, WAIT, DEAD, SLEEP }
        public enum CREATURE_DEATH_CAUSE { FlightCollision, Hunger, NA }

        public static readonly int CLOSE_OBJECT_CACHE_SIZE = 100;

        public bool DEBUGSCENE = false;
        public float TESTVAR1 = 150;
        public float TESTVAR2 = 0.00001f;

        public bool RunDebugMethod = false;
        public BASE_SPECIES baseSpecies;
        public CREATURE_DEATH_CAUSE CauseOfDeath { get
            {
                if (currentState == CREATURE_STATE.DEAD)
                    return causeOfDeath;
                return CREATURE_DEATH_CAUSE.NA;
            } }
        public Dictionary<MiscVariables.CreatureMiscVariables, float> miscVariables { get; private set; }
        public GridSector currentSector { get; private set; }
        public bool Visible { get; private set; }
        public bool InView { get; private set; }

        private CREATURE_STATE currentState;
        private CREATURE_DEATH_CAUSE causeOfDeath = CREATURE_DEATH_CAUSE.NA;
        private float lastUpdated = 0;
        private float observeEvery = 5f;
        private float lastObserved = 2.5f;
        private ObserveJobFor jobFor;
        private bool initialized = false;
        private JobHandle observeHandle;
        private bool awaitingObservation = false;
        private Transform mainCamera;
        private RAKTerrain currentTerrain;
        private float timeSinceLeftView = 0;

        private Species species;
        private SpeciesPhysicalStats creaturePhysicalStats;
        private AudioSource audioSource;

        protected Inventory inventory;
        private TaskManager taskManager;

        private CreatureAgent agent;
        private Tribe memberOfTribe;
        private Area currentArea;
        private System.Guid[] knownGridSectorsVisited;
        private int knownGridSectorsCount = 0;
        private EntityManager em;

        public bool IsInitialized()
        {
            return initialized;
        }
        public bool StillNeedsSleep()
        {
            if (creaturePhysicalStats.getNeeds().getMostUrgent() != Needs.NEEDTYPE.SLEEP &&
                creaturePhysicalStats.getNeeds().getMostUrgent() != Needs.NEEDTYPE.NONE)
            {
                Debug.LogWarning("No more sleep, most urget - " + creaturePhysicalStats.getNeeds().getMostUrgent());
                return false;
            }
            else if (GetNeedAmount(Needs.NEEDTYPE.SLEEP) <= 0)
                return false;
            return true;
        }
        public void Initialize(string name, Area area, Tribe memberOf)
        {
            Initialize(name, area);
            memberOfTribe = memberOf;
        }
        private void Initialize(string name, Area area)
        {
            em = Unity.Entities.World.Active.EntityManager;
            if (initialized)
            {
                Debug.LogError("Call to initialize when already initialized");
                return;
            }
            initialize(name);
            this.currentArea = area;
            taskManager = new TaskManager(this);
            inventory = new Inventory(this);
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                species = new Species('m', "Gnat", true, BASE_SPECIES.Gnat,
                    ConsumptionType.OMNIVORE, this);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                species = new Species('n', baseSpecies.ToString(), true, BASE_SPECIES.Gagk,
                    ConsumptionType.HERBIVORE, this);
            }
            creaturePhysicalStats = CreatureConstants.PhysicalStatsInitialize(species.getBaseSpecies(), this);
            agent = new CreatureAgent(this);
            audioSource = GetComponent<AudioSource>();
            currentState = CREATURE_STATE.IDLE;
            creaturePhysicalStats.Initialize(species.getBaseSpecies());
            agent.Initialize(species.getBaseSpecies());
            miscVariables = MiscVariables.GetCreatureMiscVariables(this);
            memberOfTribe = null;
            knownGridSectorsVisited = new System.Guid[100];
            lastObserved = Random.Range(0, observeEvery);
            Visible = false;
            mainCamera = Camera.main.transform;
            InView = false;
            agent.GetRigidBody().isKinematic = true;
            initialized = true;
        }
        public void PlayOneShot()
        {
            if (!audioSource.isPlaying && Visible)
            {
                audioSource.Play();
            }
        }
        public Thing GetClosestKnownReachableConsumable()
        {
            return species.memory.GetClosestFoodFromMemory
                (true, species.getConsumptionType(), transform.position);
        }
        public Thing GetClosestKnownConsumableProducer()
        {
            return species.memory.GetClosestFoodProducerFromMemory(transform.position);
        }
        public Thing GetClosestKnownConsumableProducer(float discludeDistanceLessThan)
        {
            return species.memory.GetClosestFoodProducerFromMemory(transform.position, discludeDistanceLessThan);
        }
        public Thing[] GetKnownConsumeableProducers()
        {
            return species.memory.GetKnownConsumeableProducers();
        }
        public void RequestObservationUpdate()
        {
            //StartCoroutine(observeSurroundings());
            Observe observeComponent = em.GetComponentData<Observe>(ThingEntity);
            // Need to refresh observations //
            if (observeComponent.ObservationAvailable == 0)
            {
                observeComponent.RequestObservation = 1;
            }
            em.SetComponentData(ThingEntity, observeComponent);
        }

        public void SetInView(bool inView)
        {

            if (!inView) // Creature has left camera view
            {
                timeSinceLeftView = .01f;
            }
            else if (timeSinceLeftView > 0)
                timeSinceLeftView = 0;
            this.InView = inView;
        }
        public void SetVisible(bool visible)
        {
            if (visible)
                agent.GetRigidBody().isKinematic = false;
            else
            {
                agent.GetRigidBody().isKinematic = true;
            }
            this.Visible = visible;
        }
        #region
        private void OnBecameVisible()
        {
            SetInView(true);
        }

        private void OnBecameInvisible()
        {
            SetInView(false);
        }

        public void ManualCreatureUpdate(float delta)
        {
            //agent.Update(delta, Visible);
            lastUpdated += delta;
            lastObserved += delta;
            if (timeSinceLeftView > 0)
            {
                timeSinceLeftView += delta;
            }
            if (Visible && timeSinceLeftView > Area.KEEP_CREATURES_VISIBLE_FOR_SECONDS_AFTER_OUT_OF_VIEW)
            {
                timeSinceLeftView = 0;
                SetVisible(false);
            }
            if (DEBUGSCENE && RunDebugMethod)
            {
                RunDebugMethod = false;
                SetInView(!Visible);
            }
            if (lastUpdated > creaturePhysicalStats.updateEvery)
            {
                lastUpdated = 0;
                updateCurrentGridSector();
                updateStateAndTasks(delta);
            }
            // DEBUG
            //RequestObservationUpdate();
        }
        private void updateStateAndTasks(float delta)
        { 
            // CREATURE IS IDLE, LOOK FOR SOMETHING TO DO //
            if (currentState == CREATURE_STATE.IDLE)
            {
                debug("Creature is idle, looking for new task");
                taskManager.GetNewTask(this);
                if (taskManager.hasTask())
                {
                    currentState = CREATURE_STATE.WAIT;
                }
            }
            // CREATURE IS NOT IDLE //
            else
            {
                debug("Performing current tasks - " + taskManager.getCurrentTaskType());
                taskManager.PerformCurrentTask();
                // Task was cancelled, mark creature as idle to get a new task next update //
                if (taskManager.GetCurrentTaskStatus() == Tasks.TASK_STATUS.Cancelled &&
                    currentState != CREATURE_STATE.IDLE)
                {
                    ChangeState(CREATURE_STATE.IDLE);
                }
                if (taskManager.GetCurrentAction() == ActionStep.Actions.MoveTo)
                    if (DEBUGSCENE)
                    {
                        Target target = em.GetComponentData<Target>(ThingEntity);
                        Debug.DrawLine(transform.position, target.targetPosition, Color.cyan, 1f);
                    }

            }
            //Debug.LogWarning("Current state - " + currentState);
            if (!taskManager.hasTask() && !CreatureConstants.CreatureIsIncapacitatedState(currentState))
            {
                debug("No task found, marking Idle");
                currentState = CREATURE_STATE.IDLE;
            }
            // CREATURE PHYSICAL STATS UPDATES //
            if (currentState != CREATURE_STATE.DEAD)
            {
                creaturePhysicalStats.Update();
                Vector3 targetPosition = taskManager.GetCurrentTaskDestination();
                Thing targetThing = taskManager.GetCurrentTaskTarget();
                System.Guid guid = System.Guid.Empty;
                if (targetThing != null)
                    guid = targetThing.guid;
                em.SetComponentData<Target>(ThingEntity, new Target
                {
                    targetGuid = guid,
                    targetPosition = targetPosition
                });
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (currentState == CREATURE_STATE.DEAD) return;
            if (agent.GetRigidBody().velocity.magnitude > 20*2)
            {
                if (currentState != CREATURE_STATE.DEAD)
                {
                    SetStateToDead(CREATURE_DEATH_CAUSE.FlightCollision);
                }
            }
        }
        #endregion

        public Rigidbody GetCreatureAgentBody()
        {
            return agent.GetRigidBody();
        }
        public void SetNavMeshAgentDestination(Vector3 destination)
        {
            setCreatureAgentDestination(destination, false);
        }
        private void setCreatureAgentDestination(Vector3 destination, bool needsToLand)
        {
            Target target = new Target
            {
                targetPosition = destination,
                targetGuid = System.Guid.Empty
            };
            em.SetComponentData(ThingEntity, target);
        }
        public bool AddThingToInventory(Thing thing)
        {
            if (!DEBUGSCENE)
                DebugMenu.AppendDebugLine(thingName + " has picked up " + thing.thingName, this);
            return inventory.addThing(thing);
        }
        public bool RemoveFromInventory(Thing thing)
        {
            return inventory.removeThing(thing);
        }
        public bool AddMemory(Verb verb, Thing subject, bool invertVerb)
        {
            return species.memory.AddMemory(verb,subject.GetBlittableThing(),invertVerb);
        }
        public void SoThisFailed(ActionStep failedStep)
        {
            if (failedStep.associatedTask == Tasks.CreatureTasks.EAT)
            {
                if (failedStep.failReason == ActionStep.FailReason.CouldntGetToTarget ||
                    failedStep.failReason == ActionStep.FailReason.InfinityDistance)
                {
                    AddMemory(Verb.MOVEDTO,Area.GetThingByGUID(failedStep._targetThing), true);
                    //Debug.LogWarning("Memory of couldnt move to " + failedStep._targetThing.thingName);
                }
            }
        }

        IEnumerator observeSurroundings()
        {
            // Observe Things //
            if (lastObserved < observeEvery || awaitingObservation || !initialized)
                yield break;
            lastObserved = 0;
            long start = System.DateTime.Now.ToBinary();
            jobFor = new ObserveJobFor();
            jobFor.allThings = Area.GetBlittableThings();
            jobFor.observeDistance = miscVariables[MiscVariables.CreatureMiscVariables.Observe_Distance];
            jobFor.origin = transform.position;
            jobFor.memories = new NativeArray<MemoryInstance>(jobFor.allThings.Length,Allocator.Persistent);
            jobFor.timestamp = start;
            observeHandle = jobFor.Schedule(jobFor.allThings.Length, 1);
            Area.AddJobHandle(observeHandle);
            //yield return new WaitUntil(() => observeHandle.IsCompleted);
            observeHandle.Complete();
            //Debug.LogWarning("TIme to complete - " + (System.DateTime.Now.ToBinary() - start));
            updateThingsWithinProximityAndDisposeCache();
        }

        private void updateThingsWithinProximityAndDisposeCache()
        {
            for (int count = 0; count < jobFor.memories.Length; count++)
            {
                MemoryInstance memory = jobFor.memories[count];
                if(!memory.IsEmpty())
                    species.memory.AddMemory(memory);
            }
            jobFor.memories.Dispose();
        }
        private void updateCurrentGridSector()
        {
            // Observe Areas //
            currentTerrain = RAKTerrainMaster.GetTerrainAtPoint(transform.position);
            currentSector = currentTerrain.GetSectorAtPos(transform.position);
            bool currentSectorVisited = false;
            for (int sectorCount = 0; sectorCount < knownGridSectorsCount; sectorCount++) 
            {
                if (knownGridSectorsVisited[sectorCount].Equals(currentSector.guid))
                {
                    currentSectorVisited = true;
                }
            }
            if (!currentSectorVisited)
            {
                if(knownGridSectorsCount >= knownGridSectorsVisited.Length)
                {
                    Debug.LogError("Max known grid sectors - " + knownGridSectorsCount);
                }
                knownGridSectorsVisited[knownGridSectorsCount] = currentSector.guid;
                knownGridSectorsCount++;
            }
        }

        public MemoryInstance[] HasAnyMemoriesOf(Verb verb, ConsumptionType consumptionType)
        {
            return species.memory.HasAnyMemoriesOf(verb, consumptionType);
        }
        public MemoryInstance HasAnyMemoryOf(Verb verb, ConsumptionType consumptionType, bool invertVerb)
        {
            return species.memory.HasAnyMemoryOf(verb, consumptionType, invertVerb);
        }
        public bool HasRecentMemoryOf(Verb verb, Thing target, bool invertVerb)
        {
            return species.memory.HasRecentMemoryOf(verb, target, invertVerb);
        }

        public void DeactivateAllParts()
        {
            if (currentState == CREATURE_STATE.DEAD)
            {
                agent.DeactivateAllParts();
            }
            else
            {
                Debug.LogError("Call to destroy all parts when still alive");
            }
        }
        public void ConsumeThing(Thing thing)
        {
            rak.world.World.CurrentArea.RemoveThingFromWorld(thing);
            //getCurrentArea().removeThingFromWorld(thing);
            RemoveFromInventory(thing);
            creaturePhysicalStats.getNeeds().DecreaseNeed(Needs.NEEDTYPE.HUNGER, thing.getWeight());
        }
        public override bool RequestControl(Creature requestor)
        {
            ControlledBy = requestor;
            return true;
        }
        public override Rigidbody RequestRigidBodyAccess(Creature requestor)
        {
            // Check for access //
            if (requestor == ControlledBy)
            {
                return GetCreatureAgentBody();
            }
            return null;
        }

        public Vector3 GetRandomKnownSectorPosition()
        {
            return Vector3.zero;
        }
        public GridSector GetClosestUnexploredSector()
        {
            GridSector closestSector = GridSector.Empty;
            GridSector[] closeSectors = currentTerrain.GetThisGridAndNeighborGrids();
            float closestDistance = Mathf.Infinity;
            foreach (GridSector sector in closeSectors)
            {
                // Already visited //
                bool visited = false;
                for(int count = 0; count < knownGridSectorsCount; count++)
                {
                    if (knownGridSectorsVisited[count].Equals(sector.guid))
                    {
                        visited = true;
                        break;
                    }

                }
                if (visited) continue;
                float distance = Vector3.Distance(sector.GetSectorPosition(), transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSector = sector;
                }
            }
            //Debug.LogWarning("Closest - " + closestSector.name + " current - " + currentSector.name);
            return closestSector;
        }

        #region GETTERS/SETTERS
        public bool HasAgent()
        {
            return agent != null;
        }
        public MemoryInstance[] GetShortTermMemory()
        {
            return species.memory.GetShortTermMemory();
        }
        public void SetUpdateStaggerTime(float staggeredUpdate)
        {
            lastUpdated = staggeredUpdate;
        }
        public Thing GetCurrentActionTarget()
        {
            return taskManager.GetCurrentTaskTarget();
        }
        public Vector3 GetCurrentActionTargetDestination()
        {
            return taskManager.GetCurrentTaskDestination();
        }
        public Tribe GetTribe()
        {
            return memberOfTribe;
        }
        public BASE_SPECIES GetBaseSpecies()
        {
            return species.getBaseSpecies();
        }
        public NeedAmount GetRelativeNeedAmount(Needs.NEEDTYPE need)
        {
            return creaturePhysicalStats.getNeeds().getNeed(need).CurrentAmount;
        }
        public float GetNeedAmount(Needs.NEEDTYPE need)
        {
            return creaturePhysicalStats.getNeeds().getNeed(need).currentAmount;
        }
        public CreatureAgent GetCreatureAgent() { return agent; }
        public ActionStep.Actions GetCurrentAction()
        {
            return taskManager.GetCurrentAction();
        }
        public Tasks.CreatureTasks GetCurrentTask()
        {
            return taskManager.getCurrentTaskType();
        }
        public string GetCurrentTaskTargetName()
        {
            return taskManager.GetCurrentTaskTargetName();
        }
        public CREATURE_STATE GetCurrentState() { return currentState; }

        public void SetStateToDead(CREATURE_DEATH_CAUSE causeOfDeath)
        {
            this.causeOfDeath = causeOfDeath;
            //ChangeState(CREATURE_STATE.DEAD);
            Debug.Log("Avoided death by " + causeOfDeath);
        }
        public void ChangeState(CREATURE_STATE requestedState)
        {
            if (requestedState != currentState)
            {
                // Request to go to Sleep //
                if (requestedState == CREATURE_STATE.SLEEP)
                    agent.DisableAgent();
                // If we need to wake up from sleep //
                else if (currentState == CREATURE_STATE.SLEEP)
                {
                    agent.EnableAgent();
                    Debug.LogWarning("ReEnabling Agent from sleep");
                }
                this.currentState = requestedState;
            }
            else
                Debug.LogError("Call to request an already active state - " + currentState + "-" + requestedState);
        }
        public Area getCurrentArea() { return currentArea; }
        private void debug(string message)
        {
            if (DEBUGSCENE)
                Debug.Log(message);
        }
        public Species getSpecies() { return species; }
        public SpeciesPhysicalStats getCreatureStats()
        {
            return creaturePhysicalStats;
        }
        public float getDistanceFromDestination()
        {
            return agent.GetDistanceFromDestination();
        }
        #endregion
    }
}