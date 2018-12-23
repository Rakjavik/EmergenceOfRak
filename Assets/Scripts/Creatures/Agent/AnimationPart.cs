using UnityEngine;

namespace rak.creatures
{
    public enum PartMovesWith { Velocity, Braking, NA, ConstantForceY, ConstantForceZ }
    public enum PartAnimationType { Movement, Particles }

    public class AnimationPart : Part
    {
        private Vector3 movementDirection;
        private float movementMultiplier;
        private ActionStep.Actions[] AnimateDuring;
        private PartMovesWith partMovesRelativeTo;
        private PartAnimationType animationType;
        private Component animationComponent;
        private float maxParticleCount = 750;

        public AnimationPart(CreaturePart creaturePart, Transform transform, CreatureAnimationMovementType partMovementType, 
            float updateEvery,Vector3 movementDirection,float movementMultiplier,ActionStep.Actions[] animateDuring,
            PartMovesWith partMovesRelativeTo,PartAnimationType animationType)
            : base(creaturePart, transform, updateEvery)
        {
            this.movementDirection = movementDirection;
            this.movementMultiplier = movementMultiplier;
            this.AnimateDuring = animateDuring;
            this.partMovesRelativeTo = partMovesRelativeTo;
            this.animationType = animationType;
            if(animationType == PartAnimationType.Particles)
            {
                animationComponent = PartTransform.gameObject.AddComponent<ParticleSystem>();
                CreatureConstants.SetPropertiesForParticleSystemByCreature((ParticleSystem)animationComponent, parentCreature);
            }
        }

        public void UpdateAnimationPart(ActionStep.Actions currentCreatureAction)
        {
            if (!animateDuringThis(currentCreatureAction))
            {
                return;
            }
            if (animationType == PartAnimationType.Movement)
            {
                animateMovement();   
            }
            else if (animationType == PartAnimationType.Particles)
            {
                animateParticles();
            }
        }

        private void animateParticles()
        {
            ParticleSystem ps = (ParticleSystem)animationComponent;
            ParticleSystem.EmissionModule emission = ps.emission;
            float modifier = attachedAgent.currentBrakeAmount;
            int amount = (int)(maxParticleCount * (modifier * .01f));
            if (amount < 100) amount = 0;
            emission.rateOverTime = amount;
            if(amount > 0 && !ps.isPlaying)
            {
                ps.Play();
                
            }
            if (amount > 0)
            {
                if (audioClip != null)
                {
                    AudioSource audioSource = ps.GetComponentInParent<AudioSource>();
                    if (!audioSource.isPlaying)
                        audioSource.PlayOneShot(audioClip);
                    if (partMovesRelativeTo == PartMovesWith.Braking)
                        audioSource.volume = (attachedAgent.currentBrakeAmount * .01f)/2;
                }
            }
        }
        private void animateMovement()
        {
            float relativeMultiplier;
            if (partMovesRelativeTo == PartMovesWith.Braking)
                relativeMultiplier = attachedAgent.currentBrakeAmount;
            else if (partMovesRelativeTo == PartMovesWith.ConstantForceY)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.y;
            else if (partMovesRelativeTo == PartMovesWith.ConstantForceZ)
                relativeMultiplier = attachedAgent.GetConstantForceComponent().relativeForce.z;
            else if (partMovesRelativeTo == PartMovesWith.Velocity)
                relativeMultiplier = attachedAgent.GetRigidBody().velocity.magnitude;
            else
                relativeMultiplier = 1;
            Vector3 rotation = movementDirection * relativeMultiplier;
            PartTransform.Rotate(rotation * movementMultiplier);
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