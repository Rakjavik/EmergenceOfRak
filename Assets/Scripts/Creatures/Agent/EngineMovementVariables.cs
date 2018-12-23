using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public class EngineMovementVariables : PartEngineVariables
    {
        public MovementState CurrentState { get; set; }
        public float MaxForce { get; set; }
        public float MinForce { get; set; }
        public Direction flightDirection { get; private set; }

        private ConstantForce cf;
        private float startupRunningFor;
        private Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables;

        public EngineMovementVariables(CreatureAgent agent,
            Direction flightDirection, Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables) :
            base(Vector3.zero, PartMovesWith.NA)
        {
            this.miscVariables = miscVariables;
            this.flightDirection = flightDirection;
        }

        public void Initialize(CreatureAgent attachedAgent, Creature creature,
            Rigidbody attachedBody)
        {
            currentConstraints = attachedBody.constraints;
            CurrentState = MovementState.UNINITIALIZED;
            if (flightDirection == Direction.Y)
                MaxForce = attachedAgent.maxForce.y;
            else if (flightDirection == Direction.Z)
                MaxForce = attachedAgent.maxForce.z;
            else if (flightDirection == Direction.X)
                MaxForce = attachedAgent.maxForce.x;
            // Amount of force needed to hold the objects weight //
            if (flightDirection == Direction.Y)
                MinForce = attachedAgent.minimumForceToHover;
            else
                MinForce = -MaxForce;
            cf = creature.GetComponent<ConstantForce>();
            startupRunningFor = 0;
            InitiateStartupSequence();
        }
        public void SetState(MovementState requestedState)
        {
            //Debug.LogWarning("Change of state to " + requestedState + "-" + flightDirection);
            List<MovementState> availableStates = CreatureConstants.GetStatesCanSwithTo(CurrentState);
            if (!availableStates.Contains(requestedState))
            {
                Debug.LogError("Requesting change of state to invalid state request-current" +
                    requestedState + "-" + CurrentState);
                return;
            }
            Vector3 currentForce = cf.relativeForce;
            if (requestedState == MovementState.FORWARD)
            {
                if (flightDirection == Direction.X)
                    currentForce.x = MaxForce;
                else if (flightDirection == Direction.Y)
                {
                    currentForce.y = MaxForce;
                }
                else
                    currentForce.z = MaxForce;
            }
            else if (requestedState == MovementState.IDLE)
            {
                if (MinForce > 0)
                {
                    if (flightDirection == Direction.X)
                    {
                        currentForce.x = MinForce;
                    }
                    else if (flightDirection == Direction.Y)
                    {
                        currentForce.y = MinForce;
                    }
                    else if (flightDirection == Direction.Z)
                    {
                        currentForce.z = MinForce;
                    }
                }
                else
                {
                    if (flightDirection == Direction.X)
                        currentForce.x = 0;
                    else if (flightDirection == Direction.Y)
                        currentForce.y = 0;
                    else if (flightDirection == Direction.Z)
                        currentForce.z = 0;
                }

            }
            else if (requestedState == MovementState.REVERSE)
            {
                if (flightDirection == Direction.X)
                    currentForce.x = MinForce;
                else if (flightDirection == Direction.Y)
                    currentForce.y = MinForce;
                else if (flightDirection == Direction.Z)
                    currentForce.z = MinForce;
            }
            else if (requestedState == MovementState.UNINITIALIZED)
            {
                if (flightDirection == Direction.X)
                    currentForce.x = 0;
                else if (flightDirection == Direction.Y)
                    currentForce.y = 0;
                else if (flightDirection == Direction.Z)
                    currentForce.z = 0;
            }
            else if (requestedState == MovementState.POWER_DOWN)
            {
                if (flightDirection == Direction.X)
                    currentForce.x = 0;
                else if (flightDirection == Direction.Y)
                    currentForce.y = MinForce;
                else if (flightDirection == Direction.Z)
                    currentForce.z = 0;
            }
            cf.relativeForce = currentForce;
            CurrentState = requestedState;
        }
        public void SubtractForceForLanding(float amount, Direction axis)
        {
            if (amount < 0) return;
            Vector3 currentForce = cf.relativeForce;
            float currentDirectionalForce;
            if (axis == Direction.X) currentDirectionalForce = currentForce.x;
            else if (axis == Direction.Y) currentDirectionalForce = currentForce.y;
            else currentDirectionalForce = currentForce.z;
            currentDirectionalForce -= amount;
            if (amount < 0) amount = 0;
            if (axis == Direction.X) currentForce.x = currentDirectionalForce;
            else if (axis == Direction.Y) currentForce.y = currentDirectionalForce;
            else currentForce.z = currentDirectionalForce;
            cf.relativeForce = currentForce;
            if (currentDirectionalForce <= 0)
            {
                SetState(MovementState.UNINITIALIZED);
            }

        }
        public void Update()
        {
            if (startupRunningFor > 0)
            {
                startupRunningFor += Time.deltaTime;
                if (startupRunningFor >
                    miscVariables[MiscVariables.AgentMiscVariables.MoveVar_Start_Up_Time_In_Minutes])
                {
                    SetState(MovementState.IDLE);
                    startupRunningFor = 0;
                }
            }

        }
        public void InitiateStartupSequence()
        {
            SetState(MovementState.STARTING);
            startupRunningFor += Time.deltaTime;
        }
    }
}