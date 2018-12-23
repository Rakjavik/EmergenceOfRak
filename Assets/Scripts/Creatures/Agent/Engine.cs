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
            //base.Disable();
            Debug.LogWarning("Disable call from engine class");
            if (Enabled)
            {
                for (int count = 0; count < engineMovementVariables.Length; count++)
                {
                    if (engineMovementVariables[count].CurrentState != MovementState.UNINITIALIZED)
                    {
                        engineMovementVariables[count].SetState(MovementState.UNINITIALIZED);
                    }
                }
                _landing = false;
                Enabled = false;
            }
        }
        public override void Enable()
        {
            //base.Enable();
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
                    float amount = (attachedAgent.GetDistanceFromGround() * Time.deltaTime) /
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
            attachedAgent.SetCurrentlyBrakingToFalse();

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

                // Special Operations //
                // MAINTAIN POSITION //
                if (attachedAgent.MaintainPosition)
                {
                    maintainPosition(engine, relativeVel);
                    continue;
                }
                // ORBITING //
                else if (attachedAgent.IsOrbitingTarget())
                {
                    orbit(engine, relativeVel);
                    continue;
                }
                // Make throttle adjustments //
                MovementState desiredState = MovementState.NONE;
                if (attachedAgent.MaintainPosition || attachedAgent.IsOrbitingTarget())
                {
                    Debug.LogError("At regular flight procedure when supposed to maintain position");
                    return;
                }
                if (!goingPlusDirection && needToGoPlusDirection)
                {
                    desiredState = MovementState.FORWARD;
                }
                else if (goingPlusDirection && !needToGoPlusDirection)
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
                else if (!goingPlusDirection && !needToGoPlusDirection)
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
                else if (goingPlusDirection && needToGoPlusDirection)
                {
                    float directionalSpeed;
                    float cruisingSpeed;
                    float timeToCollision = -1;

                    // CALCULATE NEARBY COLLISIONS //
                    if (!attachedAgent.ignoreIncomingCollisions)
                    {
                        Vector3 timeBeforeCollision = attachedAgent.GetTimeBeforeCollision();
                        if (engine.flightDirection == Direction.X)
                        {
                            timeToCollision = timeBeforeCollision.x;
                        }
                        else if (engine.flightDirection == Direction.Z)
                        {
                            timeToCollision = timeBeforeCollision.z;
                        }
                        else
                        {
                            timeToCollision = timeBeforeCollision.y;
                        }

                        if (engine.flightDirection == Direction.X)
                        {
                            directionalSpeed = relativeVel.x;
                            cruisingSpeed = attachedAgent.CruisingSpeed.x;
                        }
                        else if (engine.flightDirection == Direction.Y)
                        {
                            directionalSpeed = relativeVel.y;
                            cruisingSpeed = attachedAgent.CruisingSpeed.y;
                        }
                        else // Z AXIS
                        {
                            directionalSpeed = relativeVel.z;
                            cruisingSpeed = attachedAgent.CruisingSpeed.z;
                        }
                        // Imminenet Collision //
                        float collisionProblem =
                            miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Reverse_Engine_If_Colliding_In];
                        if (timeToCollision < collisionProblem && engine.flightDirection != Direction.Y)
                        {
                            if (engine.CurrentState != MovementState.REVERSE)
                            {
                                engine.SetState(MovementState.REVERSE);
                            }
                        }
                        else if (engine.CurrentState == MovementState.REVERSE && timeToCollision >= collisionProblem
                            && engine.flightDirection != Direction.Y)
                        {
                            engine.SetState(MovementState.IDLE);
                        }
                        else if (directionalSpeed >= cruisingSpeed)
                        {
                            if (engine.CurrentState != MovementState.IDLE)
                            {
                                engine.SetState(MovementState.IDLE);
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
                    attachedAgent.slowDownModifier)
            {
                if (relativeVel.magnitude >
                    miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Close_To_Target_Max_Vel_Mag_Before_Brake])
                {
                    Vector3 amount = Vector3.one;
                    attachedAgent.ApplyBrake(amount *
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Close_To_Target_Brake_Percent], true);
                    attachedAgent.ApplyBrake(amount *
                        miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Close_To_Target_Brake_Percent], false);
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

        public void UpdateMovePart()
        {
            if(attachedAgent.locomotionType == CreatureLocomotionType.Flight)
            {
                ProcessFlight();
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