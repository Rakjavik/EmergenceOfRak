using rak.world;
using UnityEngine;

namespace rak.creatures
{
    public class TractorBeamPart : Part
    {
        private Thing target { get; set; }
        private float beamStrength { get; set; }
        private bool locked { get; set; }
        private Rigidbody targetBody { get; set; }

        public TractorBeamPart(Transform transform, float updateEvery,float beamStrength) : 
            base(CreaturePart.TRACTORBEAM, transform, updateEvery)
        {
            attachedBody = transform.GetComponentInParent<Rigidbody>();
            if (attachedBody == null) Debug.LogError("Can't find Rigidbody for tractor beam part");
            locked = false;
            this.beamStrength = beamStrength;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            base.UpdateDerivedPart(action,delta);
            if(action == ActionStep.Actions.Add)
            {
                // Need to lock //
                if (!locked)
                {
                    LockOnTarget();
                }
                // Already locked //
                else
                {
                    if (targetBody != null && target != null)
                    {
                        float distance = Vector3.Distance(targetBody.position, attachedBody.position);
                        //Debug.LogWarning("Tractor beam on, distance - " + distance);
                        Vector3 newPosition = Vector3.MoveTowards(targetBody.position, attachedBody.position, beamStrength * Time.deltaTime);
                        targetBody.position = newPosition;
                    }
                    else
                    {
                        Debug.LogWarning("Tractor beam target is invalid");
                        Debug.Break();
                    }
                }
            }
            else
            {
                if (locked)
                    disengageBeam();
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