using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public struct EngineMovementVariable
    {
        public MovementState CurrentState { get; private set; }
        public float MaxForce { get; private set; }
        public float MinForce { get; private set; }
        public Direction FlightDirection { get; private set; }
        public float CurrentForce { get; private set; }

        private float startupRunningFor;
        
        private float startUpTimeInMin;

        public EngineMovementVariable(Direction flightDirection,Vector3 maxForce,float minimumForceToHover,
            float startUpTimeInMin)
        {
            this.FlightDirection = flightDirection;
            if (FlightDirection == Direction.Y)
                MaxForce = maxForce.y;
            else if (FlightDirection == Direction.Z)
                MaxForce = maxForce.z;
            else
                MaxForce = maxForce.x;
            // Amount of force needed to hold the objects weight //
            if (FlightDirection == Direction.Y)
                MinForce = minimumForceToHover;
            else
                MinForce = -MaxForce;
            this.startUpTimeInMin = startUpTimeInMin;
            CurrentForce = 0;
            CurrentState = MovementState.STARTING;
            startupRunningFor = .01f;
        }

        public void InitiateStartupSequence()
        {
            SetState(MovementState.STARTING);
            startupRunningFor += Time.deltaTime;
        }

        public void SetState(MovementState requestedState)
        {
            List<MovementState> availableStates = CreatureConstants.GetStatesCanSwithTo(CurrentState);
            if (!availableStates.Contains(requestedState))
            {
                Debug.LogError("Requesting change of state to invalid state request-current" +
                    requestedState + "-" + CurrentState);
                return;
            }
            if (requestedState == MovementState.FORWARD)
            {
                CurrentForce = MaxForce;
            }
            else if (requestedState == MovementState.IDLE)
            {
                if (MinForce > 0)
                {
                    CurrentForce = MinForce;
                }
                else
                {
                    CurrentForce = 0;
                }

            }
            else if (requestedState == MovementState.REVERSE)
            {
                CurrentForce = MinForce;
            }
            else if (requestedState == MovementState.UNINITIALIZED)
            {
                CurrentForce = 0;
            }
            else if (requestedState == MovementState.POWER_DOWN)
            {
                CurrentForce = 0;
            }
            CurrentState = requestedState;
        }

        public void Update(float delta)
        {
            if(startupRunningFor > 0)
            {
                startupRunningFor += delta;
                if(startupRunningFor > startUpTimeInMin)
                {
                    SetState(MovementState.IDLE);
                    startupRunningFor = 0;
                }
            }
        }
    }
}