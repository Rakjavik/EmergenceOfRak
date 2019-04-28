using rak.creatures;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct CreatureAI : IComponentData
    {
        public ActionStep.Actions CurrentAction;
        public ActionStep.FailReason FailReason;
        public Tasks.TASK_STATUS CurrentStepStatus;
        public byte DestinationSet;
        public float ElapsedTime;
        public float MaxAllowedTime;
        public float DistanceForCompletion;
        public ConsumptionType ConsumptionType;
        public byte IsKinematic;
    }

    public class CreatureAISystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(new EntityQueryDesc[] { new EntityQueryDesc {
                Any = new ComponentType[]{typeof(ShortTermMemory)}
            } }));
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CreatureAIJob job = new CreatureAIJob
            {
                Delta = Time.deltaTime,
                memoryBuffers = GetBufferFromEntity<CreatureMemoryBuf>()
            };
            return job.Schedule(this, inputDeps);
        }

        struct CreatureAIJob : IJobForEachWithEntity<CreatureAI, Target,Observe,ShortTermMemory>
        {
            public float Delta;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> memoryBuffers;

            public void Execute(Entity entity, int index, ref CreatureAI cai, ref Target target, ref Observe obs, 
                ref ShortTermMemory stm)
            {
                if(cai.CurrentAction == ActionStep.Actions.None)
                {
                    DynamicBuffer<CreatureMemoryBuf> memoryBuffer = memoryBuffers[entity];
                    int memoryLength = memoryBuffer.Length;
                    bool found = false;
                    if (memoryBuffer.IsCreated && memoryLength > 1000)
                    {
                        for (int count = 0; count < memoryLength; count++)
                        {
                            if(memoryBuffer[count].memory.Edible == 1)
                            {
                                target.targetGuid = stm.memoryBuffer[count].memory.subject;
                                target.targetPosition = stm.memoryBuffer[count].memory.Position;
                                cai.CurrentAction = ActionStep.Actions.MoveTo;
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        if (obs.ObservationAvailable == 0)
                            obs.RequestObservation = 1;
                        target.targetGuid = System.Guid.Empty;
                        Unity.Mathematics.Random random = new Unity.Mathematics.Random();
                        random.InitState((uint)(Delta*500));
                        random.NextInt();
                        int randomPosX = random.NextInt(512);
                        int randomPosZ = random.NextInt(512);
                        target.targetPosition = new float3(randomPosX, 50, randomPosZ);
                        cai.CurrentAction = ActionStep.Actions.MoveTo;
                    }
                }

                else if (cai.CurrentAction == ActionStep.Actions.MoveTo)
                {
                    if(target.distance < 50 || target.targetPosition.Equals(float3.zero))
                    {
                        cai.CurrentAction = ActionStep.Actions.None;
                    }
                }
            }
        }
    }
}
