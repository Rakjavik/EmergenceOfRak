using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RakUpdateECSTargetWithTransform : MonoBehaviour
    {
        private EntityManager em;
        private Entity entity;
        private bool initialized = false;

        // Use this for initialization
        public void Initialize(Entity entity)
        {
            em = World.Active.EntityManager;
            this.entity = entity;
            initialized = true;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (initialized)
            {
                Target target = em.GetComponentData<Target>(entity);
                target.targetPosition = transform.position;
                em.SetComponentData(entity, target);
            }
        }
    }
}
