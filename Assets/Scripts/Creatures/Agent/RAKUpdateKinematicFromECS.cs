using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateKinematicFromECS : MonoBehaviour
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
            if (initialized)
            {
                AntiGravityShield shield = em.GetComponentData<AntiGravityShield>(entity);
                bool shieldActive = shield.Activated == 1;
                bool kinematic = rb.isKinematic;
                if (shieldActive != kinematic)
                    rb.isKinematic = shieldActive;
            }
        }
    }
}
