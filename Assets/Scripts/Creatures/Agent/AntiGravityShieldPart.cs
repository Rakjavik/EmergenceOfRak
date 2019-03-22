using UnityEngine;

namespace rak.creatures
{
    public class AntiGravityShieldPart : Part
    {
        public bool Activated { get; private set; }
        private ActionStep.Actions[] _OnDuringTheseActions;
        private float ignoreStuckFor = 0; // disable turning on while this is + 0

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
            ignoreStuckFor = time;
        }
        public override void UpdateDerivedPart(ActionStep.Actions action,float delta)
        {
            base.UpdateDerivedPart(action,delta);
            if (action == ActionStep.Actions.MoveTo)
            {
                // Currently Deactivated //
                if (!Activated)
                {
                    bool activate = false;

                    // STUCK //
                    if (attachedAgent.IsStuck())
                    {
                        if (ignoreStuckFor <= 0)
                        {
                            activate = true;
                        }
                    }
                    // Not stuck //
                    else if (!attachedAgent.IsStuck())
                    {
                        // Check for imminent collision //
                        float beforeCollision = attachedAgent.GetTimeBeforeCollision();
                        float collisionProblem = miscVariables[MiscVariables.AgentMiscVariables.Agent_Brake_If_Colliding_In];
                        float velocityNeededToBrake = miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Max_Vel_Mag_Before_Brake];
                        if (Mathf.Abs(beforeCollision) <= collisionProblem && Mathf.Abs(beforeCollision) != Mathf.Infinity &&
                            beforeCollision > .01f &&
                            attachedBody.velocity.magnitude > velocityNeededToBrake)
                        {
                            activate = true;
                        }
                        // Check for spinning //
                        else if (attachedBody.angularVelocity.magnitude >
                            miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Angular_Velocity_Brake_When_Over])
                        {
                            activate = true;
                        }
                        // Check if we're going in the wrong direction if we're not stopped //
                        else if (attachedBody.velocity.magnitude >
                            miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Brake_When_Going_Wrong_Direction_If_Vel])
                        {
                            Vector3 turnNeeded = getDifferenceFromLookAtTargetRotationViaVelocity().eulerAngles;

                            if ((turnNeeded.x > 45 && turnNeeded.x < 315) ||
                                (turnNeeded.z > 45 && turnNeeded.z < 315))
                            {
                                activate = true;
                            }
                        }
                    }
                    if (ignoreStuckFor > 0)
                    {
                        ignoreStuckFor -= Time.deltaTime;
                        //Debug.LogWarning("Sleeping - " + sleepFor);
                    }
                    if (activate)
                    {
                        ActivateShield();
                    }

                }
                // Currently Activated //
                else
                {
                    if (action == ActionStep.Actions.MoveTo)
                    {
                        // Make sure we are pointing in the right direction before deactivating //
                        Vector3 turnNeeded = getDifferenceFromLookAtTargetRotation().eulerAngles;
                        if (turnNeeded.magnitude < 1f || turnNeeded.magnitude > 359)
                        {
                            DeActivateShield();
                            GotoSleep(.15f);
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
                    if (activate)
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