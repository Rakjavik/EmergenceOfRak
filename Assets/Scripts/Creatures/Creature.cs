using rak.creatures.memory;
using rak.world;
using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{

    public class Creature : Thing
    {
        public enum CREATURE_STATE { IDLE, MOVE, WAIT, DEAD, SLEEP }

        public bool DEBUGSCENE = false;
        public float TESTVAR1 = 150;
        public float TESTVAR2 = 0.00001f;
        public bool RunDebugMethod = false;
        public BASE_SPECIES baseSpecies;
        public Dictionary<MiscVariables.CreatureMiscVariables, float> miscVariables { get; private set; }

        private CREATURE_STATE currentState;
        private float lastUpdated = 0;
        private bool initialized = false;

        private Species species;
        private SpeciesPhysicalStats creaturePhysicalStats;
        private AudioSource audioSource;
        
        protected Inventory inventory;
        private TaskManager taskManager;

        private CreatureAgent agent;
        private Tribe memberOfTribe;
        private Area currentArea;
        private Dictionary<GridSector,bool> knownGridSectorsVisited;
        
        public bool IsInitialized()
        {
            return initialized;
        }
        public bool IsTargetInFrontOfMe(Thing target)
        {
            bool found = false;
            RaycastHit hit;
            Vector3 origin = transform.position;
            Vector3 destination = target.transform.position - transform.position;
            if (Physics.Raycast(origin,destination,out hit))
            {
                
                Thing possibleHit = hit.collider.GetComponent<Thing>();
                if (possibleHit == null)
                    possibleHit = hit.collider.GetComponentInParent<Thing>();
                if(possibleHit != null)
                {
                    if(possibleHit == target)
                    {
                        if (DEBUGSCENE)
                            Debug.DrawLine(origin, hit.point, Color.yellow, .5f);
                        found = true;
                    }
                    else
                    {
                        if (DEBUGSCENE)
                            Debug.DrawLine(origin, hit.point, Color.gray, .5f);
                    }
                }
            }
            return found;
        }
        public bool IsLanding()
        {
            return agent.IsLanding();
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
        public void Initialize(string name,Area area,Tribe memberOf)
        {
            Initialize(name, area);
            memberOfTribe = memberOf;
        }
        private void Initialize(string name, Area area)
        {
            base.initialize(name);
            this.currentArea = area;
            taskManager = new TaskManager(this);
            inventory = new Inventory(this);
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                species = new Species('m', "Gnat", true, BASE_SPECIES.Gnat,
                    CONSUMPTION_TYPE.OMNIVORE, this);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                species = new Species('n', baseSpecies.ToString(), true, BASE_SPECIES.Gagk, 
                    CONSUMPTION_TYPE.HERBIVORE,this);
            }
            creaturePhysicalStats = CreatureConstants.PhysicalStatsInitialize(species.getBaseSpecies(), this);
            agent = new CreatureAgent(this);
            audioSource = GetComponent<AudioSource>();
            currentState = CREATURE_STATE.IDLE;
            creaturePhysicalStats.Initialize(species.getBaseSpecies());
            agent.Initialize(species.getBaseSpecies());
            miscVariables = MiscVariables.GetCreatureMiscVariables(this);
            memberOfTribe = null;
            knownGridSectorsVisited = new Dictionary<GridSector, bool>();
            initialized = true;
        }
        public void PlayOneShot()
        {
            audioSource.Play();
        }
        public Thing GetClosestKnownReachableConsumable()
        {
            return species.memory.GetClosestFoodFromMemory
                (true, species.getConsumptionType(), transform.position);
        }
        
        #region Mono Methods
        public void Update()
        {
            if (RunDebugMethod)
            {
                taskManager.clearAllTasks();
                RunDebugMethod = false;
            }
            if(!initialized && DEBUGSCENE)
            {
                Initialize("TestCreature", World.CurrentArea);
            }
            float delta = Time.deltaTime;
            base.ManualUpdate(delta);
            agent.Update();
            lastUpdated += delta;
            if (lastUpdated > creaturePhysicalStats.updateEvery)
            {
                lastUpdated = 0;
                if (!CreatureConstants.CreatureIsIncapacitatedState(currentState))
                    observeSurroundings();
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
                    debug("Performing current tasks - " + taskManager.getCurrentTask());
                    taskManager.performCurrentTask();
                    // Task was cancelled, mark creature as idle to get a new task next update //
                    if (taskManager.GetCurrentTaskStatus() == Tasks.TASK_STATUS.Cancelled &&
                        currentState != CREATURE_STATE.IDLE)
                    {
                        ChangeState(CREATURE_STATE.IDLE);
                    }
                    if(taskManager.GetCurrentAction() == ActionStep.Actions.MoveTo)
                        if(DEBUGSCENE)
                            Debug.DrawLine(transform.position, agent.Destination, Color.cyan, 1f);
                    
                }
                //Debug.LogWarning("Current state - " + currentState);
                if (!taskManager.hasTask() && !CreatureConstants.CreatureIsIncapacitatedState(currentState))
                {
                    debug("No task found, marking Idle");
                    currentState = CREATURE_STATE.IDLE;
                }
                // CREATURE PHYSICAL STATS UPDATES //
                if(currentState != CREATURE_STATE.DEAD)
                    creaturePhysicalStats.Update();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            agent.OnCollisionEnter(collision);
            if(agent.GetRigidBody().velocity.magnitude > agent.maxVelocityMagnitude)
            {
                Debug.LogWarning("Creature dying of collision");
                ChangeState(CREATURE_STATE.DEAD);
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            agent.OnCollisionExit(collision);
        }
        #endregion

        public Rigidbody GetCreatureAgentBody()
        {
            return agent.GetRigidBody();
        }
        public void SetNavMeshAgentDestination(Vector3 destination)
        {
            SetCreatureAgentDestination(destination, false);
        }
        public void SetCreatureAgentDestination(Vector3 destination,bool needsToLand)
        {
            agent.SetDestination(destination);
        }
        public bool AddThingToInventory(Thing thing)
        {
            if(!DEBUGSCENE)
                DebugMenu.AppendDebugLine(name + " has picked up " + thing.name,this);
            return inventory.addThing(thing);
        }
        public bool RemoveFromInventory(Thing thing)
        {
            return inventory.removeThing(thing);
        }
        public bool AddMemory(MemoryInstance memory)
        {
            return species.memory.AddMemory(memory);
        }
        public void SoThisFailed(ActionStep failedStep)
        {
            if(failedStep.associatedTask == Tasks.TASKS.EAT)
            {
                if(failedStep.failReason == ActionStep.FailReason.CouldntGetToTarget ||
                    failedStep.failReason == ActionStep.FailReason.InfinityDistance)
                {
                    AddMemory(new MemoryInstance(Verb.MOVEDTO, failedStep._targetThing, true));
                    Debug.LogWarning("Memory of couldnt move to " + failedStep._targetThing.name);
                }
            }
        }
        private void observeSurroundings()
        {
            // Observe Things //

            float thingDistance = miscVariables[MiscVariables.CreatureMiscVariables.Observe_BoxCast_Size_Multiplier];
            Thing[] thingsWithinProximity = CreatureUtilities.GetThingsWithinProximityOf(this, thingDistance);
            foreach (Thing thing in thingsWithinProximity)
            {
                species.memory.AddMemory(new MemoryInstance(Verb.SAW, thing, false));
            }
            float areaDistance = 100;
            // Observe Areas //
            GridSector[] closeAreas = CreatureUtilities.GetPiecesOfTerrainCreatureCanSee(
                this, areaDistance, currentArea.GetClosestTerrainToPoint(transform.position));
            GridSector currentSector = currentArea.GetCurrentGridSector(transform);
            
            foreach (GridSector element in closeAreas)
            {
                if (!knownGridSectorsVisited.ContainsKey(element))
                {
                    knownGridSectorsVisited.Add(element, false);
                }
                if (currentSector == element && !knownGridSectorsVisited[currentSector])
                {
                    Debug.LogWarning("Explored new sector - " + currentSector.name);
                    knownGridSectorsVisited[currentSector] = true;
                }
            }
        }
        public Vector3 BoxCastNotMeGetClosestPoint(float sizeMult,Vector3 direction)
        {
            RaycastHit[] hits = Physics.BoxCastAll(transform.position, Vector3.one * sizeMult, direction);
            Vector3 closest = Vector3.positiveInfinity;
            float closestDistance = float.MaxValue;
            
            for (int count = 0; count < hits.Length; count++)
            {
                if(hits[count].collider.transform.root != gameObject.transform.root && 
                    hits[count].point != Vector3.zero)
                {
                    if(Vector3.Distance(hits[count].point,transform.position) < closestDistance)
                    {
                        closest = hits[count].point;
                        closestDistance = Vector3.Distance(hits[count].point, transform.position);
                    }
                }
            }
            Debug.DrawLine(transform.position, closest,Color.green,.5f);
            return closest;
        }
        public MemoryInstance[] HasAnyMemoriesOf(Verb verb,CONSUMPTION_TYPE consumptionType)
        {
            return species.memory.HasAnyMemoriesOf(verb, consumptionType);
        }
        public MemoryInstance HasAnyMemoryOf(Verb verb,CONSUMPTION_TYPE consumptionType,bool invertVerb)
        {
            return species.memory.HasAnyMemoryOf(verb, consumptionType, invertVerb);
        }
        public bool HasRecentMemoryOf(Verb verb,Thing target,bool invertVerb)
        {
            return species.memory.HasRecentMemoryOf(verb, target, invertVerb);
        }

        public void DestroyAllParts()
        {
            if(currentState == CREATURE_STATE.DEAD)
            {
                agent.DestroyAllParts();
            }
            else
            {
                Debug.LogError("Call to destroy all parts when still alive");
            }
        }
        public void ConsumeThing(Thing thing)
        {
            Area.removeThingFromWorld(thing);
            //getCurrentArea().removeThingFromWorld(thing);
            RemoveFromInventory(thing);
            creaturePhysicalStats.getNeeds().DecreaseNeed(Needs.NEEDTYPE.HUNGER,thing.getWeight());
        }
        public override bool RequestControl(Creature requestor)
        {
            ControlledBy = requestor;
            return true;
        }
        public override Rigidbody RequestRigidBodyAccess(Creature requestor)
        {
            // Check for access //
            if(requestor == ControlledBy)
            {
                return GetCreatureAgentBody();
            }
            return null;
        }
        public GridSector GetClosestUnexploredSector()
        {
            GridSector closestSector = null;
            float closestDistance = Mathf.Infinity;
            foreach (GridSector sector in knownGridSectorsVisited.Keys)
            {
                // Already visited //
                if (knownGridSectorsVisited[sector])
                    continue;
                float distance = Vector2.Distance(sector.GetTwoDLerpOfSector(), transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSector = sector;
                }
            }
            return closestSector;
        }

        #region GETTERS/SETTERS
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
        public Tasks.TASKS GetCurrentTask()
        {
            return taskManager.getCurrentTask();
        }
        public string GetCurrentTaskTargetName()
        {
            return taskManager.GetCurrentTaskTargetName();
        }
        public CREATURE_STATE GetCurrentState() { return currentState; }
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
                if(requestedState == CREATURE_STATE.DEAD)
                {
                    demolish();
                }
                this.currentState = requestedState;
            }
            else
                Debug.LogError("Call to request an already active state - " + currentState + "-" + requestedState);
        }
        public Area getCurrentArea() { return currentArea; }
        private void debug(string message)
        {
            if(DEBUGSCENE)
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
        public Vector3 getNavMeshDestination()
        {
            return agent.Destination;
        }
        private void demolish()
        {
            agent.DisableAgent();
            Vector3 currentVel = agent.GetRigidBody().velocity;
            for (int childCount = 0; childCount < transform.childCount; childCount++)
            {
                GameObject part = transform.GetChild(childCount).gameObject;
                part.transform.SetParent(null);
                if (part.GetComponent<Part>() != null)
                {
                    Rigidbody body = part.AddComponent<Rigidbody>();
                    body.velocity = currentVel;
                }
                else
                {
                    Destroy(transform.GetChild(childCount).gameObject);
                }
            }
        }
        #endregion
    }
}