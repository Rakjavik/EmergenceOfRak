using rak.ecs.ThingComponents;
using rak.world;
using Unity.Entities;
using UnityEngine;

namespace rak.creatures
{
    public class TractorBeamPart : Part
    {
        private Thing target { get; set; }
        private bool locked { get; set; }
        private Rigidbody targetBody { get; set; }
        private EntityManager em;

        public TractorBeamPart(Transform transform, float updateEvery,float beamStrength) : 
            base(CreaturePart.TRACTORBEAM, transform, updateEvery)
        {
            attachedBody = transform.GetComponentInParent<Rigidbody>();
            if (attachedBody == null) Debug.LogError("Can't find Rigidbody for tractor beam part");
            locked = false;
            em = Unity.Entities.World.Active.EntityManager;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            base.UpdateDerivedPart(action,delta);
            TractorBeam tb = em.
                    GetComponentData<TractorBeam>(parentCreature.ThingEntity);
            Target ecsTarget = em.GetComponentData<Target>(parentCreature.ThingEntity);
            if (tb.Locked == 1) 
            {
                if (targetBody == null)
                {
                    this.target = Area.GetThingByGUID(ecsTarget.targetGuid);
                    targetBody = this.target.RequestRigidBodyAccess(parentCreature);
                    targetBody.isKinematic = true;
                }
                if (action == ActionStep.Actions.Add)
                    targetBody.position = tb.NewTargetPosition;
            }
            else
            {
                this.target = null;
                targetBody = null;
            }
        }
        
        private void disengageBeam()
        {
            locked = false;
            target = null;
            targetBody = null;
        }
        private bool LockOnTarget()
        {
            target = attachedAgent.GetCurrentActionTarget();
            if (target == null)
            {
                Debug.LogWarning("Can't find target for lock");
                disengageBeam();
            }
            else
            {
                if (target.RequestControl(attachedAgent.creature))
                {
                    targetBody = target.RequestRigidBodyAccess(attachedAgent.creature);
                    if (targetBody != null)
                    {
                        targetBody.isKinematic = true;
                        locked = true;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}