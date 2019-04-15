using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures
{
    public enum ThingPartAnimationType { RotatePart }
    public class ThingAnimationPart
    {
        private Transform partTransform;
        private float3 direction;
        private float speed;

        public ThingAnimationPart(ThingPartAnimationType type, float3 direction,Transform partTransform, 
            float speed)
        {
            this.direction = direction;
            this.partTransform = partTransform;
            this.speed = speed;
        }

        public void ManualUpdate(float delta)
        {
            partTransform.Rotate(direction * delta*speed);
            //Debug.LogWarning("New rotation - " + partTransform.rotation.eulerAngles);
        }
    }


    public struct ThingAnimationPartJob : IJob
    {
        [ReadOnly]
        private ThingPartAnimationType type;

        [ReadOnly]
        public float delta;

        [ReadOnly]
        private float3 direction;

        public Quaternion rotation;

        public ThingAnimationPartJob(ThingPartAnimationType type, float3 direction)
        {
            this.type = type;
            this.rotation = Quaternion.identity;
            this.delta = 0;
            this.direction = direction;
        }

        public void Execute()
        {
            float3 currentEuler = rotation.eulerAngles;
            currentEuler += direction*delta;
            rotation = Quaternion.Euler(currentEuler);
        }
    }
}