using rak.world;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures
{
    public class LightArmPart : Part
    {
        public bool Deployed { get; private set; }
        private Vector3 target;
        private Transform followLightY;
        private Transform armPivotRot;
        private Transform armParent;
        private Light light;
        private float followLightHomePosition;
        private float followLightDeployedPosition;
        private float followLightMovementSpeed;

        public LightArmPart(CreaturePart creaturePart, Transform transform, float updateEvery) : base(creaturePart, transform, updateEvery)
        {
            followLightY = transform;
            armPivotRot = transform.GetChild(0);
            armParent = armPivotRot.GetChild(0);
            light = transform.GetComponentInChildren<Light>();
            followLightHomePosition = 0.024f;
            followLightDeployedPosition = -0.08f;
            followLightMovementSpeed = .5f;
            UnDeploy();
        }

        private void Deploy()
        {
            Deployed = true;
        }
        private void UnDeploy()
        {
            light.enabled = false;
            Deployed = false;
        }
        public override void UpdateDerivedPart(ActionStep.Actions action, float delta)
        {
            base.UpdateDerivedPart(action, delta);
            if (parentCreature.GetCurrentAction() == ActionStep.Actions.None) return;
            World.Time_Of_Day timeOfDay = Area.GetTimeOfDay();
            bool deploy = false;
            bool undeploy = false;
            if (timeOfDay == World.Time_Of_Day.Midday || timeOfDay == World.Time_Of_Day.SunRise)
            {
                if (Deployed)
                {
                    undeploy = true;
                }
            }
            else
            {
                if (!Deployed)
                {
                    deploy = true;
                }
            }
            target = parentCreature.GetCurrentActionTargetDestination();
            if (target != Vector3.zero && action == ActionStep.Actions.MoveTo) // Valid destination
            {
                if (!Deployed)
                    deploy = true;
            }
            else
            {
                if (Deployed)
                    undeploy = true;
            }
            if (deploy && undeploy)
            {
                Debug.LogError("Both deploy and undeploy requested");
            }
            else if (deploy)
                Deploy();
            else if (undeploy)
                UnDeploy();

            Vector3 currentLocalPos = followLightY.transform.localPosition;
            if (Deployed)
            {
                // Not fully deployed yet //
                if (currentLocalPos.y > followLightDeployedPosition)
                {
                    currentLocalPos -= Vector3.up * followLightMovementSpeed * delta;
                    if (currentLocalPos.y <= followLightDeployedPosition)
                    {
                        currentLocalPos.y = followLightDeployedPosition;
                        light.enabled = true;
                    }
                    followLightY.localPosition = currentLocalPos;
                }
                // Fully deployed //
                else
                {
                    Quaternion currentRotation = followLightY.transform.rotation;
                    Vector3 currentGlobPos = followLightY.transform.position;
                    Vector3 neededDirection = (currentGlobPos-target).normalized;
                    Quaternion desiredRotation = Quaternion.LookRotation(neededDirection);
                    Quaternion difference = currentRotation * Quaternion.Inverse(desiredRotation);
                    Vector3 newRotation = new Vector3(0, difference.y, 0);
                    //Debug.LogWarning("Needed turn - " + newRotation);
                    followLightY.Rotate(newRotation*10);
                    light.transform.LookAt(target);
                }
            }
            else
            {
                if (currentLocalPos.y < followLightHomePosition)
                {
                    currentLocalPos += Vector3.up * followLightMovementSpeed*delta;
                    if (currentLocalPos.y > followLightHomePosition)
                        currentLocalPos.y = followLightHomePosition;
                    followLightY.transform.localPosition = currentLocalPos;
                }
            }
        }
    }
}