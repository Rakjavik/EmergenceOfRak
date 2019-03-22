using rak.world;
using System.Collections;
using UnityEngine;

namespace rak.creatures
{

    public class EnginePart : Part
    {
        // Constant Force component control for X Y and Z Axis //
        private ConstantForce cf;
        private EngineMovementVariable[] engineMovementVariables;
        private float baseUpdateEvery;

        public EnginePart(CreaturePart creaturePart,Transform transform, CreatureLocomotionType partMovementType,
            float updateEvery) 
            : base(creaturePart,transform,updateEvery)
        {
            baseUpdateEvery = updateEvery;
            cf = transform.GetComponentInParent<ConstantForce>();
        }

        public void InitializeMovementPart()
        {
            CreatureLocomotionType locomotionType = attachedAgent.locomotionType;
            attachedBody = PartTransform.GetComponentInParent<Rigidbody>();
            if (locomotionType == CreatureLocomotionType.StandardForwardBack || 
                locomotionType == CreatureLocomotionType.Flight)
            {
                cf = attachedAgent.GetConstantForceComponent();
                if (cf == null)
                    cf = attachedBody.gameObject.AddComponent<ConstantForce>();
                cf.relativeForce = Vector3.zero;
                engineMovementVariables = new EngineMovementVariable[3];
                if (locomotionType == CreatureLocomotionType.Flight)
                {
                    engineMovementVariables[(int)Direction.Y] = new
                        EngineMovementVariable(Direction.Y, attachedAgent.maxForce, attachedAgent.minimumForceToHover,
                        miscVariables[MiscVariables.AgentMiscVariables.MoveVar_Start_Up_Time_In_Minutes]);
                    engineMovementVariables[(int)Direction.X] = new
                        EngineMovementVariable(Direction.X, attachedAgent.maxForce, attachedAgent.minimumForceToHover,
                        miscVariables[MiscVariables.AgentMiscVariables.MoveVar_Start_Up_Time_In_Minutes]);
                    engineMovementVariables[(int)Direction.Z] = new
                        EngineMovementVariable(Direction.Z, attachedAgent.maxForce, attachedAgent.minimumForceToHover,
                        miscVariables[MiscVariables.AgentMiscVariables.MoveVar_Start_Up_Time_In_Minutes]);
                    /*
                     * engineMovementVariables[(int)Direction.Y] = new EngineMovementVariables(
                       attachedAgent, Direction.Y, miscVariables);
                    engineMovementVariables[(int)Direction.Z] = new EngineMovementVariables(
                        attachedAgent, Direction.Z, miscVariables);
                    engineMovementVariables[(int)Direction.X] = new EngineMovementVariables(
                        attachedAgent, Direction.X, miscVariables);
                    engineMovementVariables[0].Initialize(attachedAgent, parentCreature, attachedBody);
                    engineMovementVariables[1].Initialize(attachedAgent, parentCreature, attachedBody);
                    engineMovementVariables[2].Initialize(attachedAgent, parentCreature, attachedBody);
                    */
                    Enabled = true;
                }
                // NOT IMPLEMENTED YET //
                else if (locomotionType == CreatureLocomotionType.StandardForwardBack)
                {
                    
                }
            }
            else
            {
                engineMovementVariables = new EngineMovementVariable[0];
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
            foreach (EngineMovementVariable movement in engineMovementVariables)
            {
                movement.InitiateStartupSequence();
            }
        }

        IEnumerator Flight(ActionStep.Actions currentCreatureAction)
        {
            if (currentCreatureAction == ActionStep.Actions.MoveTo)
            {
                Vector3 relativeVel = attachedBody.transform.InverseTransformDirection(attachedBody.velocity);
                float distFromGround = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.DOWN);
                yield return null;
                float distanceFromFirstZHit = attachedAgent.GetDistanceBeforeCollision(
                    CreatureUtilities.RayCastDirection.FORWARD);
                yield return null;
                float objectBlockDistance = miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Halt_Forward_Movement_If_Object_Is_Distance];
                bool objectBlockingForward = distanceFromFirstZHit < objectBlockDistance;
                //Debug.LogWarning("Distance from first z- " + distanceFromFirstZHit);
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
                if (engineMovementVariables[(int)Direction.Y].CurrentState != stateToSetY)
                    engineMovementVariables[(int)Direction.Y].SetState(stateToSetY);
                // Don't move forward if we're blocked //
                if (engineMovementVariables[(int)Direction.Z].CurrentState != MovementState.FORWARD && !objectBlockingForward)
                    engineMovementVariables[(int)Direction.Z].SetState(MovementState.FORWARD);
                if (objectBlockingForward)
                {
                    float distanceRight = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.RIGHT);
                    float distanceLeft = attachedAgent.GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection.LEFT);
                    if (engineMovementVariables[(int)Direction.X].CurrentState == MovementState.IDLE)
                    {
                        bool goRight = distanceLeft < distanceRight;
                        if (goRight)
                            engineMovementVariables[(int)Direction.X].SetState(MovementState.FORWARD);
                        else
                        {
                            engineMovementVariables[(int)Direction.X].SetState(MovementState.REVERSE);
                        }
                    }
                    if (engineMovementVariables[(int)Direction.Z].CurrentState == MovementState.FORWARD)
                        engineMovementVariables[(int)Direction.Z].SetState(MovementState.IDLE);
                    else if (engineMovementVariables[(int)Direction.Z].CurrentState == MovementState.IDLE &&
                        distanceFromFirstZHit < .5f)
                        engineMovementVariables[(int)Direction.Z].SetState(MovementState.REVERSE);
                    else if (engineMovementVariables[(int)Direction.Z].CurrentState == MovementState.REVERSE &&
                        distanceFromFirstZHit >= .5f)
                        engineMovementVariables[(int)Direction.Z].SetState(MovementState.IDLE);
                }
                else
                {
                    if (engineMovementVariables[(int)Direction.X].CurrentState != MovementState.IDLE)
                        engineMovementVariables[(int)Direction.X].SetState(MovementState.IDLE);
                }
            }
            engineMovementVariables[(int)Direction.X].Update(Time.deltaTime);
            engineMovementVariables[(int)Direction.Y].Update(Time.deltaTime);
            engineMovementVariables[(int)Direction.Z].Update(Time.deltaTime);
            //Vector3 currentForce = cf.relativeForce;
            float x = engineMovementVariables[(int)Direction.X].CurrentForce;
            float y = engineMovementVariables[(int)Direction.Y].CurrentForce;
            float z = engineMovementVariables[(int)Direction.Z].CurrentForce;
            Vector3 newForce = new Vector3(x, y, z);
            cf.relativeForce = newForce;
        }
        // GROUND //
        private void ProcessStandardForwardBack()
        {

        }
        public void SetFlightEnginesToIdle(bool force)
        {
            foreach (EngineMovementVariable movement in engineMovementVariables)
            {
                if (movement.CurrentState != MovementState.IDLE || force)
                    movement.SetState(MovementState.IDLE);
                else
                {
                    Debug.LogError("Invalid state change, set all engines to idle, already idle - " + movement.FlightDirection);
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

                attachedAgent.creature.StartCoroutine(Flight(action));
                UpdateEvery = baseUpdateEvery + (25 - attachedBody.velocity.magnitude) * .01f;
                //Debug.Log(UpdateEvery);
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