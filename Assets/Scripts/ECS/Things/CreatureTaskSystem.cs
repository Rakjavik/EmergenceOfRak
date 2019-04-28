/*using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace rak.ecs.ThingComponents
{
    public struct CreatureTask : IComponentData
    {
        public Tasks.CreatureTasks CurrentTask;
        public Tasks.TASK_STATUS TaskStatus;
        public DynamicBuffer<ActionStepBufferCurrent> CurrentSteps;
        public DynamicBuffer<ActionStepBufferPrevious> PreviousSteps;
        public int CurrentStep;
    }

    [InternalBufferCapacity(10)]
    public struct ActionStepBufferCurrent : IBufferElementData
    {
        public ActionStep ActionStep;
    }
    [InternalBufferCapacity(10)]
    public struct ActionStepBufferPrevious : IBufferElementData
    {
        public ActionStep ActionStep;
    }

    public class CreatureTaskSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CreatureTaskJob job = new CreatureTaskJob
            {

            };
            return job.Schedule(this, inputDeps);
        }

        struct CreatureTaskJob : IJobForEach<CreatureAI, CreatureTask>
        {
            public void Execute(ref CreatureAI cai, ref CreatureTask ct)
            {
                // Copy current task steps to previous steps //
                NativeArray<ActionStepBufferCurrent> lastSteps = ct.CurrentSteps.AsNativeArray();
                ct.PreviousSteps.Clear();
                for (int count = 0; count < lastSteps.Length; count++)
                {
                    ct.PreviousSteps.Add(new ActionStepBufferPrevious { ActionStep = ct.CurrentSteps[count].ActionStep });
                }
                lastSteps.Dispose();

                if (cai.CurrentStepStatus == Tasks.TASK_STATUS.Complete)
                {
                    // More tasks to perform //
                    if (ct.CurrentStep != ct.CurrentSteps.Length - 1)
                    {
                        ct.CurrentStep++;
                        // Copy target information from previous to current step if none exists //
                        ActionStep newTargetStep = ct.CurrentSteps[ct.CurrentStep].ActionStep;
                        if (!ct.CurrentSteps[ct.CurrentStep].ActionStep.HasTargetThing() &&
                            ct.CurrentSteps[ct.CurrentStep - 1].ActionStep.HasTargetThing())
                        {
                            newTargetStep._targetThing = ct.CurrentSteps[ct.CurrentStep - 1].ActionStep._targetThing;
                            ct.CurrentSteps[ct.CurrentStep] = new ActionStepBufferCurrent { ActionStep = newTargetStep };
                        }
                        if (!ct.CurrentSteps[ct.CurrentStep].ActionStep.HasTargetPosition() &&
                            ct.CurrentSteps[ct.CurrentStep - 1].ActionStep.HasTargetPosition())
                        {
                            newTargetStep._targetPosition = ct.CurrentSteps[ct.CurrentStep - 1].ActionStep._targetPosition;
                            ct.CurrentSteps[ct.CurrentStep] = new ActionStepBufferCurrent { ActionStep = newTargetStep };
                        }
                        cai.CurrentAction = newTargetStep.Action;
                        cai.CurrentStepStatus = Tasks.TASK_STATUS.Incomplete;
                        cai.ElapsedTime = 0;
                        ct.TaskStatus = Tasks.TASK_STATUS.Incomplete;
                    }
                    // Steps complete //
                    else
                    {
                        ct.TaskStatus = Tasks.TASK_STATUS.Complete;
                    }
                }
                else if (cai.CurrentStepStatus == Tasks.TASK_STATUS.Failed)
                {
                    ct.TaskStatus = Tasks.TASK_STATUS.Cancelled;
                }
            }
        }


    }
}
*/