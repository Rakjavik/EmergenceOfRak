using UnityEngine;
using Unity.Entities;
using rak.ecs.world;

namespace rak.creatures
{
    public class RAKUpdateRotationFromSun : MonoBehaviour
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
                Sun sun = em.GetComponentData<Sun>(entity);
                transform.rotation = Quaternion.Euler(new Vector3(sun.Xrotation, 0, 0));
            }
        }
    }
}
