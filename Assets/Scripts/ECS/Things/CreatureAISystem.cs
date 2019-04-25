using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public class CreatureAISystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CreatureAIJob job = new CreatureAIJob
            {
                Delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        struct CreatureAIJob : IJobForEach<CreatureAI, Target,Observe,ShortTermMemory,AgentVariables>
        {
            public float Delta;

            public void Execute(ref CreatureAI cai, ref Target target, ref Observe obs, ref ShortTermMemory stm,ref AgentVariables av)
            {
                cai.ElapsedTime += Delta;
                if(cai.ElapsedTime > cai.MaxAllowedTime)
                {
                    cai.CurrentStatus = Tasks.TASK_STATUS.Failed;
                    cai.FailReason = ActionStep.FailReason.ExceededTimeLimit;
                    return;
                }

                // LOCATE //
                if(cai.CurrentAction == ActionStep.Actions.Locate)
                {
                    obs.RequestObservation = 1;
                    if(cai.CurrentTask == Tasks.CreatureTasks.EAT)
                    {
                        int length = stm.MaxShortTermMemories;
                        float closestDistance = float.MaxValue;
                        int closestIndex = -1;
                        for (int count = 0; count < length; count++)
                        {
                            if (stm.memoryBuffer[count].memory.Edible == 1)
                            {
                                float distance = Vector3.Distance(av.Position, stm.memoryBuffer[count].memory.Position);
                                if (distance < closestDistance)
                                {
                                    closestDistance = distance;
                                    closestIndex = count;
                                }
                            }
                        }
                        if(closestIndex == -1)
                        {
                            cai.FailReason = ActionStep.FailReason.NoKnownFood;
                            cai.CurrentStatus = Tasks.TASK_STATUS.Failed;
                            return;
                        }
                        else
                        {
                            target.targetGuid = stm.memoryBuffer[closestIndex].memory.subject;
                            target.targetPosition = stm.memoryBuffer[closestIndex].memory.Position;
                            cai.CurrentStatus = Tasks.TASK_STATUS.Complete;
                        }
                    }
                }
                // MOVE TO //
                else if (cai.CurrentAction == ActionStep.Actions.MoveTo)
                {
                    if (target.distance == Mathf.Infinity)
                    {
                        cai.FailReason = ActionStep.FailReason.InfinityDistance;
                        cai.CurrentStatus = Tasks.TASK_STATUS.Failed;
                    }
                    if (cai.CurrentTask == Tasks.CreatureTasks.MOVE_AND_OBSERVE)
                        obs.RequestObservation = 1;
                    if (target.distance <= cai.DistanceForCompletion)
                    {
                        cai.CurrentStatus = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                }
                // ADD //
                else if (cai.CurrentAction == ActionStep.Actions.Add)
                {
                    if(target.distance < .5f)
                    {
                        cai.CurrentStatus = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                }
            }
        }
    }
}
