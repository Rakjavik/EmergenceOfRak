using UnityEngine;

namespace rak.creatures
{
    public class TractorBeamAnimationPart : AnimationPart
    {
        private ParticleSystem ps { get; set; }
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.ShapeModule shapeModule;
        private float startSpeed = .03f;
        private Transform target = null;

        public TractorBeamAnimationPart(CreaturePart creaturePart, Transform transform, float updateEvery, 
            Vector3 movementDirection, float movementMultiplier) 
            : base(creaturePart, transform, CreatureAnimationMovementType.NONE, updateEvery, movementDirection, 
                  movementMultiplier, new ActionStep.Actions[] { ActionStep.Actions.Add }, PartMovesWith.TargetPosition, 
                  PartAnimationType.Particles, true)
        {
            parentCreature = transform.GetComponentInParent<Creature>();
            ps = transform.GetComponent<ParticleSystem>();
            mainModule = ps.main;
            shapeModule = ps.shape;
            mainModule.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed);
            verifyParticlesAreOff();
        }

        public override void UpdateDerivedPart(ActionStep.Actions currentCreatureAction)
        {
            base.UpdateDerivedPart(currentCreatureAction);
            if (currentCreatureAction == ActionStep.Actions.Add)
            {
                if (target == null)
                {
                    Thing targetThing = parentCreature.GetCurrentActionTarget();
                    if (targetThing == null)
                    {
                        Debug.LogWarning("NO target found for tractor beam");
                        verifyParticlesAreOff();
                    }
                    else
                    {
                        target = targetThing.transform;
                        updateMainModuleStartSpeedBasedOffDistFromTarget();
                        verifyParticlesAreON();
                    }
                }
                else
                {
                    verifyParticlesAreON();
                    updateMainModuleStartSpeedBasedOffDistFromTarget();
                }
            }
            else
            {
                verifyParticlesAreOff();
            }
        }
        private void updateMainModuleStartSpeedBasedOffDistFromTarget()
        {
            float distanceFromTarget = Vector3.Distance(PartTransform.position, target.position);
            mainModule.startSpeed = new ParticleSystem.MinMaxCurve(distanceFromTarget * movementMultiplier);
        }
        private void verifyParticlesAreOff()
        {
            if (ps.isPlaying)
                ps.Stop();
        }
        private void verifyParticlesAreON()
        {
            if (!ps.isPlaying)
                ps.Play();
        }
    }

}