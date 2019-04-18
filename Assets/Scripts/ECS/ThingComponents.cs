using UnityEditor;
using Unity.Mathematics;
using Unity.Entities;
using System;
using rak.creatures;
using rak.world;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct Enabled : IComponentData
    {
        public int Value;
    }

    public struct Age : IComponentData
    {
        public float Value;
        public float MaxAge;
    }

    public struct Produces : IComponentData
    {
        public Thing.Thing_Types thingToProduce;
        public float spawnThingEvery;
        public float timeSinceLastSpawn;
    }

    public struct Engine : IComponentData
    {
        public CreatureLocomotionType moveType;
        public int kinematic;
        public float objectBlockDistance;
        public float sustainHeight;

        public MovementState CurrentStateX;
        public MovementState CurrentStateY;
        public MovementState CurrentStateZ;
        public float MaxForceX;
        public float MaxForceY;
        public float MaxForceZ;
        public float MinForceX;
        public float MinForceY;
        public float MinForceZ;
        public float CurrentForceX;
        public float CurrentForceY;
        public float CurrentForceZ;

    }

    public struct CreatureAI : IComponentData
    {
        public ActionStep.Actions CurrentAction;
    }

    public struct Agent : IComponentData
    {
        public float DistanceFromFirstZHit;
        public float DistanceFromGround;
        public float DistanceFromLeft;
        public float DistanceFromRight;
        public byte RequestRaycastUpdateDirectionForward;
        public byte RequestRaycastUpdateDirectionDown;
        public byte RequestRaycastUpdateDirectionLeft;
        public byte RequestRaycastUpdateDirectionRight;
        public float ZLastUpdated;
        public float YLastUpdated;
    }

    public struct AgentVariables : IComponentData
    {
        public float3 RelativeVelocity;
        public float3 Position;
    }
}
