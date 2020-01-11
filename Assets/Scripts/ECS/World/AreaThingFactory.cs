using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;
using rak.creatures;
using Unity.Mathematics;
using System.Collections.Generic;

namespace rak.ecs.area
{
    public class AreaThingFactory
    {

        public static void CreateFruit (float3 producerPos,EntityManager em,Dictionary<Entity,GameObject> ecsMap)
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
            gameObject.AddComponent<RAKUpdateECSTransform>().Initialize(newFruit);
            ecsMap.Add(newFruit, gameObject);
        }

        public static void CreateGnat(EntityManager em,Dictionary<Entity,GameObject> ecsMap)
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
            em.AddComponentData(newGnat, new Position
            {
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
                DestroyedThingInPosessionIndex = -1,
            });
            em.AddComponentData(newGnat, new Target { });
            em.AddComponentData(newGnat, new Observe
            {
                ObserveDistance = 100, // Distance from creature before creature can see it
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
            gameObject.AddComponent<RAKUpdateConstantForceFromECS>().Initialize(newGnat);
            gameObject.AddComponent<RAKUpdateECSTransform>().Initialize(newGnat);
            gameObject.AddComponent<RAKUpdateECSVelocity>().Initialize(newGnat);
            gameObject.AddComponent<RAKUpdateKinematicFromECS>().Initialize(newGnat);
            gameObject.AddComponent<RAKUpdateECSRelativeDirections>().Initialize(newGnat);
            gameObject.AddComponent<RAKUpdateRotationFromEngine>().Initialize(newGnat);

            ecsMap.Add(newGnat, gameObject);
        }
    }
}
