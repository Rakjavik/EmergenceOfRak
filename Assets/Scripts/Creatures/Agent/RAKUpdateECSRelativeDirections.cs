using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateECSRelativeDirections : MonoBehaviour
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

        private void Update()
        {
            if (initialized)
            {
                em.SetComponentData(entity, new RelativeDirections
                {
                    Forward = transform.forward,
                    Right = transform.right
                });
            }
        }
    }

}
