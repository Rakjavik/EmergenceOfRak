using UnityEngine;
using System.Collections;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateECSTransform : MonoBehaviour
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
        void Update()
        {
            if (initialized)
            {
                em.SetComponentData(entity, new Position { Value = transform.position });
                em.SetComponentData(entity, new Rotation { Value = transform.rotation });
            }
        }
    }
}
