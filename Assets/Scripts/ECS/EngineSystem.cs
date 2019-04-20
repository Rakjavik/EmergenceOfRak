using Unity.Entities;
using rak.creatures;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace rak.ecs.ThingComponents
{
    public class EngineSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EngineJob job = new EngineJob
            {
                currentTime = Time.time,
                delta = Time.deltaTime
        };
            return job.Schedule(this, inputDeps);
        }

        //[BurstCompile]
        struct EngineJob : IJobForEach<Engine,CreatureAI,Agent,AgentVariables,EngineConstantForce,Target>
        {
            public float currentTime;
            public float delta;

            private void setState(MovementState requestedState, Direction direction, ref Engine engine, ref EngineConstantForce ecf)
            {
                MovementState currentState = MovementState.NONE;
                float currentForce;
                float maxForce;
                float minForce;
                if (direction == Direction.X)
                {
                    currentState = engine.CurrentStateX;
                    maxForce = engine.MaxForceX;
                    minForce = engine.MinForceX;
                    currentForce = ecf.CurrentForce.x;
                }
                else if (direction == Direction.Y)
                {
                    currentState = engine.CurrentStateY;
                    maxForce = engine.MaxForceY;
                    minForce = engine.MinForceY;
                    currentForce = ecf.CurrentForce.y;
                }
                else
                {
                    currentState = engine.CurrentStateZ;
                    maxForce = engine.MaxForceZ;
                    minForce = engine.MinForceZ;
                    currentForce = ecf.CurrentForce.z;
                }
                bool validState = false;
                MovementState[] availableStates = GetStatesCanSwithTo(currentState);
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
                    ecf.CurrentForce.x = currentForce;
                }
                else if (direction == Direction.Y)
                { 
                    engine.CurrentStateY = requestedState;
                    ecf.CurrentForce.y = currentForce;
                }
                else
                {
                    engine.CurrentStateZ = requestedState;
                    ecf.CurrentForce.z = currentForce;
                }
            }

            public void Execute(ref Engine engine, ref CreatureAI ai,ref Agent agent,
                ref AgentVariables av,ref EngineConstantForce ecf,ref Target target)
            {
                ActionStep.Actions currentAction = ai.CurrentAction;
                // VISIBLE TO CAMERA //
                if (av.Visible == 1)
                {
                    if (currentAction == ActionStep.Actions.MoveTo)
                    {
                        //Debug.LogWarning(currentTime - agent.YLastUpdated);
                        if (currentTime - agent.YLastUpdated > .5f)
                        {
                            agent.RequestRaycastUpdateDirectionDown = 1;
                            agent.YLastUpdated = currentTime;
                        }
                        if (currentTime - agent.ZLastUpdated > .5f)
                        {
                            agent.RequestRaycastUpdateDirectionForward = 1;
                            agent.ZLastUpdated = currentTime;
                        }
                        float velMag = av.RelativeVelocity.x + av.RelativeVelocity.y +
                            av.RelativeVelocity.z;
                        if (velMag > 10 && currentTime - agent.VelLastUpdated > .2f)
                        {
                            agent.RequestRayCastUpdateDirectionVel = 1;
                            agent.VelLastUpdated = currentTime;
                        }
                        bool objectBlockingForward = agent.DistanceFromFirstZHit < engine.objectBlockDistance;
                        MovementState stateToSetY = MovementState.IDLE;
                        // Moving down or close to ground, throttle up //

                        if (av.RelativeVelocity.y < -.5f || agent.DistanceFromGround < engine.sustainHeight)
                        {
                            stateToSetY = MovementState.FORWARD;
                        }

                        else if (agent.DistanceFromGround == Mathf.Infinity)
                        {

                        }
                        else if (av.RelativeVelocity.y > .5f)
                        {
                            stateToSetY = MovementState.IDLE;
                        }
                        else
                            stateToSetY = MovementState.IDLE;

                        if (engine.CurrentStateY != stateToSetY)
                            setState(stateToSetY, Direction.Y, ref engine, ref ecf);
                        // Don't move forward if we're blocked //
                        if (engine.CurrentStateZ != MovementState.FORWARD && !objectBlockingForward)
                            setState(MovementState.FORWARD, Direction.Z, ref engine, ref ecf);
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
                                    setState(MovementState.FORWARD, Direction.X, ref engine, ref ecf);
                                else
                                {
                                    setState(MovementState.REVERSE, Direction.X, ref engine, ref ecf);
                                }
                            }
                            if (engine.CurrentStateZ == MovementState.FORWARD)
                                setState(MovementState.IDLE, Direction.Z, ref engine, ref ecf);
                            else if (engine.CurrentStateZ == MovementState.IDLE &&
                                agent.DistanceFromFirstZHit < .5f)
                                setState(MovementState.REVERSE, Direction.Z, ref engine, ref ecf);
                            else if (engine.CurrentStateZ == MovementState.REVERSE &&
                                agent.DistanceFromFirstZHit >= .5f)
                                setState(MovementState.IDLE, Direction.Z, ref engine, ref ecf);
                        }
                        else
                        {
                            if (engine.CurrentStateX != MovementState.IDLE)
                                setState(MovementState.IDLE, Direction.X, ref engine, ref ecf);
                        }
                    }
                }
                // NOT VISIBLE TO CAMERA //
                else
                {
                    if(currentAction == ActionStep.Actions.MoveTo)
                    {
                        engine.NonPhysicsPositionUpdate = Vector3.MoveTowards(av.Position, target.targetPosition,
                            engine.VelWhenMovingWithoutPhysics * delta);
                        Debug.LogWarning(engine.NonPhysicsPositionUpdate);
                    }
                }
            }

            public MovementState[] GetStatesCanSwithTo(MovementState currentState)
            {
                MovementState[] possibleStates;
                // Destroyed can be switched to at any point //
                
                if (currentState == MovementState.FORWARD)
                {
                    possibleStates = new MovementState[4];
                    possibleStates[0] = MovementState.IDLE;
                    possibleStates[1] = MovementState.REVERSE;
                    possibleStates[2] = MovementState.POWER_DOWN;
                    possibleStates[3] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.IDLE)
                {
                    possibleStates = new MovementState[4];
                    possibleStates[0] = MovementState.FORWARD;
                    possibleStates[1] = MovementState.REVERSE;
                    possibleStates[2] = MovementState.POWER_DOWN;
                    possibleStates[3] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.REVERSE)
                {
                    possibleStates = new MovementState[4];
                    possibleStates[0] = MovementState.FORWARD;
                    possibleStates[1] = MovementState.IDLE;
                    possibleStates[2] = MovementState.POWER_DOWN;
                    possibleStates[3] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.POWER_DOWN)
                {

                    possibleStates = new MovementState[3];
                    possibleStates[0] = MovementState.UNINITIALIZED;
                    possibleStates[1] = MovementState.STARTING;
                    possibleStates[2] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.UNINITIALIZED)
                {
                    possibleStates = new MovementState[2];
                    possibleStates[0] = MovementState.STARTING;
                    possibleStates[1] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.STARTING)
                {
                    possibleStates = new MovementState[4];
                    possibleStates[0] = MovementState.FORWARD;
                    possibleStates[1] = MovementState.IDLE;
                    possibleStates[2] = MovementState.REVERSE;
                    possibleStates[3] = MovementState.DESTROYED;
                }
                else if (currentState == MovementState.NONE)
                {
                    possibleStates = new MovementState[2];
                    possibleStates[0] = MovementState.NONE;
                    possibleStates[1] = MovementState.DESTROYED;
                }
                else
                {
                    possibleStates = new MovementState[0];
                }
                return possibleStates;
            }
        }
    }
}
