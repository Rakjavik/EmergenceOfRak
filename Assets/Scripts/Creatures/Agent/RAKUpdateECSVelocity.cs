using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateECSVelocity : MonoBehaviour
    {
        private EntityManager em;
        private Entity entity;
        private Rigidbody rb;
        private bool initialized = false;

        // Use this for initialization
        public void Initialize(Entity entity)
        {
            em = World.Active.EntityManager;
            this.entity = entity;
            rb = GetComponent<Rigidbody>();
            initialized = true;
        }

        void Update()
        {
            em.SetComponentData(entity, new Velocity
            {
                AngularVelocity = rb.angularVelocity,
                NormalVelocity = rb.velocity,
                RelativeVelocity = transform.InverseTransformDirection(rb.velocity)
            });
        }
    }
}
