using UnityEngine;

namespace rak.creatures
{
    public enum PartMovesWith { Velocity, Braking, NA, ConstantForceY, ConstantForceZ, IsKinematic, TargetPosition }
    public enum PartAnimationType { Movement, Particles }

    public class AnimationPart : Part
    {
        private Vector3 movementDirection;
        protected float movementMultiplier;
        private ActionStep.Actions[] AnimateDuring;
        private PartMovesWith partMovesRelativeTo;
        private PartAnimationType animationType;
        private bool visibleIfNotAnimating;

        private bool visible {get; set;}

        public AnimationPart(CreaturePart creaturePart, Transform transform, CreatureAnimationMovementType partMovementType, 
            float updateEvery,Vector3 movementDirection,float movementMultiplier,ActionStep.Actions[] animateDuring,
            PartMovesWith partMovesRelativeTo,PartAnimationType animationType, bool visibleIfNotAnimating)
            : base(creaturePart, transform, updateEvery)
        {
            this.movementDirection = movementDirection;
            this.movementMultiplier = movementMultiplier;
            this.AnimateDuring = animateDuring;
            this.partMovesRelativeTo = partMovesRelativeTo;
            this.animationType = animationType;
            this.visibleIfNotAnimating = visibleIfNotAnimating;
            visible = true;
        }

        public override void UpdateDerivedPart(ActionStep.Actions currentCreatureAction)
        {
            base.UpdateDerivedPart(currentCreatureAction);
            if (!animateDuringThis(currentCreatureAction))
            {
                if (!visibleIfNotAnimating && visible)
                {
                    SetVisibility(false);
                }
                return;
            }
            if (animationType == PartAnimationType.Movement)
            {
                animateMovement();
            }
        }
        
        private void SetVisibility(bool visible)
        {
            if (this.visible == visible) return;
            MeshRenderer renderer = PartTransform.GetComponent<MeshRenderer>();
            if (visible)
            {
                if (renderer != null) renderer.enabled = true;
                for(int count = 0; count < PartTransform.childCount; count++)
                {
                    PartTransform.GetChild(count).gameObject.SetActive(true);
                };
                this.visible = true;
            }
            else
            {
                
                if (renderer != null) renderer.enabled = false;
                for (int count = 0; count < PartTransform.childCount; count++)
                {
                    PartTransform.GetChild(count).gameObject.SetActive(false);
                };
                this.visible = false;
            }
        }
        private void animateMovement()
        {
            float relativeMultiplier;
            if (partMovesRelativeTo == PartMovesWith.Braking)
                relativeMultiplier = attachedAgent.CurrentBrakeAmountRequest.magnitude;
            else if (partMovesRelativeTo == PartMovesWith.ConstantForceY)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.y;
            else if (partMovesRelativeTo == PartMovesWith.ConstantForceZ)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.z;
            else if (partMovesRelativeTo == PartMovesWith.Velocity)
                relativeMultiplier = attachedAgent.GetRigidBody().velocity.magnitude;
            else if (partMovesRelativeTo == PartMovesWith.IsKinematic)
                relativeMultiplier = attachedAgent.IsKinematic();
            else
                relativeMultiplier = 1;
            // NO changes //
            if (relativeMultiplier == 0)
            {
                if (!visibleIfNotAnimating && visible)
                    SetVisibility(false);
            }
            else
            {
                if (!visibleIfNotAnimating && !visible)
                    SetVisibility(true);

                Vector3 rotation = movementDirection * relativeMultiplier;
                PartTransform.Rotate(rotation * movementMultiplier);
            }
        }

        private bool animateDuringThis(ActionStep.Actions action)
        {
            foreach(ActionStep.Actions animatedAction in AnimateDuring)
            {
                if (action == animatedAction)
                    return true;
            }
            return false;
        }
    }
}