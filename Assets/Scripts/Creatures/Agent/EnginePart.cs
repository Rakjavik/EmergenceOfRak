﻿using rak.world;
using UnityEngine;

namespace rak.creatures
{

    public class EnginePart : Part
    {
        private bool _landing = false; // Whether the landing process has been started
        
        // Constant Force component control for X Y and Z Axis //
        private EngineMovementVariables[] engineMovementVariables;

        public EnginePart(CreaturePart creaturePart,Transform transform, CreatureLocomotionType partMovementType,
            float updateEvery) 
            : base(creaturePart,transform,updateEvery)
        {

        }

        public void InitializeMovementPart()
        {
            CreatureLocomotionType locomotionType = attachedAgent.locomotionType;
            attachedBody = PartTransform.GetComponentInParent<Rigidbody>();
            if (locomotionType == CreatureLocomotionType.StandardForwardBack || 
                locomotionType == CreatureLocomotionType.Flight)
            {
                ConstantForce cf = attachedBody.gameObject.GetComponent<ConstantForce>();
                if (cf == null)
                    cf = attachedBody.gameObject.AddComponent<ConstantForce>();
                cf.relativeForce = Vector3.zero;
                engineMovementVariables = new EngineMovementVariables[3];
                if (locomotionType == CreatureLocomotionType.Flight)
                {
                    engineMovementVariables[(int)Direction.Y] = new EngineMovementVariables(
                       attachedAgent, Direction.Y, miscVariables);
                    engineMovementVariables[(int)Direction.Z] = new EngineMovementVariables(
                        attachedAgent, Direction.Z, miscVariables);
                    engineMovementVariables[(int)Direction.X] = new EngineMovementVariables(
                        attachedAgent, Direction.X, miscVariables);
                    engineMovementVariables[0].Initialize(attachedAgent, parentCreature, attachedBody);
                    engineMovementVariables[1].Initialize(attachedAgent, parentCreature, attachedBody);
                    engineMovementVariables[2].Initialize(attachedAgent, parentCreature, attachedBody);
                    Enabled = true;
                }
                else if (locomotionType == CreatureLocomotionType.StandardForwardBack)
                {
                    engineMovementVariables[(int)Direction.Y] = new EngineMovementVariables(
                       attachedAgent, Direction.Y, miscVariables);
                    engineMovementVariables[(int)Direction.Z] = new EngineMovementVariables(
                        attachedAgent, Direction.Z, miscVariables);
                    engineMovementVariables[(int)Direction.X] = new EngineMovementVariables(
                        attachedAgent, Direction.X, miscVariables);
                }
            }
            else
            {
                engineMovementVariables = new EngineMovementVariables[0];
            }
        }
        public override void Disable()
        {
            Debug.LogWarning("Disable call from engine class");
            if (Enabled)
            {
                for (int count = 0; count < engineMovementVariables.Length; count++)
                {
                    engineMovementVariables[count].SetState(MovementState.DESTROYED);
                }
                _landing = false;
                Enabled = false;
            }
        }
        public override void Enable()
        {
            Debug.LogWarning("Enable call from engine class");
            if (Enabled) Debug.LogError("Call to enable part when already enabled");
            InitiateEngineStartupSequences();
            Enabled = true;
        }
        private void InitiateEngineStartupSequences()
        {
            foreach (EngineMovementVariables movement in engineMovementVariables)
            {
                movement.InitiateStartupSequence();
            }
        }

        private void orbit(EngineMovementVariables engine, Vector3 relativeVelocity)
        {
            if(relativeVelocity.magnitude > 5)
            {
                Vector3 brakeAmount = relativeVelocity;
                brakeAmount.y = 0;
                attachedAgent.ApplyBrake(relativeVelocity*20,true);
            }
            float orbitZDistance = 5;
            if (engine.flightDirection == Direction.Z || engine.flightDirection == Direction.Y)
            {
                
                // Too far away to orbit //
                float directionalDistance;
                if(engine.flightDirection == Direction.Z)
                    directionalDistance = Mathf.Abs(attachedAgent.GetDistanceZFromDestination());
                else
                    directionalDistance = Mathf.Abs(attachedAgent.GetDistanceYFromDestination());
                // If we're getting a negative distance, don't do anything //
                if (directionalDistance < 0)
                    return;

                if (directionalDistance > orbitZDistance + 1)
                {
                    // Moving backward slowly //
                    if (Mathf.Abs(relativeVelocity.z) < 1)
                    {
                        if(engine.CurrentState != MovementState.IDLE)
                            engine.SetState(MovementState.IDLE);
                    }
                    // Moving backward fast //
                    else
                    {
                        if (engine.CurrentState != MovementState.FORWARD)
                            engine.SetState(MovementState.FORWARD);
                    }
                }
                // Too close to target //
                else if (directionalDistance < orbitZDistance-1)
                {
                    // Moving forward slowly //
                    if(Mathf.Abs(relativeVelocity.z) < 1)
                    {
                        if (engine.CurrentState != MovementState.IDLE)
                            engine.SetState(MovementState.IDLE);
                    }
                    // Moving forward fast //
                    else
                    {
                        if (engine.CurrentState != MovementState.REVERSE)
                            engine.SetState(MovementState.REVERSE);
                    }
                }
            }
            else if (engine.flightDirection == Direction.X && Mathf.Abs(relativeVelocity.x) < 3)
            {
                if (!attachedAgent.IsCollidingWithSomethingOnAxis(Direction.X))
                {
                    if (engine.CurrentState != MovementState.FORWARD)
                        engine.SetState(MovementState.FORWARD);
                }
            }
            else if (engine.flightDirection == Direction.X && Mathf.Abs(relativeVelocity.x)>= 3)
            {
                if (engine.CurrentState != MovementState.IDLE)
                    engine.SetState(MovementState.IDLE);
            }
        }
        private void maintainPosition(EngineMovementVariables engine, Vector3 currentRelativeVel)
        {
            // If we're already killing power, return //
            if (engine.CurrentState == MovementState.POWER_DOWN) return;
            MovementState requestedState = MovementState.NONE;
            float maintainBelowVelocity =
                miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Maintain_Below_Velocity];
            if (engine.flightDirection == Direction.Z)
            {
                if (currentRelativeVel.z > maintainBelowVelocity)
                {
                    requestedState = MovementState.REVERSE;
                }
                else if (currentRelativeVel.z < -maintainBelowVelocity)
                {

                    requestedState = MovementState.FORWARD;
                }
                else
                {
                    if (engine.CurrentState != MovementState.IDLE)
                    {
                        requestedState = MovementState.IDLE;
                    }
                }
                if (engine.CurrentState != requestedState && requestedState != MovementState.NONE)
                {
                    engine.SetState(requestedState);
                }
            }
            else if (engine.flightDirection == Direction.X)
            {
                if (currentRelativeVel.x > maintainBelowVelocity)
                {
                    requestedState = MovementState.REVERSE;
                }
                else if (currentRelativeVel.x < -maintainBelowVelocity)
                {
                    requestedState = MovementState.FORWARD;
                }
                else
                {
                    if (engine.CurrentState != MovementState.IDLE)
                    {
                        requestedState = MovementState.IDLE;
                    }
                }
                if (engine.CurrentState != requestedState && requestedState != MovementState.NONE)
                {
                    engine.SetState(requestedState);
                }
            }
            else // DIRECTION Y
            {
                // If we're landing, shut down the engine when we're moving slow enough //
                if (attachedAgent.creature.GetCurrentAction() == ActionStep.Actions.Land)
                {
                    if (currentRelativeVel.magnitude < 1 && engine.CurrentState != MovementState.POWER_DOWN)
                    {
                        engine.SetState(MovementState.POWER_DOWN);
                    }
                }
            }
            if (engine.CurrentState == MovementState.POWER_DOWN)
            {
                float relativeDirection;
                if (engine.flightDirection == Direction.X)
                    relativeDirection = currentRelativeVel.x;
                else if (engine.flightDirection == Direction.Y)
                    relativeDirection = currentRelativeVel.y;
                else
                    relativeDirection = currentRelativeVel.z;
                // Powering down, but moving up //
                float atMostVel = miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Y_Engine_PowerDown_Until_Vel_At_Most];
                if (relativeDirection > atMostVel && engine.flightDirection == Direction.Y)
                {
                    float amount = (attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.DOWN) * Time.deltaTime) /
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Y_Engine_PowerDown_Reduction_Factor];
                    engine.SubtractForceForLanding(amount, engine.flightDirection);
                }
                else if (engine.flightDirection == Direction.X || engine.flightDirection == Direction.Z)
                {
                    float amount = Time.deltaTime;
                    engine.SubtractForceForLanding(amount, engine.flightDirection);
                }
            }
        }
        public void PrepareForLanding()
        {
            for (int count = 0; count < engineMovementVariables.Length; count++)
            {
                engineMovementVariables[count].SetState(MovementState.POWER_DOWN);
            }
            _landing = true;
        }
        public bool IsLanding()
        {
            return _landing;
        }

        // FLIGHT //
        private void ProcessFlight()
        {
            Vector3 relativeVel = attachedBody.transform.InverseTransformDirection(attachedBody.velocity);
            bool goingForward = relativeVel.z > 0.01f;
            float distanceToInteract = parentCreature.getCreatureStats().getDistanceFromTargetBeforeConsideredReached();
            bool needDirectionForward = Mathf.Abs(attachedAgent.GetDistanceZFromDestination()) > distanceToInteract /
                miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Z_Engine_Dist_To_Act_FWD_Factor];
            bool needDirectionUp = attachedAgent.GetDistanceYFromDestination() < distanceToInteract *
                miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Y_Engine_Dist_To_Act_UP_Mult];
            bool goingUp = (relativeVel.y > .01f);
            attachedAgent.SetBrakeRequestToZero();

            // Loop through axis //
            for (int count = 0; count < 3; count++)
            {
                EngineMovementVariables engine = engineMovementVariables[count];
                engine.Update();
                if (engineMovementVariables[count].CurrentState == MovementState.STARTING)
                    continue;

                bool goingPlusDirection;
                bool needToGoPlusDirection;
                if (engine.flightDirection == Direction.Y)
                {
                    goingPlusDirection = goingUp;
                    needToGoPlusDirection = needDirectionUp;
                }
                else if (engine.flightDirection == Direction.Z)
                {
                    goingPlusDirection = goingForward;
                    needToGoPlusDirection = needDirectionForward;
                }
                else // X AXIS
                {
                    float kickInIfMoreThan =
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_X_Engine_Kick_In_When_Faster_Than];
                    // Speed on X axis //
                    if (relativeVel.x > kickInIfMoreThan)
                    {
                        needToGoPlusDirection = false;
                        goingPlusDirection = true;
                    }
                    else if (relativeVel.x < -kickInIfMoreThan)
                    {
                        needToGoPlusDirection = true;
                        goingPlusDirection = false;
                    }
                    // Not much movement on X axis //
                    else
                    {
                        // Dummy values since these will be skipped //
                        needToGoPlusDirection = false;
                        goingPlusDirection = false;
                    }
                }

                // Make throttle adjustments //
                MovementState desiredState = MovementState.NONE;
                if (!goingPlusDirection && needToGoPlusDirection && !engine._engineLocked)
                {
                    desiredState = MovementState.FORWARD;
                }
                else if (goingPlusDirection && !needToGoPlusDirection && !engine._engineLocked)
                {
                    if (engine.flightDirection != Direction.X)
                    {
                        desiredState = MovementState.IDLE;
                    }
                    else
                    {
                        desiredState = MovementState.REVERSE;
                    }
                }
                else if (!goingPlusDirection && !needToGoPlusDirection && !engine._engineLocked)
                {
                    if (engine.flightDirection == Direction.Y)
                    {
                        if (attachedAgent.IsCollidingWithSomethingOnAxis(Direction.Y))
                        {
                            Debug.LogWarning("Hitting ground, throttle to Y");
                            desiredState = MovementState.FORWARD;
                        }
                        else
                            desiredState = MovementState.IDLE;
                    }
                    else
                        desiredState = MovementState.IDLE;
                }
                else
                {
                    float directionalSpeed;
                    float cruisingSpeed;
                    float timeToCollision = -1;

                    // CALCULATE NEARBY COLLISIONS //
                    if (!attachedAgent.ignoreIncomingCollisions)
                    {
                        timeToCollision = attachedAgent.TimeToCollisionAtCurrentVel;
                        // Imminenet Collision //
                        float collisionProblem =
                            miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Reverse_Engine_If_Colliding_In];
                        if (goingPlusDirection && needToGoPlusDirection)
                        {
                            // Z Engine, go into reverse if we're going to collide //
                            if (timeToCollision < collisionProblem &&
                            engine.flightDirection == Direction.Z)
                            {
                                if (engine.CurrentState != MovementState.REVERSE)
                                {
                                    engine.SetState(MovementState.REVERSE);
                                }
                            }
                        }
                    
                        // X Engine, move to the side to avoid collision in front //
                        if (engine.flightDirection == Direction.X)
                        {
                            if ((timeToCollision < collisionProblem) ||
                                (attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.FORWARD) 
                                < 1))
                            {
                                engineMovementVariables[(int)Direction.X]._engineLocked = true;
                                float distanceLeft = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.LEFT);
                                float distanceRight = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.RIGHT);
                                if (distanceLeft > distanceRight)
                                {
                                    if (engine.CurrentState != MovementState.REVERSE)
                                    {
                                        //Debug.LogWarning("Shifting X engine to reverse");
                                        engine.SetState(MovementState.REVERSE);
                                    }
                                }
                                else
                                {
                                    if (engine.CurrentState != MovementState.FORWARD)
                                    {
                                        //Debug.LogWarning("Shifting X engine to forward");
                                        engine.SetState(MovementState.FORWARD);
                                    }
                                }
                            }
                            else
                            {
                                engineMovementVariables[(int)Direction.X]._engineLocked = false;
                            }
                        }
                    }
                }
                if (engine.CurrentState != desiredState && desiredState != MovementState.NONE)
                {
                    engine.SetState(desiredState);
                }
            }
            // If close to target apply brakes //
            if (attachedAgent.GetDistanceFromDestination() < distanceToInteract *
                    attachedAgent.slowDownModifier && !attachedAgent.GetRigidBody().isKinematic)
            {
                if (relativeVel.magnitude >
                    miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Max_Vel_Mag_Before_Brake])
                {
                    Vector3 amount = Vector3.one;
                    attachedAgent.ApplyBrake(amount *
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Angular_Velocity_Brake_When_Over], true);
                    attachedAgent.ApplyBrake(amount *
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Angular_Velocity_Brake_When_Over], false);
                    //Debug.LogWarning("Applying brake, close to target - " + amount);
                }
            }
            float magnitude = attachedBody.velocity.magnitude;
            if (magnitude > attachedAgent.maxVelocityMagnitude)
            {
                attachedAgent.ApplyBrake(attachedBody.velocity*
                    miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Max_Vel_Magnitude_Brake_Multiplier],false);
            }
            // END FLIGHT PROCESSING //
        }
        private void Flight(ActionStep.Actions currentCreatureAction)
        {
            if(currentCreatureAction == ActionStep.Actions.MoveTo)
            {
                EngineMovementVariables engineZ = engineMovementVariables[(int)Direction.Z];
                EngineMovementVariables engineY = engineMovementVariables[(int)Direction.Y];
                EngineMovementVariables engineX = engineMovementVariables[(int)Direction.X];
                Vector3 relativeVel = attachedBody.transform.InverseTransformDirection(attachedBody.velocity);
                float distFromGround = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.DOWN);
                float distanceFromFirstZHit = attachedAgent.GetDistanceBeforeCollision(
                    CreatureUtilities.RayCastDirection.FORWARD);
                bool objectBlockingForward = distanceFromFirstZHit < 3;
                MovementState stateToSetY = MovementState.IDLE;
                // Moving down or close to ground, throttle up //
                if (relativeVel.y < -.5f || distFromGround < attachedAgent.GetSustainHeight())
                {
                    stateToSetY = MovementState.FORWARD;
                }
                else if (distFromGround == Mathf.Infinity)
                {
                    if (attachedBody.position.y < Area.MinimumHeight)
                        stateToSetY = MovementState.FORWARD;
                    else if (attachedBody.position.y > Area.MaximumHeight)
                        stateToSetY = MovementState.IDLE;
                }
                // Moving up, throttle idle
                else if (relativeVel.y > .5f)
                {
                    stateToSetY = MovementState.IDLE;
                }
                else
                {
                    stateToSetY = MovementState.IDLE;
                }
                if (engineY.CurrentState != stateToSetY)
                    engineY.SetState(stateToSetY);
                // Don't move forward if we're blocked //
                if (engineZ.CurrentState != MovementState.FORWARD && !objectBlockingForward)
                    engineZ.SetState(MovementState.FORWARD);
                if (objectBlockingForward)
                {
                    float distanceRight = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.RIGHT);
                    float distanceLeft = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.LEFT);
                    if (engineX.CurrentState == MovementState.IDLE)
                    {
                        bool goRight = distanceLeft < distanceRight;
                        if (goRight)
                            engineX.SetState(MovementState.FORWARD);
                        else
                        {
                            engineX.SetState(MovementState.REVERSE);
                        }
                    }
                    if (engineZ.CurrentState == MovementState.FORWARD)
                        engineZ.SetState(MovementState.IDLE);
                    else if (engineZ.CurrentState == MovementState.IDLE &&
                        distanceFromFirstZHit < .5f)
                        engineZ.SetState(MovementState.REVERSE);
                    else if (engineZ.CurrentState == MovementState.REVERSE &&
                        distanceFromFirstZHit >= .5f)
                        engineZ.SetState(MovementState.IDLE);
                }
                else
                {
                    if (engineX.CurrentState != MovementState.IDLE)
                        engineX.SetState(MovementState.IDLE);
                }

            }
        }
        // GROUND //
        private void ProcessStandardForwardBack()
        {

        }
        public void SetFlightEnginesToIdle(bool force)
        {
            foreach (EngineMovementVariables movement in engineMovementVariables)
            {
                if (movement.CurrentState != MovementState.IDLE || force)
                    movement.SetState(MovementState.IDLE);
                else
                {
                    Debug.LogError("Invalid state change, set all engines to idle, already idle - " + movement.flightDirection);
                    return;
                }
            }
        }

        public void OnCollisionEnter(Collision collision) { }

        public void OnCollisionExit(Collision collision) { }

        public override void UpdateDerivedPart(ActionStep.Actions action)
        {
            if (attachedBody.isKinematic) return;
            if(attachedAgent.locomotionType == CreatureLocomotionType.Flight)
            {
                Flight(action);
            }
            else if (attachedAgent.locomotionType == CreatureLocomotionType.StandardForwardBack)
            {
                ProcessStandardForwardBack();
            }
        }
        
        private void DebugFlyer(bool goingUp, bool rightDirection, float timeTillReachTarget, float distanceFromGround,
            float currentYForce)
        {
            string debug = "Going {directionRight} direction - {direction} YForce-{yForce}\n" +
                    "time to reach - {timeTillReach} distance from ground {distanceFromGround}";
            // Going Up //
            if (goingUp)
            {
                debug = debug.Replace("{direction}", "Up");
            }
            // Going Down //
            else
            {
                debug = debug.Replace("{direction}", "Down");
            }
            if (rightDirection)
            {
                debug = debug.Replace("{directionRight}", "Correct");
            }
            else
            {
                debug = debug.Replace("{directionRight}", "Wrong");
            }
            debug = debug.Replace("{timeTillReach}", timeTillReachTarget.ToString());
            debug = debug.Replace("{distanceFromGround}", distanceFromGround.ToString());
            debug = debug.Replace("{yForce}", currentYForce.ToString());
            Debug.LogWarning(debug);
        }
        
    }
}