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
        public static int NUMBEROFCREATURES = 1;
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
                AreaThingFactory.CreateFruit(new float3(256, 50, 256),em,ecsMap);
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
                AreaThingFactory.CreateGnat(em,ecsMap);
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
                if (cai.DestroyedThingInPosessionIndex != -1)
                {
                    NativeArray<Entity> entities = em.GetAllEntities(Allocator.Temp);
                    if (entities.Length <= cai.DestroyedThingInPosessionIndex)
                    {
                        Debug.LogWarning("Destroy index out of range - " + cai.DestroyedThingInPosessionIndex);
                    }
                    else
                    {
                        Entity entityToDestroy = entities[cai.DestroyedThingInPosessionIndex];
                        Debug.Log("Destroying entity - " + entityToDestroy);
                        Destroy(ecsMap[entityToDestroy].gameObject);
                        ecsMap.Remove(entityToDestroy);
                        em.DestroyEntity(entityToDestroy);
                    }
                    cai.DestroyedThingInPosessionIndex = -1;
                    em.SetComponentData(entity, cai);
                    entities.Dispose();
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
                        AreaThingFactory.CreateFruit(producerPos,em,ecsMap);
                    prod.ProductionAvailable = 0;
                    prod.timeSinceLastSpawn = 0;
                    em.SetComponentData(producers[count], prod);
                }
            }
            producers.Dispose();
        }

        private void targetNotValid(Entity entity)
        {
            //Debug.Log("Target no longer available");
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
    }
}
