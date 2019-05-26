using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct Agent : IComponentData
    {
        public float DistanceFromFirstZHit;
        public float DistanceFromGround;
        public float DistanceFromLeft;
        public float DistanceFromRight;
        public float DistanceFromVel;
        public byte RequestRaycastUpdateDirectionForward;
        public byte RequestRaycastUpdateDirectionDown;
        public byte RequestRaycastUpdateDirectionLeft;
        public byte RequestRaycastUpdateDirectionRight;
        public byte RequestRayCastUpdateDirectionVel;
        public float ZLastUpdated;
        public float YLastUpdated;
        public float VelLastUpdated;
        public float DistanceLastUpdated;
        public float UpdateDistanceEvery;
        public float3 PreviousPositionMeasured;
        public float4 DistanceMoved;
        public int CurrentDistanceIndex;

        public float GetDistanceMoved()
        {
            float distance = DistanceMoved.w + DistanceMoved.x + DistanceMoved.y + DistanceMoved.z;
            //Debug.LogWarning("DIstance moved - " + distance);
            return distance;
        }
    }

    public class AgentSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AgentJob job = new AgentJob
            {
                currentTime = Time.time
            };
            return job.Schedule(this,inputDeps);
        }
        
        [BurstCompile]
        struct AgentJob : IJobForEach<Agent,Visible,CreatureAI,Position>
        {
            public float currentTime;

            public void Execute(ref Agent agent, ref Visible av,ref CreatureAI ai,ref Position pos)
            {
                // VISIBLE TO CAMERA //
                if (av.Value == 1)
                {
                    if (currentTime - agent.DistanceLastUpdated >= agent.UpdateDistanceEvery)
                    {

                        float3 currentPosition = pos.Value;
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
}
