using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using rak.ecs.ThingComponents;
using Unity.Collections;
using rak.creatures;
using Unity.Mathematics;
using rak.ecs.world;

namespace rak.ecs.area
{
    public struct Area : IComponentData
    {
        public int NumberOfCreatures;
    }

    public class AreaSystem : JobComponentSystem
    {
        public Entity AreaEntity { get; private set; }
        private bool initialized = false;

        private void initialize()
        {
            AreaEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(AreaEntity, new Sun
            {
                AreaLocalTime = 0,
                DayLength = 240,
                ElapsedHours = 0,
                Xrotation = 0,
            });
            EntityManager.AddComponentData(AreaEntity, new Area
            {

            });

            rak.world.World world = GameObject.FindObjectOfType<rak.world.World>();
            world.GetComponent<RAKUpdateRotationFromSun>().Initialize(AreaEntity);

            FruitTreeECS[] fruitTrees = GameObject.FindObjectsOfType<FruitTreeECS>();
            for(int count = 0; count < fruitTrees.Length; count++)
            {
                Entity newTree = EntityManager.CreateEntity();
                int spawnThingsEvery = 30;
                EntityManager.AddComponentData(newTree, new Produces
                {
                    spawnThingEvery = spawnThingsEvery,
                    thingToProduce = Thing.Thing_Types.Fruit,
                    timeSinceLastSpawn = UnityEngine.Random.Range(0, spawnThingsEvery)
                });
                EntityManager.AddComponentData(newTree, new Position
                {
                    Value = fruitTrees[count].transform.position
                });
                EntityManager.AddComponentData(newTree, new Observable
                {
                    BaseType = Thing.Base_Types.PLANT,
                    Mass = 5000
                });
            }
            initialized = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!initialized)
            {
                if (RAKTerrainMaster.Initialized)
                    initialize();
                else
                    return inputDeps;
            }
            NativeArray<Entity> creatures =
                GetEntityQuery(new ComponentType[] { typeof(IsCreature) }).ToEntityArray(Allocator.TempJob);
            int numOfCreatures = creatures.Length;
            if (numOfCreatures == 0)
            {
                createGnat();
            }
            for(int count = 0; count < numOfCreatures; count++)
            {
                float3 origin = EntityManager.GetComponentData<Position>(creatures[count]).Value;
                float3 destination = EntityManager.GetComponentData<Target>(creatures[count]).targetPosition;
                Debug.DrawLine(origin, destination, Color.cyan, .5f);
            }
            creatures.Dispose();
            NativeArray<Entity> producers =
                GetEntityQuery(new ComponentType[] { typeof(Produces) }).ToEntityArray(Allocator.TempJob);
            for(int count = 0; count < producers.Length; count++)
            {
                Produces prod = EntityManager.GetComponentData<Produces>(producers[count]);
                if(prod.ProductionAvailable == 1)
                {
                    float3 producerPos = EntityManager.GetComponentData<Position>(producers[count]).Value;
                    if(prod.thingToProduce == Thing.Thing_Types.Fruit)
                        createFruit(producerPos);
                    prod.ProductionAvailable = 0;
                    EntityManager.SetComponentData(producers[count], prod);
                }
            }
            producers.Dispose();
            return inputDeps;
        }

        private void createFruit(float3 producerPos)
        {
            Entity newFruit = EntityManager.CreateEntity();
            EntityManager.AddComponentData(newFruit, new Age
            {
                MaxAge = 10
            });
            EntityManager.AddComponentData(newFruit, new Enabled { Value = 1 });
            EntityManager.AddComponentData(newFruit, new Observable
            {
                BaseType = Thing.Base_Types.PLANT,
                Mass = 1
            });
            EntityManager.AddComponentData(newFruit, new Position { });
            EntityManager.AddComponentData(newFruit, new Rotation { });

            GameObject prefab = RAKUtilities.getThingPrefab("fruitECS");
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.position = producerPos;
            gameObject.GetComponent<RAKUpdateECSTransform>().Initialize(newFruit);
        }

        private void createGnat()
        {
            Entity newGnat = EntityManager.CreateEntity();
            EntityManager.AddComponentData(newGnat, new EngineConstantForce { });
            EntityManager.AddComponentData(newGnat, new Engine
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
            EntityManager.AddComponentData(newGnat, new EngineRotationTurning
            {
                RotationSpeed = 20, // Modifier for slerp between
            });
            EntityManager.AddComponentData(newGnat, new Position {
            });
            EntityManager.AddComponentData(newGnat, new Rotation { });
            EntityManager.AddComponentData(newGnat, new Velocity { });
            EntityManager.AddComponentData(newGnat, new IsCreature { });
            EntityManager.AddComponentData(newGnat, new Agent
            {
                UpdateDistanceEvery = .25f, // How often to add a new entry to distance traveled
            });
            EntityManager.AddComponentData(newGnat, new Visible
            {
                Value = 1
            });
            EntityManager.AddComponentData(newGnat, new CreatureAI
            {
                CurrentAction = ActionStep.Actions.None,
                PreviousSteps = EntityManager.AddBuffer<ActionStepBufferPrevious>(newGnat),
                CurrentSteps = EntityManager.AddBuffer<ActionStepBufferCurrent>(newGnat),
            });
            EntityManager.AddComponentData(newGnat, new Target { });
            EntityManager.AddComponentData(newGnat, new Observe
            {
                ObserveDistance = 100, // Distance from creature before creature can see it
                memoryBuffer = EntityManager.AddBuffer<ObserveBuffer>(newGnat)
            });
            EntityManager.AddComponentData(newGnat, new Observable { });
            EntityManager.AddComponentData(newGnat, new ShortTermMemory
            {
                CurrentMemoryIndex = 0,
                MaxShortTermMemories = 100,
                memoryBuffer = EntityManager.AddBuffer<CreatureMemoryBuf>(newGnat)
            });
            EntityManager.AddComponentData(newGnat, new ThingComponents.Needs { });
            EntityManager.AddComponentData(newGnat, new RelativeDirections { });
            EntityManager.AddComponentData(newGnat, new AntiGravityShield
            {
                BrakeIfCollidingIn = .5f, // Use brake mechanism if a velocity collision is happening
                EngageIfWrongDirectionAndMovingFasterThan = 15, // Velocity magnitude before brake will kick in if going wrong direction from target
                VelocityMagNeededBeforeCollisionActivating = 20, // If colliding shortly, this minimum magnitude needs to be met before brake
                Activated = 0,
            });
            EntityManager.AddComponentData(newGnat, new TractorBeam
            {
                BeamStrength = 1, // Movement modifier
            });
            EntityManager.AddComponentData(newGnat, new CreatureState { Value = Creature.CreatureState.IDLE });

            GameObject prefab = RAKUtilities.getCreaturePrefab("GnatECS");
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.position = new Vector3(256, 50, 256);
            gameObject.GetComponent<RAKUpdateConstantForceFromECS>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSTransform>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSVelocity>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateKinematicFromECS>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateECSRelativeDirections>().Initialize(newGnat);
            gameObject.GetComponent<RAKUpdateRotationFromEngine>().Initialize(newGnat);
        }
    }
}
