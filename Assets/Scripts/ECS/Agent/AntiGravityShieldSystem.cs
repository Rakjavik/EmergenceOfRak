using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public struct AntiGravityShield : IComponentData
    {
        public byte Activated;
        public float IgnoreStuckFor; // Will ignore being stuck until back to 0
        public float BrakeIfCollidingIn;
        public float VelocityMagNeededBeforeCollisionActivating;
        public float EngageIfWrongDirectionAndMovingFasterThan;
    }

    public class AntiGravityShieldSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            Enabled = true;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            AntiGravityShieldJob job = new AntiGravityShieldJob
            {
                delta = Time.deltaTime
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }

        struct AntiGravityShieldJob : IJobForEach
            <AntiGravityShield,AgentVariables,Agent,Target,CreatureAI,Engine>
        {
            public float delta;

            public void Execute
                (ref AntiGravityShield shield, ref AgentVariables agentVar,ref Agent agent,ref Target target
                ,ref CreatureAI ai, ref Engine engine)
            {
                if (agentVar.Visible == 0)
                {
                    shield.Activated = 0;
                    return;
                }
                ActionStep.Actions currentAction = ai.CurrentAction;
                if (currentAction == ActionStep.Actions.MoveTo)
                {
                    // Currently deactivated //
                    if (shield.Activated == 0)
                    {
                        bool activate = false;
                        // STUCK //
                        if (agent.GetDistanceMoved() <= .5f && shield.IgnoreStuckFor <= 0)
                        {
                            activate = true;
                            shield.IgnoreStuckFor = .2f;
                            //Debug.LogWarning("Activate Stuck");
                        }
                        // Angular Velocity //
                        else if (agentVar.GetAngularVelocityMag() > 2)
                        {
                            activate = true;
                            //Debug.LogWarning("Activate AngularVel - " + agentVar.AngularVelocity.ToString());
                        }
                        else
                        {
                            float velMag = Mathf.Abs(agentVar.RelativeVelocity.x + agentVar.RelativeVelocity.y + agentVar.RelativeVelocity.z);
                            float beforeCollision = agent.DistanceFromVel / velMag;
                            
                            // Check for imminent collision //
                            if (Mathf.Abs(beforeCollision) <= shield.BrakeIfCollidingIn &&
                                Mathf.Abs(beforeCollision) != Mathf.Infinity &&
                                beforeCollision > .001f &&
                                velMag > shield.VelocityMagNeededBeforeCollisionActivating)
                            {
                                //Debug.LogWarning("Activate collision");
                                activate = true;
                            }
                            // Check if going in wrong direction //
                            else if (velMag > shield.EngageIfWrongDirectionAndMovingFasterThan)
                            {
                                float3 turnNeeded = getAmountOfTurnNeeded(ref agentVar, ref target, 1);
                                if ((turnNeeded.x > 2f && turnNeeded.x < 358) && engine.AvoidingObstacles == 0)
                                {
                                    activate = true;
                                    //Debug.LogWarning("Activate Wrong Direction - " + turnNeeded);
                                }
                            }
                            if (shield.IgnoreStuckFor > 0)
                            {
                                shield.IgnoreStuckFor = shield.IgnoreStuckFor - delta;
                            }
                        }
                        if (activate)
                        {
                            shield.Activated = 1;
                        }
                    }
                    // Currently Active //
                    else
                    {
                        if(ai.CurrentAction == ActionStep.Actions.MoveTo)
                        {
                            // Make sure we are pointing in the right direction before deactivating //
                            float3 turnNeeded = getAmountOfTurnNeeded(ref agentVar, ref target,0);
                            float turnNeededMag = Mathf.Abs(turnNeeded.x + turnNeeded.y + turnNeeded.z);
                            if (turnNeededMag < 1f || turnNeededMag > 359 || engine.AvoidingObstacles == 1)
                            {
                                shield.Activated = 0;
                                shield.IgnoreStuckFor = .5f;
                                //Debug.Log("Deactivate shield");
                            }
                        }
                    }
                }
                else
                {
                    if (shield.Activated == 0)
                    {
                        bool activate = false;
                        ActionStep.Actions[] actionsThatActivate = getActionsToStayActivatedDuring();
                        for(int count = 0; count < actionsThatActivate.Length; count++)
                        {
                            if(currentAction == actionsThatActivate[count])
                            {
                                activate = true;
                                break;
                            }
                        }
                        if (activate)
                            shield.Activated = 1;
                    }
                }
            }
            private float3 getAmountOfTurnNeeded(ref AgentVariables agentVar,ref Target target,byte velocity)
            {
                Quaternion start;
                // Start rotation does not use velocity //
                if (velocity == 0) {
                    start = new Quaternion(agentVar.Rotation.x, agentVar.Rotation.y,
                          agentVar.Rotation.z, agentVar.Rotation.w);
                }
                // Use velocity for start rotation //
                else
                {
                    float3 normalized = Vector3.Normalize(agentVar.Velocity);
                    start = Quaternion.LookRotation(normalized, Vector3.up);
                }
                float3 neededDirection = Vector3.Normalize(target.targetPosition - agentVar.Position);
                if (neededDirection.Equals(float3.zero))
                    return float3.zero;
                Quaternion desired = Quaternion.LookRotation(neededDirection,Vector3.up);
                Quaternion difference = Quaternion.Inverse(desired) * start;
                return difference.eulerAngles;
            }
            private ActionStep.Actions[] getActionsToStayActivatedDuring()
            {
                return new ActionStep.Actions[]
                {
                    ActionStep.Actions.Add,ActionStep.Actions.Locate,ActionStep.Actions.None,
                    ActionStep.Actions.Wait
                };
            }
        }
    }
}
