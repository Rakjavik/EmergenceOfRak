using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public enum NEEDTYPE { NONE, HUNGER, THIRST, TEMPERATURE, SLEEP, REPRODUCTION }
    public struct Needs : IComponentData
    {
        public float Hunger;
        public float Thirst;
        public float Temperature;
        public float Sleep;
        public float Reproduction;
    }

    public class NeedsSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NeedsJob job = new NeedsJob
            {
                Delta = UnityEngine.Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        [BurstCompile]
        struct NeedsJob : IJobForEach<Needs>
        {
            public float Delta;

            public void Execute(ref Needs needs)
            {
                needs.Hunger += Delta;
                needs.Thirst += Delta;
                needs.Temperature += Delta;
                needs.Sleep += Delta;
                needs.Reproduction += Delta;
            }
        }
    }
}
