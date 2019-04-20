using rak.ecs.ThingComponents;
using Unity.Jobs;
using UnityEngine;

namespace rak.creatures
{
    public class TurnPartInching : TurnPart
    {
        public enum InchState { NONE,Rest,Raise,Move }

        public InchState state { get; private set; }

        private HingeJoint joint;
        private Rigidbody inchBody;
        private Rigidbody attachedToBody;
        
        public Direction hingeAxis { get; private set; }

        public TurnPartInching(CreaturePart creaturePart, Transform transform,
            CreatureTurnType turnType, float updateEvery,Direction hingeAxis,
            Transform hingeAttachedTo) :
            base(creaturePart, transform,CreatureTurnType.Inch, updateEvery)
        {
            state = InchState.NONE;
            this.hingeAxis = hingeAxis;
            attachedToBody = hingeAttachedTo.gameObject.AddComponent<Rigidbody>();
            Initialize(hingeAxis);
            
        }

        private void Initialize(Direction hingeAxis)
        {
            inchBody = PartTransform.gameObject.AddComponent<Rigidbody>();
            joint = PartTransform.gameObject.AddComponent<HingeJoint>();
            joint.connectedBody = attachedToBody;
            if (hingeAxis == Direction.X)
                joint.axis = Vector3.right;
            else if (hingeAxis == Direction.Y)
                joint.axis = Vector3.up;
            if (hingeAxis == Direction.Z)
                joint.axis = Vector3.forward;
            JointLimits limits = joint.limits;
            limits.min = -61.897f;
            joint.limits = limits;
            joint.useLimits = true;
            joint.autoConfigureConnectedAnchor = true;

        }
        public Rigidbody GetThisRigidBody()
        {
            return inchBody;
        }
        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            base.UpdateDerivedPart(action,delta);
        }
    }
    public class TurnPartRotation : TurnPart
    {

        public TurnPartRotation(CreaturePart creaturePart, Transform transform,
            CreatureTurnType turnType, float updateEvery) :
            base(creaturePart, transform, CreatureTurnType.Inch, updateEvery){}

        public override void UpdateDerivedPart(ActionStep.Actions action, float delta)
        {
            base.UpdateDerivedPart(action, delta);

            EngineRotationTurning ert = parentCreature.goEntity.EntityManager.
                GetComponentData<EngineRotationTurning>(parentCreature.goEntity.Entity);
            parentCreature.transform.rotation = new Quaternion(ert.RotationUpdate.x, ert.RotationUpdate.y,
                ert.RotationUpdate.z, ert.RotationUpdate.w);
        }
    }
    public abstract class TurnPart : Part
    {
        private CreatureTurnType turnType;
        public TurnPart(CreaturePart creaturePart,Transform transform,
            CreatureTurnType turnType,float updateEvery) : 
            base(creaturePart, transform, updateEvery)
        {
            this.turnType = turnType;
        }
    }
}