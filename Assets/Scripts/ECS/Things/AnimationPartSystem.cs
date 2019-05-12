using rak.creatures;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct AnimationPart : IComponentData
    {
        public float3 MovmeentDirection;
        public float MovementMult;
        public PartMovesWith PartMovesWith;
        public PartAnimationType AnimationType;
        public byte VisibleIfNotAnimating;
        public byte Visible;
        public quaternion CurrentRotation;
    }
    public struct AnimationComponentGroup : IComponentData
    {
        public DynamicBuffer<AnimationBuffer> AnimationParts;
    }
    [InternalBufferCapacity(10)]
    public struct AnimationBuffer : IBufferElementData
    {
        public AnimationPart ac;
    }

    public class AnimationPartSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AnimationPartJob job = new AnimationPartJob
            {
                Delta = Time.deltaTime,
                animationParts = GetBufferFromEntity<AnimationBuffer>(false),
            };
            return job.Schedule(this, inputDeps);
        }

        struct AnimationPartJob : IJobForEachWithEntity<AnimationComponentGroup,AgentVariables,EngineConstantForce,AntiGravityShield>
        {
            public float Delta;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<AnimationBuffer> animationParts;

            public void Execute(Entity entity, int index, ref AnimationComponentGroup ac, ref AgentVariables av, ref EngineConstantForce ecf, 
                ref AntiGravityShield shield)
            {
                DynamicBuffer<AnimationBuffer> buffer = animationParts[entity];
                int partLength = buffer.Length;
                for (int count = 0; count < partLength; count++)
                {
                    AnimationPart single = buffer[count].ac;
                    if (single.AnimationType == PartAnimationType.Movement)
                    {
                        float relativeMult;
                        if (single.PartMovesWith == PartMovesWith.ConstantForceY)
                            relativeMult = ecf.CurrentForce.y;
                        else if (single.PartMovesWith == PartMovesWith.ConstantForceZ)
                            relativeMult = ecf.CurrentForce.z;
                        else if (single.PartMovesWith == PartMovesWith.Velocity)
                            relativeMult = (av.Velocity.x + av.Velocity.y + av.Velocity.z);
                        else if (single.PartMovesWith == PartMovesWith.IsKinematic)
                            relativeMult = shield.Activated;
                        else if (single.PartMovesWith == PartMovesWith.TargetPosition)
                            relativeMult = 0;
                        else
                            relativeMult = 1;

                        float3 rotation = single.MovmeentDirection * relativeMult * Delta;
                        float3 newEulers = new float3(single.CurrentRotation.value.x, single.CurrentRotation.value.y,
                            single.CurrentRotation.value.z) + rotation;
                        single.CurrentRotation = quaternion.Euler(newEulers);
                        buffer[count] = new AnimationBuffer { ac = single };
                    }
                }
            }
        }
    }
}
