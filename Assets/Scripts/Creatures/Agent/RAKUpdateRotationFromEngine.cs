using UnityEngine;
using Unity.Entities;
using rak.ecs.ThingComponents;

namespace rak.creatures
{
    public class RAKUpdateRotationFromEngine : MonoBehaviour
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
                EngineRotationTurning rot = em.GetComponentData<EngineRotationTurning>(entity);
                transform.rotation = rot.RotationUpdate;
            }
        }
    }
}
