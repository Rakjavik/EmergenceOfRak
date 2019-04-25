using Unity.Entities;
using UnityEngine;
using rak.creatures.memory;
using Unity.Collections;
using Unity.Jobs;
using rak.UI;

namespace rak.ecs.ThingComponents
{
    public struct CreatureBrowser : IComponentData
    {
        [WriteOnly]
        public DynamicBuffer<CreatureMemoryBuf> MemoryBuffer;
    }
    public class CreatureBrowserSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Entity selectedCreature = CreatureBrowserMono.SelectedCreature.ThingEntity;

            CreatureBrowserJob job = new CreatureBrowserJob
            {
                MemoryBuffers = GetBufferFromEntity<CreatureMemoryBuf>(),
                SelectedCreature = selectedCreature
            };

            return job.Schedule(this, inputDeps);
        }

        struct CreatureBrowserJob : IJobForEachWithEntity<CreatureBrowser>
        {
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> MemoryBuffers;
            public Entity SelectedCreature;

            public void Execute(Entity browserEntity, int index, ref CreatureBrowser cb)
            {
                DynamicBuffer<CreatureMemoryBuf> myBuffer = MemoryBuffers[browserEntity];
                DynamicBuffer<CreatureMemoryBuf> SelectedBuffer = MemoryBuffers[SelectedCreature];
                myBuffer.Clear();
                NativeArray<CreatureMemoryBuf> selectedNativeArray = new NativeArray<CreatureMemoryBuf>(SelectedBuffer.AsNativeArray(), Allocator.Temp);
                myBuffer.CopyFrom(selectedNativeArray);
                cb.MemoryBuffer = myBuffer;
                selectedNativeArray.Dispose();
            }
        }
    }
}
