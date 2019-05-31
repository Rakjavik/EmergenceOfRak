using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using rak.ecs.ThingComponents;
using Unity.Collections;
using rak.creatures;
using Unity.Mathematics;
using rak.ecs.world;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace rak.ecs.area
{
    public struct Area : IComponentData
    {
        public int NumberOfCreatures;
    }

    public class AreaSystem : MonoBehaviour
    {
        public static int NUMBEROFCREATURES = 2;
        public Entity AreaEntity { get; private set; }
        private bool initialized = false;
        private EntityManager em;
        private static Dictionary<Entity, GameObject> ecsMap;
        public static GameObject GetEntityGO(Entity entity)
        {
            return ecsMap[entity];
        }


        private void initialize()
        {
            bool initializeFruitTrees = true;
            bool createTestFruit = false;

            em = Unity.Entities.World.Active.EntityManager;
            ecsMap = new Dictionary<Entity, GameObject>();
            ecsMap.Add(Entity.Null, null);
            AreaEntity = em.CreateEntity();
            ecsMap.Add(AreaEntity, gameObject);
            em.AddComponentData(AreaEntity, new Sun
            {
                AreaLocalTime = 0,
                DayLength = 240,
                ElapsedHours = 0,
                Xrotation = 0,
            });
            em.AddComponentData(AreaEntity, new Area { });

            rak.world.World world = GameObject.FindObjectOfType<rak.world.World>();
            world.GetComponent<RAKUpdateRotationFromSun>().Initialize(AreaEntity);

            // Initialize fruit trees //
            
            if (initializeFruitTrees)
            {
                FruitTreeECS[] fruitTrees = GameObject.FindObjectsOfType<FruitTreeECS>();
                for (int count = 0; count < fruitTrees.Length; count++)
                {
                    Entity newTree = em.CreateEntity();
                    ecsMap.Add(newTree, fruitTrees[count].gameObject);
                    int spawnThingsEvery = 30;
                    em.AddComponentData(newTree, new Produces
                    {
                        spawnThingEvery = spawnThingsEvery,
                        thingToProduce = Thing.Thing_Types.Fruit,
                        timeSinceLastSpawn = UnityEngine.Random.Range(0, spawnThingsEvery)
                    });
                    em.AddComponentData(newTree, new Position
                    {
                        Value = fruitTrees[count].transform.position
                    });
                    em.AddComponentData(newTree, new Observable
                    {
                        BaseType = Thing.Base_Types.PLANT,
                        Mass = 5000
                    });
                }
            }
            
            if (createTestFruit)
            {
                createFruit(new float3(256, 50, 256));
            }

            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                if (RAKTerrainMaster.Initialized)
                    initialize();
                else
                    return;
            }
            NativeArray<Entity> creatures =
                em.CreateEntityQuery(new ComponentType[] { typeof(IsCreature) }).ToEntityArray(Allocator.TempJob);
            int numOfCreatures = creatures.Length;
            if (numOfCreatures < NUMBEROFCREATURES)
            {
                createGnat();
            }

            // MONO CREATURE UPDATES //
            for(int count = 0; count < numOfCreatures; count++)
            {
                Entity entity = creatures[count];
                float3 origin = em.GetComponentData<Position>(entity).Value;
                float3 destination = em.GetComponentData<Target>(entity).targetPosition;
                Debug.DrawLine(origin, destination, Color.cyan, .2f);

                Target target = em.GetComponentData<Target>(entity);
                TractorBeam tb = em.GetComponentData<TractorBeam>(entity);
                CreatureAI cai = em.GetComponentData<CreatureAI>(entity);

                // AI UPDATES //
                if (!cai.DestroyedThingInPosession.Equals(Entity.Null))
                {
                    Destroy(ecsMap[cai.DestroyedThingInPosession].gameObject);
                    ecsMap.Remove(cai.DestroyedThingInPosession);
                    em.DestroyEntity(cai.DestroyedThingInPosession);
                    cai.DestroyedThingInPosession = Entity.Null;
                    em.SetComponentData(entity, cai);
                }

                // TRACTOR BEAM UPDATES //
                if (tb.RequestLockFromMono == 1)
                {
                    GameObject targetGO = null;
                    ecsMap.TryGetValue(target.targetEntity, out targetGO);
                    if (targetGO != null)
                    {
                        RAKUpdatePositionWithECSTractorBeam utwt = targetGO.AddComponent<RAKUpdatePositionWithECSTractorBeam>();
                        Destroy(targetGO.GetComponent<RAKUpdateECSTransform>());
                        utwt.Initialize(creatures[count]);
                        tb.Locked = 1;
                        tb.RequestLockFromMono = 0;
                        em.SetComponentData(creatures[count], tb);
                    }
                    else
                    {
                        // Target no longer available //
                        targetNotValid(creatures[count]);
                    }
                }
                else if (tb.RequestUnLockFromMono == 1)
                {
                    GameObject targetGO = ecsMap[target.targetEntity];
                    Destroy(targetGO.GetComponent<RAKUpdatePositionWithECSTractorBeam>());
                    targetGO.AddComponent<RAKUpdateECSTransform>().Initialize(entity);
                    tb.RequestUnLockFromMono = 0;
                    tb.Locked = 0;
                    em.SetComponentData(creatures[count], tb);
                }
                // TARGET UPDATES //
                if(target.RequestTargetFromMono == 1)
                {
                    GameObject monoObject = null;
                    ecsMap.TryGetValue(target.targetEntity, out monoObject);
                    if (monoObject != null)
                    {
                        RakUpdateECSTargetWithTransform targetFromMono = monoObject.AddComponent<RakUpdateECSTargetWithTransform>();
                        targetFromMono.Initialize(creatures[count]);
                        target.RequestTargetFromMono = 0;
                        target.LockedToMono = 1;
                        em.SetComponentData(creatures[count], target);
                    }
                    else
                    {
                        Debug.Log("Lock request for invalid target - " + target.targetEntity);
                        targetNotValid(creatures[count]);
                    }
                }
                else if (target.RequestMonoTargetUnlock == 1)
                {
                    GameObject monoObject = null;
                    ecsMap.TryGetValue(target.targetEntity, out monoObject);
                    if (monoObject != null)
                    {
                        Destroy(monoObject.GetComponent<RakUpdateECSTargetWithTransform>());
                        target.RequestMonoTargetUnlock = 0;
                        target.LockedToMono = 0;
                        em.SetComponentData(creatures[count], target);
                    }
                }
                // VISIBILITY UPDATES //
                Visible vis = em.GetComponentData<Visible>(entity);
                Rigidbody rb = ecsMap[entity].GetComponent<Rigidbody>();
                if (vis.RequestVisible == 0 && vis.IsVisible == 1)
                {
                    rb.isKinematic = true;
                    em.AddComponentData(entity, new NonPhysicsMovement
                    {
                        Speed = 150,
                    });
                    em.SetComponentData(entity, new Visible
                    {
                        IsVisible = 0,
                        RequestVisible = 0
                    });
                    GameObject creatureGO = ecsMap[entity];
                    creatureGO.AddComponent<RAKUpdatePositionWithECSPosition>().Initialize(entity);
                    Destroy(creatureGO.GetComponent<RAKUpdateKinematicFromECS>());
                    Destroy(creatureGO.GetComponent<RAKUpdateECSTransform>());
                    Debug.Log("Invisible");
                }
                else if (vis.RequestVisible == 1 && vis.IsVisible == 0)
                {
                    rb.isKinematic = false;
                    em.RemoveComponent<NonPhysicsMovement>(entity);
                    em.SetComponentData(entity, new Visible
                    {
                        IsVisible = 1,
                        RequestVisible = 1
                    });
                    GameObject creatureGO = ecsMap[entity];
                    Destroy(creatureGO.GetComponent<RAKUpdatePositionWithECSPosition>());
                    creatureGO.AddComponent<RAKUpdateECSTransform>().Initialize(entity);
                    creatureGO.AddComponent<RAKUpdateKinematicFromECS>().Initialize(entity);
                    Debug.Log("Visible");
                }
            }
            creatures.Dispose();
            // MONO PRODUCER UPDATES //
            NativeArray<Entity> producers =
                em.CreateEntityQuery(new ComponentType[] { typeof(Produces) }).ToEntityArray(Allocator.TempJob);
            for(int count = 0; count < producers.Length; count++)
            {
                Produces prod = em.GetComponentData<Produces>(producers[count]);
                if(prod.ProductionAvailable == 1)
                {
                    float3 producerPos = em.GetComponentData<Position>(producers[count]).Value;
                    if(prod.thingToProduce == Thing.Thing_Types.Fruit)
                        createFruit(producerPos);
                    prod.ProductionAvailable = 0;
                    prod.timeSinceLastSpawn = 0;
                    em.SetComponentData(producers[count], prod);
                }
            }
            producers.Dispose();
        }

        private void targetNotValid(Entity entity)
        {
            Debug.Log("Target no longer available");
            em.SetComponentData(entity, new Target
            {
                targetEntity = Entity.Null,
                targetPosition = float3.zero
            });
            CreatureAI ai = em.GetComponentData<CreatureAI>(entity);
            ai.CurrentStepStatus = Tasks.TASK_STATUS.Failed;
            ai.FailReason = ActionStep.FailReason.TargetNoLongerAvailable;
            em.SetComponentData(entity, ai);
        }

        private void createFruit(float3 producerPos)
        {
            Entity newFruit = em.CreateEntity();
            em.AddComponentData(newFruit, new Age
            {
                MaxAge = 10
            });
            em.AddComponentData(newFruit, new Enabled { Value = 1 });
            em.AddComponentData(newFruit, new Observable
            {
                BaseType = Thing.Base_Types.PLANT,
                Mass = 1
            });
            em.AddComponentData(newFruit, new Position { });
            em.AddComponentData(newFruit, new Rotation { });
            em.AddComponentData(newFruit, new Claimed { });

            GameObject prefab = RAKUtilities.getThingPrefab("fruitECS");
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.name = newFruit.ToString();
            gameObject.transform.position = producerPos;
            gameObject.GetComponent<RAKUpdateECSTransform>().Initialize(newFruit);

            ecsMap.Add(newFruit, gameObject);
        }

        private void createGnat()
        {
            Entity newGnat = em.CreateEntity();

            em.AddComponentData(newGnat, new EngineConstantForce { });
            em.AddComponentData(newGnat, new Engine
            {
                moveType = CreatureLocomotionType.Flight, // Engine movement type (Flight)
                objectBlockDistance = 10, // Distance a raycast forward has to be below before alt flight logic for being blocked
                sustainHeight = 15, // Target height when in flight
                MaxForceX = 8, // Max force for ConstantForceComponent
                MaxForceY = 15,
                MaxForceZ = 8,
                MinForceX = -8, // Min force for ConstantForceComponent
                MinForceY = 8,
                MinForceZ = -8,
                CurrentStateX = MovementState.IDLE,
                CurrentStateY = MovementState.IDLE,
                CurrentStateZ = MovementState.IDLE,
                VelWhenMovingWithoutPhysics = 20
            });
            em.AddComponentData(newGnat, new EngineRotationTurning
            {
                RotationSpeed = 20, // Modifier for slerp between
            });
            em.AddComponentData(newGnat, new Position {
            });
            em.AddComponentData(newGnat, new Rotation { });
            em.AddComponentData(newGnat, new Velocity { });
            em.AddComponentData(newGnat, new IsCreature { });
            em.AddComponentData(newGnat, new Agent
            {
                UpdateDistanceEvery = .25f, // How often to add a new entry to distance traveled
            });
            em.AddComponentData(newGnat, new Visible
            {
                RequestVisible = 1,
                IsVisible = 1
            });
            em.AddComponentData(newGnat, new CreatureAI
            {
                CurrentAction = ActionStep.Actions.None,
                PreviousSteps = em.AddBuffer<ActionStepBufferPrevious>(newGnat),
                CurrentSteps = em.AddBuffer<ActionStepBufferCurrent>(newGnat),
            });
            em.AddComponentData(newGnat, new Target { });
            em.AddComponentData(newGnat, new Observe
            {
                ObserveDistance = 100, // Distance from creature before creature can see it
                memoryBuffer = em.AddBuffer<ObserveBuffer>(newGnat)
            });
            em.AddComponentData(newGnat, new Observable { });
            em.AddComponentData(newGnat, new ShortTermMemory
            {
                CurrentMemoryIndex = 0,
                MaxShortTermMemories = 100,
                memoryBuffer = em.AddBuffer<CreatureMemoryBuf>(newGnat)
            });
            em.AddComponentData(newGnat, new ThingComponents.Needs { });
            em.AddComponentData(newGnat, new RelativeDirections { });
            em.AddComponentData(newGnat, new AntiGravityShield
            {
                BrakeIfCollidingIn = .5f, // Use brake mechanism if a velocity collision is happening
                EngageIfWrongDirectionAndMovingFasterThan = 15, // Velocity magnitude before brake will kick in if going wrong direction from target
                VelocityMagNeededBeforeCollisionActivating = 20, // If colliding shortly, this minimum magnitude needs to be met before brake
                Activated = 0,
            });
            em.AddComponentData(newGnat, new TractorBeam
            {
                BeamStrength = 1, // Movement modifier
                UnlockAtDistance = .5f, // Drop lock at this distance or less
            });
            em.AddComponentData(newGnat, new CreatureState { Value = Creature.CreatureState.IDLE });

            GameObject prefab = RAKUtilities.getCreaturePrefab("GnatECS");
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.position = new Vector3(256, 50, 256);
            gameObject.GetComponent<RAKUpdateConstantForceFromECS>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSTransform>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSVelocity>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateKinematicFromECS>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSRelativeDirections>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateRotationFromEngine>().Initialize(newGnat);

            ecsMap.Add(newGnat, gameObject);
        }
    }
}
