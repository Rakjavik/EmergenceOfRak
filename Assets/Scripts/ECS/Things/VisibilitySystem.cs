using rak.UI;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{

    public struct Visible : IComponentData
    {
        public byte RequestVisible;
        public byte IsVisible;
    }

    public class VisibilitySystem : JobComponentSystem
    {
        private Transform cameraTransform;

        protected override void OnCreate()
        {
            Enabled = true;
            cameraTransform = GameObject.FindObjectOfType<FollowCamera>().transform;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            VisibilityJob job = new VisibilityJob
            {
                cameraPos = cameraTransform.position,
                disableWhenThisFar = 1,
            };
            return job.Schedule(this, inputDeps);
        }

        struct VisibilityJob : IJobForEach<Position,Visible>
        {
            public float3 cameraPos;
            public float disableWhenThisFar;

            public void Execute(ref Position pos, ref Visible vis)
            {
                float distance = Vector3.Distance(pos.Value, cameraPos);
                if (distance >= disableWhenThisFar)
                    vis.RequestVisible = 0;
                else
                    vis.RequestVisible = 1;
                    
            }
        }
    }
}
