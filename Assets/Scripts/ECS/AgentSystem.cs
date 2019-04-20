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
        struct AgentJob : IJobForEach<Agent,AgentVariables>
        {
            public float currentTime;

            public void Execute(ref Agent agent, ref AgentVariables agentVar)
            {
                if (currentTime - agent.DistanceLastUpdated >= agent.UpdateDistanceEvery)
                {

                    float3 currentPosition = agentVar.Position;
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
        }
    }
}
