using Unity.Entities;
using rak.creatures;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Burst;

namespace rak.ecs.ThingComponents
{
    public class EngineSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineJob job = new EngineJob
            {
                currentTime = Time.time
            };
            return job.Schedule(this, inputDeps);
        }

        //[BurstCompile]
        struct EngineJob : IJobForEach<Engine,CreatureAI,Agent,AgentVariables>
        {
            public float currentTime;

            private void setState(MovementState requestedState, Direction direction, ref Engine engine)
            {
                MovementState currentState;
                float currentForce;
                float maxForce;
                float minForce;
                if (direction == Direction.X)
                {
                    currentState = engine.CurrentStateX;
                    maxForce = engine.MaxForceX;
                    minForce = engine.MinForceX;
                    currentForce = engine.CurrentForceX;
                }
                else if (direction == Direction.Y)
                {
                    currentState = engine.CurrentStateY;
                    maxForce = engine.MaxForceY;
                    minForce = engine.MinForceY;
                    currentForce = engine.CurrentForceY;
                }
                else
                {
                    currentState = engine.CurrentStateZ;
                    maxForce = engine.MaxForceZ;
                    minForce = engine.MinForceZ;
                    currentForce = engine.CurrentForceZ;
                }

                MovementState[] availableStates = CreatureConstants.GetStatesCanSwithTo(currentState).ToArray();
                bool validState = false;
                for (int count = 0; count < availableStates.Length; count++)
                {
                    if (availableStates[count] == requestedState)
                        validState = true;
                }
                if (!validState)
                {
                    Debug.LogError("Requesting change of state to invalid state request-current" +
                        requestedState + "-" + currentState);
                    return;
                }
                if (requestedState == MovementState.FORWARD)
                {
                    currentForce = maxForce;
                }
                else if (requestedState == MovementState.IDLE)
                {
                    if (minForce > 0)
                    {
                        currentForce = minForce;
                    }
                    else
                    {
                        currentForce = 0;
                    }

                }
                else if (requestedState == MovementState.REVERSE)
                {
                    currentForce = minForce;
                }
                else if (requestedState == MovementState.UNINITIALIZED)
                {
                    currentForce = 0;
                }
                else if (requestedState == MovementState.POWER_DOWN)
                {
                    currentForce = 0;
                }

                if (direction == Direction.X)
                {
                    engine.CurrentStateX = requestedState;
                    engine.CurrentForceX = currentForce;
                }
                else if (direction == Direction.Y)
                { 
                    engine.CurrentStateY = requestedState;
                    engine.CurrentForceY = currentForce;
                }
                else
                {
                    engine.CurrentStateZ = requestedState;
                    engine.CurrentForceZ = currentForce;
                }
            }

            public void Execute(ref Engine engine, ref CreatureAI creatureAI,ref Agent agent,ref AgentVariables agentVariables)
            {
                ActionStep.Actions currentAction = creatureAI.CurrentAction;
                if (currentAction == ActionStep.Actions.MoveTo)
                {
                    Debug.LogWarning(currentTime - agent.YLastUpdated);
                    if(currentTime-agent.YLastUpdated > .5f)
                    {
                        agent.RequestRaycastUpdateDirectionDown = 1;
                        agent.YLastUpdated = currentTime;
                    }
                    if(currentTime-agent.ZLastUpdated > .5f)
                    {
                        agent.RequestRaycastUpdateDirectionForward = 1;
                        agent.ZLastUpdated = currentTime;
                    }
                    bool objectBlockingForward = agent.DistanceFromFirstZHit < engine.objectBlockDistance;
                    MovementState stateToSetY = MovementState.IDLE;
                    // Moving down or close to ground, throttle up //
                    
                    if (agentVariables.RelativeVelocity.y < -.5f || agent.DistanceFromGround < engine.sustainHeight)
                    {
                        stateToSetY = MovementState.FORWARD;
                    }

                    else if (agent.DistanceFromGround == Mathf.Infinity)
                    {

                    }
                    else if (agentVariables.RelativeVelocity.y > .5f)
                    {
                        stateToSetY = MovementState.IDLE;
                    }
                    else
                        stateToSetY = MovementState.IDLE;

                    if (engine.CurrentStateY != stateToSetY)
                        setState(stateToSetY, Direction.Y, ref engine);
                    // Don't move forward if we're blocked //
                    if (engine.CurrentStateZ != MovementState.FORWARD && !objectBlockingForward)
                        setState(MovementState.FORWARD, Direction.Z, ref engine);
                    if (objectBlockingForward)
                    {
                        agent.RequestRaycastUpdateDirectionLeft = 1;
                        agent.RequestRaycastUpdateDirectionRight = 1;
                        agent.RequestRaycastUpdateDirectionForward = 1;
                        float distanceRight = agent.DistanceFromRight;
                        float distanceLeft = agent.DistanceFromRight;
                        if (engine.CurrentStateX == MovementState.IDLE)
                        {
                            bool goRight = distanceLeft < distanceRight;
                            if (goRight)
                                setState(MovementState.FORWARD, Direction.X, ref engine);
                            else
                            {
                                setState(MovementState.REVERSE, Direction.X, ref engine);
                            }
                        }
                        if (engine.CurrentStateZ == MovementState.FORWARD)
                            setState(MovementState.IDLE, Direction.Z, ref engine);
                        else if (engine.CurrentStateZ == MovementState.IDLE &&
                            agent.DistanceFromFirstZHit < .5f)
                            setState(MovementState.REVERSE, Direction.Z, ref engine);
                        else if (engine.CurrentStateZ == MovementState.REVERSE &&
                            agent.DistanceFromFirstZHit >= .5f)
                            setState(MovementState.IDLE, Direction.Z, ref engine);
                    }
                    else
                    {
                        if (engine.CurrentStateX != MovementState.IDLE)
                            setState(MovementState.IDLE, Direction.X, ref engine);
                    }
                }
            }
        }
    }
}
