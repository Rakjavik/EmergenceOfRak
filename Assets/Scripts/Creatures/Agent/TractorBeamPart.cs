using rak.ecs.ThingComponents;
using rak.world;
using Unity.Entities;
using UnityEngine;

namespace rak.creatures
{
    public class TractorBeamPart : Part
    {
        private Thing target { get; set; }
        private Rigidbody targetBody { get; set; }
        private EntityManager em;

        public TractorBeamPart(Transform transform, float updateEvery,float beamStrength) : 
            base(CreaturePart.TRACTORBEAM, transform, updateEvery)
        {
            attachedBody = transform.GetComponentInParent<Rigidbody>();
            if (attachedBody == null) Debug.LogError("Can't find Rigidbody for tractor beam part");
            em = Unity.Entities.World.Active.EntityManager;
        }

        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            TractorBeam tb = em.
                    GetComponentData<TractorBeam>(parentCreature.ThingEntity);
            Target ecsTarget = em.GetComponentData<Target>(parentCreature.ThingEntity);
            if (tb.Locked == 1)
            {
                if (this.target == null || targetBody == null)
                {
                    this.target = Area.GetThingByEntity(ecsTarget.targetEntity);
                    targetBody = this.target.RequestRigidBodyAccess(parentCreature);
                    targetBody.isKinematic = true;
                }
                /*if (ecsTarget.NeedTargetPositionRefresh == 1)
                {
                    // If locked on target, update ECS with transform info //
                    ecsTarget.targetPosition = target.transform.position;
                    ecsTarget.NeedTargetPositionRefresh = 0;
                    em.SetComponentData(parentCreature.ThingEntity, ecsTarget);
                }
                else
                {*/
                if (action == ActionStep.Actions.Add)
                {
                    targetBody.position = tb.NewTargetPosition;
                    ecsTarget.targetPosition = tb.NewTargetPosition;
                    em.SetComponentData(parentCreature.ThingEntity, ecsTarget);
                }
            }
            else
            {
                this.target = null;
                targetBody = null;
            }
        }
    }
}