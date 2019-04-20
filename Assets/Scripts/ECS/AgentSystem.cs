using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public class AgentSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AgentJob job = new AgentJob
            {
                currentTime = Time.time
            };
            return job.Schedule(this,inputDeps);
        }
        
        //[BurstCompile]
        struct AgentJob : IJobForEach<Agent,AgentVariables,CreatureAI>
        {
            public float currentTime;

            public void Execute(ref Agent agent, ref AgentVariables av,ref CreatureAI ai)
            {
                // VISIBLE TO CAMERA //
                if (av.Visible == 1)
                {
                    if (currentTime - agent.DistanceLastUpdated >= agent.UpdateDistanceEvery)
                    {

                        float3 currentPosition = av.Position;
                        agent.DistanceLastUpdated = currentTime;
                        float distanceMovedSinceLastCheck = Vector3.Distance(
                            agent.PreviousPositionMeasured, currentPosition);
                        int currentIndex = agent.CurrentDistanceIndex;
                        agent.DistanceMoved[currentIndex] = distanceMovedSinceLastCheck;
                        currentIndex += 1;
                        if (currentIndex == 4)
                            currentIndex = 0;
                        agent.CurrentDistanceIndex = currentIndex;
                        agent.PreviousPositionMeasured = currentPosition;
                    }
                }
                // NOT VISIBLE TO CAMERA //
                else
                {
                    ActionStep.Actions currentAction = ai.CurrentAction;
                    
                }
            }
        }
    }
}
