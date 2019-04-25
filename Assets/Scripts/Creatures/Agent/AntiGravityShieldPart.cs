using rak.ecs.ThingComponents;
using Unity.Entities;
using UnityEngine;

namespace rak.creatures
{
    public class AntiGravityShieldPart : Part
    {
        private Creature creature;
        private bool Activated;

        public AntiGravityShieldPart(CreaturePart creaturePart, Transform transform, float updateEvery,
            Rigidbody bodyToShield, ActionStep.Actions[] actions)
            : base(creaturePart, transform, updateEvery)
        {
            this.PartType = creaturePart;
            this.PartTransform = transform;
            this.UpdateEvery = updateEvery;
            this.attachedBody = bodyToShield;
            this.creature = transform.GetComponentInParent<Creature>();
        }

        private void ActivateShield()
        {
            if (Activated)
            {
                Debug.LogWarning("Call to activate shield when already active");
                return;
            }
            attachedBody.isKinematic = true;
            Activated = true;
        }
        private void DeActivateShield()
        {
            if (!Activated)
            {
                Debug.LogWarning("Call to DeActivate shield when already Deactive");
                return;
            }
            if(creature.Visible && attachedBody.isKinematic == true)
                attachedBody.isKinematic = false;
            Activated = false;
        }
        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            
            AntiGravityShield shield = World.Active.EntityManager.GetComponentData<AntiGravityShield>(creature.ThingEntity);
            bool shieldActive = (shield.Activated == 1);
            if (shieldActive != Activated)
            {
                if (shieldActive)
                    ActivateShield();
                else
                    DeActivateShield();
            }
        }
    }
}