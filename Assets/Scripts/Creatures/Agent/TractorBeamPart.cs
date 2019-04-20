using rak.ecs.ThingComponents;
using rak.world;
using UnityEngine;

namespace rak.creatures
{
    public class TractorBeamPart : Part
    {
        private Thing target { get; set; }
        private bool locked { get; set; }
        private Rigidbody targetBody { get; set; }

        public TractorBeamPart(Transform transform, float updateEvery,float beamStrength) : 
            base(CreaturePart.TRACTORBEAM, transform, updateEvery)
        {
            attachedBody = transform.GetComponentInParent<Rigidbody>();
            if (attachedBody == null) Debug.LogError("Can't find Rigidbody for tractor beam part");
            locked = false;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            base.UpdateDerivedPart(action,delta);
            TractorBeam tb = parentCreature.goEntity.EntityManager.
                    GetComponentData<TractorBeam>(parentCreature.goEntity.Entity);
            if (tb.Locked == 1) 
            {
                if (targetBody == null && target == null)
                {
                    target = attachedAgent.GetCurrentActionTarget();
                    targetBody = target.RequestRigidBodyAccess(parentCreature);
                    targetBody.isKinematic = true;
                }
                if (action == ActionStep.Actions.Add)
                    targetBody.position = tb.NewTargetPosition;
            }
            else
            {
                target = null;
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