using rak.creatures;
using rak.creatures.memory;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace rak.ecs.ThingComponents
{
    public struct CreatureAI : IComponentData
    {
        public ActionStep.Actions CurrentAction;
        public Tasks.CreatureTasks CurrentTask;
        public ActionStep.FailReason FailReason;
        public Tasks.TASK_STATUS CurrentStepStatus;
        public byte DestinationSet;
        public float ElapsedTime;
        public float MaxAllowedTime;
        public float DistanceForCompletion;
        public ConsumptionType ConsumptionType;
        public DynamicBuffer<ActionStepBufferCurrent> CurrentSteps;
        public DynamicBuffer<ActionStepBufferPrevious> PreviousSteps;
        public int CurrentStepNum;
        public Entity DestroyedThingInPosession;
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

    public class CreatureAISystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            EndSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CreatureAIJob job = new CreatureAIJob
            {
                Delta = Time.deltaTime,
                memoryBuffers = GetBufferFromEntity<CreatureMemoryBuf>(),
                currentBuffers = GetBufferFromEntity<ActionStepBufferCurrent>(),
                previousBuffers = GetBufferFromEntity<ActionStepBufferPrevious>(),
                commandBuffer = EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                tractorBeams = GetComponentDataFromEntity<TractorBeam>(),
                random = new Random((uint)UnityEngine.Random.Range(1, 100000)),
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handle);
            return handle;
        }

        struct CreatureAIJob : IJobForEachWithEntity<CreatureAI, Target,Observe,ShortTermMemory,Visible,Position>
        {
            public float Delta;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<CreatureMemoryBuf> memoryBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<ActionStepBufferCurrent> currentBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<ActionStepBufferPrevious> previousBuffers;
            [ReadOnly]
            public ComponentDataFromEntity<TractorBeam> tractorBeams;

            public EntityCommandBuffer.Concurrent commandBuffer;

            public Random random;

            public void Execute(Entity entity, int index, ref CreatureAI cai, ref Target target, ref Observe obs, 
                ref ShortTermMemory stm, ref Visible av, ref Position pos)
            {

                // STEP COMPLETE //
                if (cai.CurrentStepStatus == Tasks.TASK_STATUS.Complete)
                {
                    cai.CurrentStepNum++;
                    DynamicBuffer<ActionStepBufferCurrent> currentBuffer = currentBuffers[entity];
                    // Check if task is complete //
                    if (cai.CurrentStepNum >= currentBuffer.Length)
                    {
                        cai.CurrentAction = ActionStep.Actions.None;
                        target.targetEntity = Entity.Null;
                        target.targetPosition = float3.zero;
                    }
                    else
                    {
                        cai.CurrentAction = currentBuffers[entity][cai.CurrentStepNum].ActionStep.Action;
                        cai.CurrentStepStatus = Tasks.TASK_STATUS.Started;
                    }
                }
                // STEP FAILED //
                else if (cai.CurrentStepStatus == Tasks.TASK_STATUS.Failed)
                {
                    cai.CurrentAction = ActionStep.Actions.None;
                    if(cai.FailReason == ActionStep.FailReason.TargetNoLongerAvailable)
                    {
                        rememberTargetAsUnavailable(entity, ref target);
                    }
                }
                if (cai.CurrentAction == ActionStep.Actions.None)
                {
                    getNewTask(ref entity);
                    cai.CurrentStepNum = 0;
                    cai.CurrentAction = currentBuffers[entity][0].ActionStep.Action;
                    cai.CurrentTask = currentBuffers[entity][0].ActionStep.associatedTask;
                    cai.CurrentStepStatus = Tasks.TASK_STATUS.Started;
                }
                // LOCATE //
                else if(cai.CurrentAction == ActionStep.Actions.Locate)
                {
                    DynamicBuffer<ActionStepBufferCurrent> currentBuffer = currentBuffers[entity];
                    locate(ref entity, ref target, ref stm, ref obs, ref cai,ref av,ref pos, currentBuffer.Length,
                        index);
                }
                //  MOVETO  //
                else if (cai.CurrentAction == ActionStep.Actions.MoveTo)
                {
                    commandBuffer.AddComponent(index, entity, new PerformObservation { });
                    moveTo(ref target, ref cai,ref obs);
                }
                // ADD  //
                else if (cai.CurrentAction == ActionStep.Actions.Add)
                {
                    add(ref target, ref cai);
                }
                //  EAT  //
                else if (cai.CurrentAction == ActionStep.Actions.Eat)
                {
                    eat(ref cai, ref target, ref stm, ref entity,index);
                }
            }

            private void getNewTask(ref Entity entity)
            {
                DynamicBuffer<ActionStepBufferPrevious> previousBuffer = previousBuffers[entity];
                DynamicBuffer<ActionStepBufferCurrent> currentBuffer = currentBuffers[entity];

                // Copy current steps to previous //
                int bufferLength = currentBuffer.Length;
                previousBuffer.Clear();
                for(int count = 0; count < bufferLength; count++)
                {
                    previousBuffer.Add(new ActionStepBufferPrevious { ActionStep = currentBuffer[count].ActionStep });
                }
                // Clear out current buffer to get new steps //
                currentBuffer.Clear();

                // Populate current steps //
                Tasks.CreatureTasks chosenTask = Tasks.CreatureTasks.EAT;
                populateBufferWithStepList(ref currentBuffer, chosenTask);
            }

            private void populateBufferWithStepList(ref DynamicBuffer<ActionStepBufferCurrent> buffer, Tasks.CreatureTasks task)
            {
                if(task == Tasks.CreatureTasks.EAT)
                {
                    ActionStep[] steps = new ActionStep[4];
                    steps[0] = new ActionStep
                    {
                        Action = ActionStep.Actions.Locate,
                        associatedTask = Tasks.CreatureTasks.EAT,
                    };
                    steps[1] = new ActionStep
                    {
                        Action = ActionStep.Actions.MoveTo,
                        associatedTask = Tasks.CreatureTasks.EAT,
                    };
                    steps[2] = new ActionStep
                    {
                        Action = ActionStep.Actions.Add,
                        associatedTask = Tasks.CreatureTasks.EAT,
                    };
                    steps[3] = new ActionStep
                    {
                        Action = ActionStep.Actions.Eat,
                        associatedTask = Tasks.CreatureTasks.EAT,
                    };
                    for(int count = 0; count < steps.Length; count++)
                    {
                        buffer.Add(new ActionStepBufferCurrent { ActionStep = steps[count] });
                    }
                }
            }

            private void moveTo(ref Target target, ref CreatureAI cai, ref Observe obs)
            {
                // Constant Observation DEBUG //
                obs.RequestObservation = 1;
                if (target.distance < 10 || target.targetPosition.Equals(float3.zero))
                {
                    cai.CurrentStepStatus = Tasks.TASK_STATUS.Complete;
                }
            }

            private void add(ref Target target,ref CreatureAI cai)
            {
                if(target.distance < .5f)
                {
                    target.RequestMonoTargetUnlock = 1;
                    cai.CurrentStepStatus = Tasks.TASK_STATUS.Complete;
                }
            }

            private void rememberTargetAsUnavailable(Entity entity, ref Target target)
            {
                Debug.Log("Remembering target as unavailable - " + entity);
                DynamicBuffer<CreatureMemoryBuf> buffer = memoryBuffers[entity];
                int bufferLength = buffer.Length;
                for (int count = 0; count < bufferLength; count++)
                {
                    if (buffer[count].memory.InvertVerb == 0 && buffer[count].memory.Verb == Verb.SAW
                        && buffer[count].memory.Subject.Equals(target.targetEntity))
                    {
                        MemoryInstance modified = buffer[count].memory;
                        modified.InvertVerb = 1;
                        buffer[count] = new CreatureMemoryBuf { memory = modified };
                        //MemoryInstance newMemory = buffer[count].memory;
                        //newMemory.InvertVerb = 1;
                        //buffer[count] = new CreatureMemoryBuf { memory = newMemory };
                    }
                }
            }
            private void eat(ref CreatureAI cai,ref Target target,ref ShortTermMemory stm,ref Entity entity,int index)
            {
                cai.CurrentStepStatus = Tasks.TASK_STATUS.Complete;
                cai.DestroyedThingInPosession = target.targetEntity;
                Debug.Log("Requesting destroy of " + target.targetEntity);
                rememberTargetAsUnavailable(entity,ref target);
                target = new Target
                {
                    targetPosition = float3.zero,
                    targetEntity = Entity.Null
                };
            }

            private void locate(ref Entity entity,ref Target target,ref ShortTermMemory stm, ref Observe obs, ref CreatureAI cai,
                ref Visible av, ref Position pos, int currentStepLength,int index)
            {
                DynamicBuffer<CreatureMemoryBuf> memoryBuffer = memoryBuffers[entity];
                int memoryLength = memoryBuffer.Length;
                bool targetFound = false;
                if (memoryBuffer.IsCreated && memoryLength > 0)
                {
                    float closestDistance = float.MaxValue;
                    int closestIndex = -1;
                    for (int count = 0; count < memoryLength; count++)
                    {
                        if (memoryBuffer[count].memory.Edible == 1 && memoryBuffer[count].memory.InvertVerb == 0 && 
                            memoryBuffer[count].memory.Verb == Verb.SAW)
                        {
                            // TOO BIG //
                            if (memoryBuffer[count].memory.SubjectMass > 2)
                                continue;
                            float distance = Vector3.Distance(memoryBuffer[count].memory.Position, pos.Value);
                            if(distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestIndex = count;
                            }
                        }
                    }
                    if (closestIndex != -1)
                    {
                        targetFound = true;
                        target.targetEntity = memoryBuffer[closestIndex].memory.Subject;
                        target.targetPosition = memoryBuffer[closestIndex].memory.Position;
                        target.RequestTargetFromMono = 1;
                        cai.CurrentStepStatus = Tasks.TASK_STATUS.Complete;
                    }
                }
                // Unable to find type needed, explore //
                if (!targetFound)
                {
                    if (obs.ObservationAvailable == 0)
                    {
                        obs.RequestObservation = 1;
                        commandBuffer.AddComponent(index, entity, new PerformObservation { });
                    }
                    target.targetEntity = Entity.Null;
                    random.NextInt();
                    int randomPosX = random.NextInt(512);
                    int randomPosZ = random.NextInt(512);
                    target.targetPosition = new float3(randomPosX, 70, randomPosZ);
                    // Override to explore to point, set current step num so task completes on next go //
                    cai.CurrentAction = ActionStep.Actions.MoveTo;
                    cai.CurrentTask = Tasks.CreatureTasks.EXPLORE;
                    cai.CurrentStepNum = currentStepLength;
                }
            }
        }
    }
}
