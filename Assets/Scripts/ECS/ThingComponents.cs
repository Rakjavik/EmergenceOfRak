using UnityEditor;
using Unity.Mathematics;
using Unity.Entities;
using rak.creatures;
using rak.world;

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
    }

    public struct EngineConstantForce : IComponentData
    {
        public float3 CurrentForce;
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
        public float DistanceFromVel;
        public byte RequestRaycastUpdateDirectionForward;
        public byte RequestRaycastUpdateDirectionDown;
        public byte RequestRaycastUpdateDirectionLeft;
        public byte RequestRaycastUpdateDirectionRight;
        public byte RequestRayCastUpdateDirectionVel;
        public float ZLastUpdated;
        public float YLastUpdated;
        public float VelLastUpdated;
        public float DistanceLastUpdated;
        public float UpdateDistanceEvery;
        public float3 PreviousPositionMeasured;
        public float4 DistanceMoved;
        public int CurrentDistanceIndex;

        public float GetDistanceMoved()
        {
            float distance = DistanceMoved.w + DistanceMoved.x + DistanceMoved.y + DistanceMoved.z;
            //Debug.LogWarning("DIstance moved - " + distance);
            return distance;
        }
    }

    // USED FOR MONO TO WRITE TO ECS VARIABLES FOR PHYSICS //
    public struct AgentVariables : IComponentData
    {
        public float3 RelativeVelocity;
        public float3 Position;
        public float3 Velocity;
        public float4 Rotation;
        public float3 AngularVelocity;

        public float GetVelocityMagnitude()
        {
            return Velocity.x + Velocity.y + Velocity.z;
        }
        public float GetAngularVelocityMag()
        {
            return AngularVelocity.x + AngularVelocity.y + AngularVelocity.z;
        }
    }

    public struct AntiGravityShield : IComponentData
    {
        public byte Activated;
        public float IgnoreStuckFor; // Will ignore being stuck until back to 0
        public float BrakeIfCollidingIn;
        public float VelocityMagNeededBeforeCollisionActivating;
        public float EngageIfWrongDirectionAndMovingFasterThan;
    }

    public struct Target : IComponentData
    {
        public System.Guid targetGuid;
        public float3 targetPosition;
    }

    public struct EngineSound : IComponentData
    {
        public float CurrentLevel;
        public float TargetLevel;
        public float ChangeSpeed;
    }

    public struct EngineRotationTurning : IComponentData
    {
        public float4 RotationUpdate; // Requested rotation to update transform to
        public float RotationSpeed; // Speed modifier for slerp between current and dest rotation
    }

    public struct TractorBeam : IComponentData
    {
        public float BeamStrength;
        public byte Locked;
        public float3 NewTargetPosition;
        public float DistanceFromTarget;
    }
}
