using UnityEngine;

namespace rak.creatures
{
    public class AntiGravityShieldPart : Part
    {
        public bool Activated { get; private set; }
        private ActionStep.Actions[] _OnDuringTheseActions;
        private float sleepFor = 0; // disable turning on while this is + 0

        public AntiGravityShieldPart(CreaturePart creaturePart, Transform transform, float updateEvery,
            Rigidbody bodyToShield, ActionStep.Actions[] actions)
            : base(creaturePart, transform, updateEvery)
        {
            this.PartType = creaturePart;
            this.PartTransform = transform;
            this.UpdateEvery = updateEvery;
            Activated = bodyToShield.isKinematic;
            this.attachedBody = bodyToShield;
            this._OnDuringTheseActions = actions;
        }

        private void ActivateShield()
        {
            //Debug.LogWarning("Activate shield");
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
            attachedBody.isKinematic = false;
            Activated = false;
        }
        private void GotoSleep(float time)
        {
            sleepFor = time;
        }
        public override void UpdateDerivedPart(ActionStep.Actions action)
        {
            base.UpdateDerivedPart(action);
            if (action == ActionStep.Actions.MoveTo)
            {
                // Currently Deactivated //
                if (!Activated)
                {
                    bool activate = false;

                    // STUCK //
                    if (attachedAgent.IsStuck())
                    {
                        activate = true;
                    }
                    // Not stuck, do additional checks //
                    else if (!attachedAgent.IsStuck())
                    {
                        // Check for imminent collision //
                        Vector3 beforeCollision = attachedAgent.GetTimeBeforeCollision();
                        float collisionProblem = 10;
                        if (Mathf.Abs(beforeCollision.z) <= collisionProblem && Mathf.Abs(beforeCollision.z) != Mathf.Infinity &&
                            beforeCollision.z > .1f &&
                            attachedBody.velocity.magnitude > 5)
                        {
                            activate = true;
                        }
                    }
                    
                    // Check if we're going in the wrong direction if we're not stopped //
                    else if (attachedBody.velocity.magnitude > 12)
                    {
                        Vector3 turnNeeded = getDifferenceFromLookAtTargetRotationViaVelocity().eulerAngles;
                        if ((turnNeeded.x > 10 && turnNeeded.x < 350) ||
                            (turnNeeded.z > 10 && turnNeeded.z < 350))
                        {
                            activate = true;
                        }
                    }
                    if (sleepFor > 0) sleepFor -= Time.deltaTime;
                    if (activate && sleepFor <= 0)
                        ActivateShield();
                    
                }
                // Currently Activated //
                else
                {
                    if (action == ActionStep.Actions.MoveTo)
                    {
                        // Make sure we are pointing in the right direction before deactivating //
                        Vector3 turnNeeded = getDifferenceFromLookAtTargetRotation().eulerAngles;
                        if (turnNeeded.magnitude < 5f || turnNeeded.magnitude > 355)
                        {
                            DeActivateShield();
                            GotoSleep(.05f);
                        }
                    }
                }

            }
            else
            {
                if (!Activated)
                {
                    bool activate = false;
                    for (int count = 0; count < _OnDuringTheseActions.Length; count++)
                    {
                        if (_OnDuringTheseActions[count] == action)
                        {
                            activate = true;
                            break;
                        }
                    }
                    if(activate)
                        ActivateShield();
                }
            }

        }
        private Vector3 getNeededDirection()
        {
            Vector3 direction = (attachedAgent.GetCurrentActionDestination() - parentCreature.transform.position).normalized;
            return direction;
        }
        private Quaternion getDifferenceFromLookAtTargetRotation()
        {
            Quaternion start = attachedBody.transform.rotation;
            Quaternion desired = Quaternion.LookRotation(getNeededDirection());
            return start * Quaternion.Inverse(desired);
        }
        private Quaternion getDifferenceFromLookAtTargetRotationViaVelocity()
        {
            Quaternion start = Quaternion.LookRotation(attachedBody.velocity);
            Quaternion desired = Quaternion.LookRotation(getNeededDirection());
            return start * Quaternion.Inverse(desired);
        }
    }
}