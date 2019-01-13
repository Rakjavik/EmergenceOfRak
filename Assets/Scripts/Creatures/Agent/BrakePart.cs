using UnityEngine;

namespace rak.creatures
{
    public class BrakePart : Part
    {
        public bool Braking { get; private set; }
        public Vector3 CurrentBrakeAmount { get; private set; }
        public bool AngularBraking { get; private set; }

        private Rigidbody bodyToBrake { get; set; }

        public BrakePart(CreaturePart creaturePart, Transform transform, float updateEvery) :
            base(creaturePart, transform, updateEvery)
        {
            CurrentBrakeAmount = Vector3.zero;
            Braking = false;
            AngularBraking = false;
            bodyToBrake = transform.GetComponent<Rigidbody>();
            if (bodyToBrake == null)
                bodyToBrake = transform.GetComponentInParent<Rigidbody>();
            if(bodyToBrake == null)
            {
                Debug.LogError("No rigid body found for braking part");
            }
        }

        public override void UpdateDerivedPart(ActionStep.Actions action)
        {
            base.UpdateDerivedPart(action);
            Vector3 percentToBrake = attachedAgent.CurrentBrakeAmountRequest;
            //Debug.LogWarning("Applying brake - " + percentToBrake);
            if (percentToBrake.x < 0) percentToBrake.x = 0;
            else if (percentToBrake.x > 100) percentToBrake.x = 100;
            if (percentToBrake.y < 0) percentToBrake.y = 0;
            else if (percentToBrake.y > 100) percentToBrake.y = 100;
            if (percentToBrake.z < 0) percentToBrake.z = 0;
            else if (percentToBrake.z > 100) percentToBrake.z = 100;

            float x = percentToBrake.x * .01f;
            float y = percentToBrake.y * .01f;
            float z = percentToBrake.z * .01f;

            Vector3 currentVelocity;
            if (!AngularBraking)
                currentVelocity = bodyToBrake.velocity;
            else
                currentVelocity = bodyToBrake.angularVelocity;
            currentVelocity.x = currentVelocity.x * (1f - x);
            currentVelocity.y = currentVelocity.y * (1f - y);
            currentVelocity.z = currentVelocity.z * (1f - z);
            if (!AngularBraking)
                bodyToBrake.velocity = currentVelocity;
            else
                bodyToBrake.angularVelocity = currentVelocity;
            Braking = true;
            CurrentBrakeAmount = percentToBrake;
        }
    }
}