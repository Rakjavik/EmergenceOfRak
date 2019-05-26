using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdatePositionWithECSPosition : MonoBehaviour
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
                Position pos = em.GetComponentData<Position>(entity);
                transform.position = pos.Value;
            }
        }
    }
}
