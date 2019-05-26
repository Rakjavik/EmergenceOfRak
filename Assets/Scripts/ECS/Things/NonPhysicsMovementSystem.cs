using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct NonPhysicsMovement : IComponentData
    {
        public float Speed;
    }

    public class NonPhysicsMovementSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NonPhysicsJob job = new NonPhysicsJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        struct NonPhysicsJob : IJobForEach<Position, NonPhysicsMovement,Target>
        {
            public float delta;

            public void Execute(ref Position pos, ref NonPhysicsMovement npm, ref Target target)
            {
                float3 newPosition = Vector3.MoveTowards(pos.Value, target.targetPosition, npm.Speed * delta);
                pos.Value = newPosition;
            }
        }
    }
}
