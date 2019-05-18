using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.world
{
    public struct Sun : IComponentData
    {
        public float AreaLocalTime;
        public rak.world.World.Time_Of_Day TimeOfDay;
        public float DayLength;
        public float TimeInDay;
        public float Xrotation;
        public int ElapsedHours;
    }

    public class SunSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            SunJob job = new SunJob
            {
                delta = Time.deltaTime
            };
            return job.Schedule(this, inputDeps);
        }

        public struct SunJob : IJobForEach<Sun>
        {
            public float delta;

            public void Execute(ref Sun sun)
            {
                sun.AreaLocalTime += delta;
                sun.TimeInDay = sun.AreaLocalTime % sun.DayLength;
                float timePerPeriod = sun.DayLength / 4;
                if (sun.TimeInDay < timePerPeriod)
                    sun.TimeOfDay = rak.world.World.Time_Of_Day.SunRise;
                else if (sun.TimeInDay < timePerPeriod * 2)
                    sun.TimeOfDay = rak.world.World.Time_Of_Day.Midday;
                else if (sun.TimeInDay < timePerPeriod * 3)
                    sun.TimeOfDay = rak.world.World.Time_Of_Day.SunSet;
                else
                    sun.TimeOfDay = rak.world.World.Time_Of_Day.Night;

                int elapsedDays = (int)(sun.AreaLocalTime / sun.DayLength);
                int hourInDay = (int)(sun.TimeInDay * .1f);
                sun.ElapsedHours = hourInDay + (int)(elapsedDays * sun.DayLength * .1f);
                sun.Xrotation = (sun.TimeInDay / sun.DayLength) * 360;
            }
        }
    }
}