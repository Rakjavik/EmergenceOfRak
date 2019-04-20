﻿using rak.ecs.ThingComponents;
using rak.world;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures
{
    public class CreatureAgent
    {
        public static bool DEBUG = World.ISDEBUGSCENE;
        private bool initialized = false;
        

        // Movement destination //
        public float velocityWhenMovingWithoutPhysics { get; private set; }
        public float TimeToCollisionAtCurrentVel { get; private set; }
        private float _timeUpdatedCollisionAtVel = 0;
        private float[] distanceToCollision = new float[5];
        private float[] _timeUpdatedDistanceToCollision = new float[5];
        public Vector3 Destination;
        // Not active skips update method //
        public bool Active { get; private set; }
        // Size of the boxcast when looking for explore targets //
        public float ExploreRadiusModifier { get; private set; }
        // Creature object being controlled //
        public Creature creature { get; private set; }
        // Amount of brake requested to any BrakeParts //
        public float3 CurrentBrakeAmountRequest { get; private set; }
        // Maximium total velocity before brakes kick in //
        public float maxVelocityMagnitude { get; private set; }
        // Maximum force that can be applied to the ConstantForce componenet //
        public float3 maxForce = float3.zero;
        // Maximum Angular Velocity //
        public float maxAngularVel { get; private set; }
        // Force required to keep creature floating //
        public float minimumForceToHover { get; private set; }
        // Multiplier for how close creature has to be to target before engaging brakes //
        public int slowDownModifier { get; private set; }
        // Speed modifier for Turn method //
        public float turnSpeed { get; private set; }
        // Rigid bodies currently in contact with creature //
        public List<Transform> touchingBodies { get; private set; }
        // Throttle will back off when this is reached //
        public float3 CruisingSpeed { get; private set; }
        public bool ignoreIncomingCollisions { get; private set; }
        // Type of turning this creature uses //
        public CreatureTurnType creatureTurnType { get; private set; }
        public CreatureLocomotionType locomotionType { get; private set; }
        public CreatureGrabType grabType { get; private set; }

        private Rigidbody rigidbody;
        // All body parts in agent //
        private Part[] allParts;
        private float lastUpdate = 0;
        // Height creature tries to keep from ground if flying //
        private float sustainHeight = 3;
        // If a flyer, whether he is landing or not //
        private bool landing = false;
        // Creatures position last Update //
        private Vector3 positionLastUpdate { get; set; }
        // For tracking distance movement each update //
        public float DistanceMovedLastUpdate { get; private set; }

        private Transform _transform;
        // TODO OPTIMIZE //
        private Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables;
        

        #region GETTERS/SETTERS
        public CreatureLocomotionType GetMoveType()
        {
            return locomotionType;
        }
        public float GetObjectBlockDistance()
        {
            return miscVariables[MiscVariables.AgentMiscVariables.Part_Flight_Halt_Forward_Movement_If_Object_Is_Distance];
        }
        public Thing GetCurrentActionTarget()
        {
            return creature.GetCurrentActionTarget();
        }
        public float3 GetCurrentActionDestination()
        {
            return creature.GetCurrentActionTargetDestination();
        }
        public void SetCruisingSpeed(float3 cruisingSpeed)
        {
            this.CruisingSpeed = cruisingSpeed;
        }
        public ActionStep.Actions GetCurrentCreatureAction()
        {
            return creature.GetCurrentAction();
        }
        public bool IsLanding()
        {
            return landing;
        }
        public void SetTurnSpeed(float turnSpeed)
        {
            this.turnSpeed = turnSpeed;
        }
        public void SetSlowDownModifier(int slowDownModifier)
        {
            this.slowDownModifier = slowDownModifier;
        }
        public void SetMinimumForceToHover(float minimumForceToHover)
        {
            this.minimumForceToHover = minimumForceToHover;
        }
        public void SetMaxVelocityMagnitude(float maxVelocityMagnitude)
        {
            this.maxVelocityMagnitude = maxVelocityMagnitude;
        }
        public void SetExploreRadiusModifier(float exploreRadiusModifier)
        {
            this.ExploreRadiusModifier = exploreRadiusModifier;
        }
        public void SetGrabType(CreatureGrabType grabType)
        {
            this.grabType = grabType;
        }
        public float GetSustainHeight() { return sustainHeight; }
        public Rigidbody GetRigidBody() { return rigidbody; }
        public void setParts(List<Part> allParts)
        {
            this.allParts = allParts.ToArray();
        }
        public void SetCreatureTurnType(CreatureTurnType creatureTurnType)
        {
            if (creatureTurnType == this.creatureTurnType) Debug.LogWarning("Turn type already set");
            this.creatureTurnType = creatureTurnType;
        }
        public void SetSustainHeight(float sustainHeight)
        {
            this.sustainHeight = sustainHeight;
        }
        public ConstantForce GetConstantForceComponent() { return rigidbody.GetComponent<ConstantForce>(); }
        public void SetBrakeRequestToZero()
        {
            CurrentBrakeAmountRequest = float3.zero;
        }
        public void SetMaxAngularVel(float maxAngularVel)
        {
            this.maxAngularVel = maxAngularVel;
        }
        public void SetDestination(Vector3 destination)
        {
            this.Destination = destination;
        }
        
        public void SetIgnoreCollisions(bool ignore)
        {
            ignoreIncomingCollisions = ignore;
        }
        public int IsKinematic()
        {
            if (rigidbody.isKinematic)
                return 1;
            return 0;
        }
        public Part[] GetAllParts()
        {
            return allParts;
        }
        #endregion

        #region MISC METHODS
        public void DeactivateAllParts()
        {
            foreach(Part part in allParts)
            {
                part.Disable();
            }
        }
        public void Land()
        {
            for (int count = 0; count < allParts.Length; count++)
            {
                if (allParts[count] is EnginePart)
                {
                    EnginePart currentPart = (EnginePart)allParts[count];
                    // TODO Landing
                }
            }
            landing = true;
        }
        public void EnableAgent()
        {
            if (Active)
                Debug.LogError("Call to enable agent when already active");
            foreach(Part part in allParts)
            {
                part.Enable();
            }
            rigidbody.constraints = RigidbodyConstraints.None;
            ignoreIncomingCollisions = false;
            Active = true;
        }
        public void DisableAgent()
        {
            if (Active)
            {
                foreach(Part part in allParts)
                {
                    part.Disable();
                }
                rigidbody.constraints = RigidbodyConstraints.FreezePosition;
                landing = false;
                Active = false;
            }
            else
                Debug.LogWarning("Call to deactivate Creature agent when already disabled");
        }
        public void Sleep()
        {
            creature.ChangeState(Creature.CREATURE_STATE.SLEEP);
        }
        public void ApplyBrake(float3 percentToBrake,bool angular)
        {
            CurrentBrakeAmountRequest = percentToBrake;
        }
        #endregion MISC METHODS

        #region CALCULATION METHODS
        public bool IsCollidingWithSomethingOnAxis(Direction axis)
        {
            bool xCollision = false;
            bool yCollision = false;
            bool zCollision = false;
            foreach(Transform body in touchingBodies)
            {
                Vector3 delta = creature.transform.position - body.position;
                if (Mathf.Abs(delta.x) < .5f)
                    xCollision = true;
                if (Mathf.Abs(delta.y) < .5f)
                    yCollision = true;
                if (Mathf.Abs(delta.z) < .5f)
                    zCollision = true;
            }
            if (axis == Direction.X)
                return xCollision;
            else if (axis == Direction.Y)
                return yCollision;
            else
                return zCollision;
        }
        public float GetDistanceFromDestination()
        {
            float3 destinationNoY = new float3(Destination.x, 0, Destination.z);
            float3 positionNoY = new float3(creature.transform.position.x, 0, creature.transform.position.z);
            return Vector3.Distance(positionNoY, destinationNoY);
        }

        private void _RaycastTrajectory()
        {
            int trajIndex = (int)CreatureUtilities.RayCastDirection.VELOCITY;
            Vector3 relativeVel =
                rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
            RaycastHit hit;
            float rayLength = 20;//miscVariables[MiscVariables.AgentMiscVariables.Agent_Detect_Collision_Vel_Distance];
            if (Physics.Raycast(_transform.position, rigidbody.velocity, out hit, rayLength))
            {
                if (DEBUG)
                    Debug.DrawLine(_transform.position, hit.point, Color.black, .5f);
                distanceToCollision[trajIndex] = Vector3.Distance(_transform.position, hit.point);
                TimeToCollisionAtCurrentVel = distanceToCollision[(int)CreatureUtilities.RayCastDirection.VELOCITY]
                    / relativeVel.magnitude;
                _timeUpdatedCollisionAtVel = Time.time;
                _timeUpdatedDistanceToCollision[trajIndex] = Time.time;
            }
        }
        private void _RaycastDirection(CreatureUtilities.RayCastDirection direction)
        {
            Vector3 vectorDirection;
            if (direction == CreatureUtilities.RayCastDirection.FORWARD)
                vectorDirection = _transform.forward;
            else if (direction == CreatureUtilities.RayCastDirection.LEFT)
                vectorDirection = -_transform.right;
            else if (direction == CreatureUtilities.RayCastDirection.RIGHT)
                vectorDirection = _transform.right;
            else if (direction == CreatureUtilities.RayCastDirection.DOWN)
                vectorDirection = Vector3.down;
            else
            {
                Debug.LogError("Raycast direction for velocity in wrong method");
                return;
            }
            RaycastHit hit;
            float rayLength = miscVariables[MiscVariables.AgentMiscVariables.Agent_Detect_Collision_Vel_Distance];
            if (Physics.Raycast(_transform.position, vectorDirection, out hit, rayLength))
            {
                if (DEBUG)
                {
                    Color color;
                    if (direction == CreatureUtilities.RayCastDirection.LEFT)
                        color = Color.white;
                    else if (direction == CreatureUtilities.RayCastDirection.RIGHT)
                        color = Color.red;
                    else
                        color = Color.yellow;
                    //Debug.DrawLine(_transform.position, hit.point, color, .5f);
                }
                distanceToCollision[(int)direction] = Vector3.Distance(_transform.position, hit.point);
                _timeUpdatedDistanceToCollision[(int)direction] = Time.time;
            }

        }
        public float GetDistanceBeforeCollision(CreatureUtilities.RayCastDirection direction)
        {
            if(Time.time - _timeUpdatedDistanceToCollision[(int)direction] > .2f)
            {
                _RaycastDirection(direction);
                //Debug.LogWarning("Refreshing direction ray - " + direction);
            }
            return distanceToCollision[(int)direction];
        }
        public float GetTimeBeforeCollision()
        {
            if(Time.time -_timeUpdatedCollisionAtVel > .2f)
            {
                _RaycastTrajectory();
                //Debug.LogWarning("Before collision - " + TimeToCollisionAtCurrentVel);
            }
            return TimeToCollisionAtCurrentVel;
        }
        #endregion CALCULATION METHODS

        // CONSTRUCTOR //
        public CreatureAgent(Creature creature)
        {
            this.creature = creature;
            positionLastUpdate = Vector3.zero;
            DistanceMovedLastUpdate = 0;
            touchingBodies = new List<Transform>();
            miscVariables = MiscVariables.GetAgentMiscVariables(creature);
            Active = true;
        }

        public void Initialize(BASE_SPECIES baseSpecies)
        {
            rigidbody = creature.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = creature.GetComponentInParent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = creature.GetComponentInChildren<Rigidbody>();
                    if (rigidbody == null)
                        Debug.LogError("Can't find rigid body on " + creature.thingName);
                }
            }
            CreatureConstants.CreatureAgentInitialize(baseSpecies, this);
            rigidbody.constraints = RigidbodyConstraints.None;
            _transform = rigidbody.transform;
            if (baseSpecies == BASE_SPECIES.Gnat)
                locomotionType = CreatureLocomotionType.Flight;
            else if (baseSpecies == BASE_SPECIES.Gagk)
                locomotionType = CreatureLocomotionType.StandardForwardBack;
            ignoreIncomingCollisions = false;
            velocityWhenMovingWithoutPhysics = 20;
            initialized = true;
        }

        #region MONO METHODS
        // Called from Creature Object //
        public void OnCollisionEnter(Collision collision)
        {
            if (!touchingBodies.Contains(collision.gameObject.transform))
            {
                touchingBodies.Add(collision.gameObject.transform);
            }
        }
        // Called from Creature Object //
        public void OnCollisionExit(Collision collision)
        {
            if (touchingBodies.Contains(collision.gameObject.transform))
            {
                touchingBodies.Remove(collision.gameObject.transform);
                //Debug.LogWarning("Body not touching anymore - " + collision.gameObject.name);
            }
        }
        public void CollisionRemoveIfPresent(Transform collision)
        {
            if(touchingBodies.Contains(collision))
                touchingBodies.Remove(collision);
        }
        public bool HasCollision(Transform collision)
        {
            return touchingBodies.Contains(collision);
        }
        // Called from Creature Object //
        public void Update(float delta, bool visible)
        {
            if (!Active) return;
            lastUpdate += delta;
            CreatureAI ai = new CreatureAI
            {
                CurrentAction = creature.GetCurrentAction()
            };
            Unity.Entities.World.Active.EntityManager.SetComponentData(creature.goEntity.Entity, ai);
            // If In View update everything normally //
            if (visible)
            {
                // Part updates //
                foreach (Part part in allParts)
                {
                    part.Update(delta);
                }
                Engine engineData = creature.goEntity.EntityManager.GetComponentData<Engine>(creature.goEntity.Entity);
                AgentVariables agentVariables = creature.goEntity.EntityManager.
                    GetComponentData<AgentVariables>(creature.goEntity.Entity);
                float3 relativeVel = creature.transform.InverseTransformDirection(rigidbody.velocity);
                Quaternion rotation = creature.transform.rotation;
                float4 currentRot = new float4(rotation.x, rotation.y, rotation.z, rotation.w);
                AgentVariables agentData = new AgentVariables
                {
                    RelativeVelocity = relativeVel,
                    Position = creature.transform.position,
                    Velocity = rigidbody.velocity,
                    Rotation = currentRot,
                    AngularVelocity = rigidbody.angularVelocity
                };
                Unity.Entities.World.Active.EntityManager.SetComponentData
                    (creature.goEntity.Entity, agentData);
            }
            // If not in view, manually move without physics //
            else
            {
                ActionStep.Actions currentAction = creature.GetCurrentAction();
                if (currentAction == ActionStep.Actions.MoveTo)
                {
                    creature.transform.position = Vector3.MoveTowards(creature.transform.position, Destination, 
                        velocityWhenMovingWithoutPhysics*delta);
                    GridSector currentSector = creature.currentSector;

                    if (!currentSector.IsEmpty())
                    {
                        float terrainY = creature.currentSector.GetTerrainHeightFromGlobalPos(creature.transform.position);
                        creature.transform.position = new Vector3(creature.transform.position.x, terrainY+sustainHeight, creature
                            .transform.position.z);
                    }
                    // Update these parts when not visible //
                    for (int count = 0; count < allParts.Length; count++)
                    {
                        if (allParts[count] is AnimationPart &&
                            ((AnimationPart)allParts[count]).PartType == CreaturePart.SHIELD)
                            allParts[count].Update(delta);
                        else if (allParts[count] is AntiGravityShieldPart)
                            allParts[count].Update(delta);
                        else if (allParts[count] is TractorBeamPart)
                            allParts[count].Update(delta);
                        else if (allParts[count] is LightArmPart)
                            allParts[count].Update(delta);
                    }
                }
                else if (currentAction == ActionStep.Actions.Add)
                {
                    for(int count = 0; count < allParts.Length; count++)
                    {
                        if (allParts[count] is TractorBeamPart)
                            allParts[count].Update(delta);
                    }
                }
            }
        }
        #endregion MONO METHODS
    }
    public enum CreaturePart { LEG , FOOT, BODY, ENGINE_Z, ENGINE_Y, ENGINE_X, BRAKE, SHIELD,
        TRACTORBEAM, LIGHT_ARM, NONE
    }
    public enum CreatureGrabType { TractorBeam }
    public enum CreatureLocomotionType { StandardForwardBack, Flight, NONE }
    public enum CreatureAnimationMovementType { Rotation, Inch, NONE }
    public enum CreatureTurnType { Rotate , Shorten, Inch }
}