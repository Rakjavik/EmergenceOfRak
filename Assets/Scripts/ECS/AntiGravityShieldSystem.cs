using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace rak.ecs.ThingComponents
{
    public class AntiGravityShieldSystem : JobComponentSystem
    {
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
            <AntiGravityShield,AgentVariables,Agent,Target,CreatureAI>
        {
            public float delta;

            public void Execute
                (ref AntiGravityShield shield, ref AgentVariables agentVar,ref Agent agent,ref Target target
                ,ref CreatureAI ai)
            {
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
                        }
                        // Angular Velocity //
                        else if (agentVar.GetAngularVelocityMag() > 2)
                        {
                            activate = true;
                        }
                        else
                        {
                            float velMag = agentVar.RelativeVelocity.x + agentVar.RelativeVelocity.y + agentVar.RelativeVelocity.z;
                            float beforeCollision = agent.DistanceFromVel / velMag;

                            // Check for imminent collision //
                            if (Mathf.Abs(beforeCollision) <= shield.BrakeIfCollidingIn &&
                                Mathf.Abs(beforeCollision) != Mathf.Infinity &&
                                beforeCollision > .001f &&
                                velMag > shield.VelocityMagNeededBeforeCollisionActivating)
                            {
                                activate = true;
                            }
                            // Check if going in wrong direction //
                            else if (velMag > shield.EngageIfWrongDirectionAndMovingFasterThan)
                            {
                                float3 turnNeeded = getAmountOfTurnNeeded(ref agentVar, ref target, 1);
                                if ((turnNeeded.x > 45 && turnNeeded.x < 315) ||
                                    (turnNeeded.z > 45 && turnNeeded.z < 315))
                                {
                                    activate = true;
                                }
                            }
                            if (shield.IgnoreStuckFor > 0)
                                shield.IgnoreStuckFor = shield.IgnoreStuckFor - delta;
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
                            float turnNeededMag = turnNeeded.x + turnNeeded.y + turnNeeded.z;
                            if (turnNeededMag < 1f || turnNeededMag > 359)
                            {
                                shield.Activated = 0;
                                shield.IgnoreStuckFor = .05f;
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
                    start = Quaternion.LookRotation(agentVar.Velocity);
                }
                float3 neededDirection = target.targetPosition - agentVar.Position;
                Quaternion desired = Quaternion.LookRotation(neededDirection);
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
