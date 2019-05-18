using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateConstantForceFromECS : MonoBehaviour
    {
        private EntityManager em;
        private Entity entity;
        private ConstantForce cf;
        private bool initialized = false;

        public void Initialize(Entity entity)
        {
            em = World.Active.EntityManager;
            this.entity = entity;
            cf = GetComponent<ConstantForce>();
            initialized = true;
        }

        void Update()
        {
            if (initialized)
            {
                EngineConstantForce ecf = em.GetComponentData<EngineConstantForce>(entity);
                cf.relativeForce = ecf.CurrentForce;
            }
        }
    }

}
