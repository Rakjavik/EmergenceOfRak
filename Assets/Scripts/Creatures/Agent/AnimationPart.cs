using rak.ecs.ThingComponents;
using Unity.Entities;
using UnityEngine;

namespace rak.creatures
{
    public enum PartMovesWith { Velocity, Braking, NA, ConstantForceY, ConstantForceZ, IsKinematic, TargetPosition }
    public enum PartAnimationType { Movement, Particles }

    public class AnimationPart : Part
    {
        public Vector3 MovementDirection;
        public float MovementMultiplier;
        private ActionStep.Actions[] AnimateDuring;
        public PartMovesWith PartMovesRelativeTo;
        public PartAnimationType AnimationType;
        public bool VisibleIfNotAnimating;
        public int IndexInComponentArray;

        private bool visible {get; set;}

        public AnimationPart(CreaturePart creaturePart, Transform transform, CreatureAnimationMovementType partMovementType, 
            float updateEvery,Vector3 movementDirection,float movementMultiplier,ActionStep.Actions[] animateDuring,
            PartMovesWith partMovesRelativeTo,PartAnimationType animationType, bool visibleIfNotAnimating)
            : base(creaturePart, transform, updateEvery)
        {
            this.MovementDirection = movementDirection;
            this.MovementMultiplier = movementMultiplier;
            this.AnimateDuring = animateDuring;
            this.PartMovesRelativeTo = partMovesRelativeTo;
            this.AnimationType = animationType;
            this.VisibleIfNotAnimating = visibleIfNotAnimating;
            visible = true;
        }

        public override void UpdateDerivedPart(ActionStep.Actions currentCreatureAction,float delta)
        {
            if (!animateDuringThis(currentCreatureAction))
            {
                if (!VisibleIfNotAnimating && visible)
                {
                    SetVisibility(false);
                }
                return;
            }
            if (AnimationType == PartAnimationType.Movement)
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
            if (PartMovesRelativeTo == PartMovesWith.ConstantForceY)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.y;
            else if (PartMovesRelativeTo == PartMovesWith.ConstantForceZ)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.z;
            else if (PartMovesRelativeTo == PartMovesWith.Velocity)
                relativeMultiplier = attachedAgent.GetRigidBody().velocity.magnitude;
            else if (PartMovesRelativeTo == PartMovesWith.IsKinematic)
                relativeMultiplier = attachedAgent.IsKinematic();
            else if (PartMovesRelativeTo == PartMovesWith.TargetPosition)
                relativeMultiplier = 0;
            else
                relativeMultiplier = 1;
            // NO changes //
            if (relativeMultiplier == 0)
            {
                if (!VisibleIfNotAnimating && visible)
                    SetVisibility(false);
            }
            else
            {
                if (!VisibleIfNotAnimating && !visible)
                    SetVisibility(true);

                Vector3 rotation = MovementDirection * relativeMultiplier;
                PartTransform.Rotate(rotation * MovementMultiplier);
            }
            
            // ECS, NOT WORKING RIGHT YET //
            /*DynamicBuffer<AnimationBuffer> buffer = World.Active.EntityManager.
                GetBuffer<AnimationBuffer>(parentCreature.ThingEntity);
            PartTransform.rotation = buffer[IndexInComponentArray].ac.CurrentRotation;*/
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