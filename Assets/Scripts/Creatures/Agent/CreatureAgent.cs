using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public class CreatureAgent
    {
        public static bool DEBUG = false;
        private bool initialized = false;
        public void Initialize(BASE_SPECIES baseSpecies)
        {
            rigidbody = creature.GetComponent<Rigidbody>();
            CreatureConstants.CreatureAgentInitialize(baseSpecies, this);
            if (rigidbody == null)
            {
                rigidbody = creature.GetComponentInParent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = creature.GetComponentInChildren<Rigidbody>();
                    if (rigidbody == null)
                        Debug.LogError("Can't find rigid body on " + creature.name);
                }
            }
            rigidbody.constraints = RigidbodyConstraints.None;
            if (baseSpecies == BASE_SPECIES.Gnat)
                locomotionType = CreatureLocomotionType.Flight;
            else if (baseSpecies == BASE_SPECIES.Gagk)
                locomotionType = CreatureLocomotionType.StandardForwardBack;
            foreach (Part part in allParts)
            {
                if(part is EnginePart)
                {
                    EnginePart movePart = (EnginePart)part;
                    movePart.InitializeMovementPart();
                }
            }
            ignoreIncomingCollisions = false;
            initialized = true;
        }

        // Movement destination //
        public Vector3 Destination { get; private set; }
        // Not active skips update method //
        public bool Active { get; private set; }
        // Size of the boxcast when looking for explore targets //
        public float ExploreRadiusModifier { get; private set; }
        // Creature object being controlled //
        public Creature creature { get; private set; }
        // If creature is close to target on X and Z axis //
        public bool OverTarget { get; private set; }
        // Amount of brake applied this update //
        public int currentBrakeAmount { get; private set; }
        // Maximium total velocity before brakes kick in //
        public float maxVelocityMagnitude { get; private set; }
        // Maximum force that can be applied to the ConstantForce componenet //
        public Vector3 maxForce = Vector3.zero;
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
        public Vector3 CruisingSpeed { get; private set; }
        public bool MaintainPosition { get; private set; }
        public bool ignoreIncomingCollisions { get; private set; }
        // Type of turning this creature uses //
        public CreatureTurnType creatureTurnType { get; private set; }
        public CreatureLocomotionType locomotionType { get; private set; }

        private Rigidbody rigidbody;
        // All body parts in agent //
        private List<Part> allParts;
        // How often to run the Turn update //
        private float updateMainAgentEvery = .1f;
        private float sinceLastTurnUpdate = 0;
        // Height creature tries to keep from ground if flying //
        private float sustainHeight = 3;
        // If a flyer, whether he is landing or not //
        private bool landing = false;
        // If enabled the flyer will orbit around the target destination //
        private bool orbitTarget = false;
        // Whether the brake has been activate this update //
        private bool braking = false;
        private float distanceMovedInLastFiveSeconds { get; set; }
        private float distanceMovedLastUpdate { get; set; }
        // Creatures position last Update //
        private Vector3 positionLastUpdate { get; set; }
        // For tracking distance movement each update //
        private float lastTimeDistanceChecked { get; set; }
        private Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables;
        

        #region GETTERS/SETTERS
        public void SetCruisingSpeed(Vector3 cruisingSpeed)
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
        public float GetSustainHeight() { return sustainHeight; }
        public Rigidbody GetRigidBody() { return rigidbody; }
        public void setParts(List<Part> allParts)
        {
            this.allParts = allParts;
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
        public void SetUpdateAgentEvery(float updateTurnEvery)
        {
            this.updateMainAgentEvery = updateTurnEvery;
        }
        public ConstantForce GetConstantForceComponent() { return rigidbody.GetComponent<ConstantForce>(); }
        public void SetCurrentlyBrakingToFalse()
        {
            braking = false;

        }
        public bool IsBraking()
        {
            return braking;
        }
        public void SetMaxAngularVel(float maxAngularVel)
        {
            this.maxAngularVel = maxAngularVel;
        }
        public void SetDestination(Vector3 destination)
        {
            this.Destination = destination;
        }
        public void SetOrbitTarget(bool orbit)
        {
            orbitTarget = orbit;
        }
        public bool IsOrbitingTarget()
        {
            return orbitTarget;
        }
        public void SetIgnoreCollisions(bool ignore)
        {
            ignoreIncomingCollisions = ignore;
        }
        #endregion

        #region MISC METHODS
        private void destroyAll(GameObject gameObject)
        {
            for(int count = 0; count < gameObject.transform.childCount; count++)
            {
                if (gameObject.transform.GetChild(count).childCount > 0)
                {
                    destroyAll(gameObject.transform.GetChild(count).gameObject);
                }
                GameObject.Destroy(gameObject.transform.GetChild(count).gameObject);
            }
        }
        public void DestroyAllParts()
        {
            foreach(Part part in allParts)
            {
                destroyAll(part.PartTransform.gameObject);
            }
            
        }
        public void Land()
        {
            for (int count = 0; count < allParts.Count; count++)
            {
                if (allParts[count] is EnginePart)
                {
                    EnginePart currentPart = (EnginePart)allParts[count];
                    currentPart.PrepareForLanding();
                }
            }
            landing = true;
            OverTarget = false;
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
                MaintainPosition = false;
                OverTarget = false;
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
        
        public void ApplyBrake(Vector3 percentToBrake,bool angular)
        {
            //Debug.LogWarning("Applying brake - " + percentToBrake);
            if (percentToBrake.x < 0) percentToBrake.x = 0;
            else if (percentToBrake.x > 100) percentToBrake.x = 100;
            if (percentToBrake.y < 0) percentToBrake.y = 0;
            else if (percentToBrake.y > 100) percentToBrake.y = 100;
            if (percentToBrake.z < 0) percentToBrake.z = 0;
            else if (percentToBrake.z > 100) percentToBrake.z = 100;

            float x = percentToBrake.x * .01f;
            float y = percentToBrake.y * .01f;
            float z = percentToBrake.z * .01f;

            Vector3 currentVelocity;
            if (!angular)
                currentVelocity = rigidbody.velocity;
            else
                currentVelocity = rigidbody.angularVelocity;
            currentVelocity.x = currentVelocity.x * (1f - x);
            currentVelocity.y = currentVelocity.y * (1f - y);
            currentVelocity.z = currentVelocity.z * (1f - z);
            if (!angular)
                rigidbody.velocity = currentVelocity;
            else
                rigidbody.angularVelocity = currentVelocity;
            braking = true;
            currentBrakeAmount = (int)percentToBrake.magnitude;
        }
        private Quaternion CorrectRotation()
        {
            float minimumDiffFromCorrectRotation =
                miscVariables[MiscVariables.AgentMiscVariables.Agent_Correct_Rotation_If_Diff_Less_Than];
            float currentZRotation = creature.transform.rotation.z;
            float currentXRotation = creature.transform.rotation.x;
            if (Mathf.Abs(currentXRotation) > minimumDiffFromCorrectRotation ||
               Mathf.Abs(currentZRotation) > minimumDiffFromCorrectRotation)
            {
                Quaternion currentRot = creature.transform.rotation;
                currentRot.y = 0;
                return Quaternion.Slerp(
                    currentRot, Quaternion.identity, turnSpeed);
            }
            return Quaternion.identity;
        }
        #endregion MISC METHODS

        #region CALCULATION METHODS
        private bool isStuck()
        {
            if (landing) return false;
            return distanceMovedInLastFiveSeconds < 
                miscVariables[MiscVariables.AgentMiscVariables.Agent_Is_Stuck_If_Moved_Less_Than_In_Five_Secs];
        }
        public bool HasCompletedLanding()
        {
            if (touchingBodies.Count > 0)
            {
                return true;
            }
            else if (rigidbody.velocity.y < 
                miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_Complete_When_Y_Vel_Lower_Than]
                && GetDistanceFromGround() < 
                miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_Complete_When_Distance_From_Ground_Less_Than])
            {
                return true;
            }
            return false;
        }
        public float GetTimeTillReachGroundYAtCurrentVelocity()
        {
            float returnTime = GetDistanceFromGround() / Mathf.Abs(rigidbody.velocity.y);
            if (returnTime < .01) returnTime = 0;
            return returnTime;
        }
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
        public bool IsOnSolidGround()
        {
            if (GetDistanceFromGround() < 
                    miscVariables[MiscVariables.AgentMiscVariables.Agent_OnSolidGround_If_Dist_From_Ground_Less_Than]
                && distanceMovedLastUpdate < 
                    miscVariables[MiscVariables.AgentMiscVariables.Agent_OnSolidGround_If_Dist_Moved_Last_Update_Less_Than])
                return true;
            else
                return false;
        }
        public float GetDistanceYFromDestination()
        {
            return (creature.transform.position - Destination).y;
        }
        public float GetDistanceZFromDestination()
        {
            //Debug.LogWarning("Distance from z - " + (creature.transform.position - Destination).z);
            return (creature.transform.position - Destination).z;
        }
        public float GetDistanceXFromDestination()
        {
            return (creature.transform.position - Destination).x;
        }
        public float GetDistanceFromDestination()
        {
            return Vector3.Distance(creature.transform.position, Destination);
        }
        public float GetDistanceFromGround()
        {
            RaycastHit hit;
            if (Physics.Raycast(creature.transform.position, Vector3.down, out hit))
            {
                float hitY = hit.point.y;
                return creature.transform.position.y - hitY;
            }
            return sustainHeight;
        }
        public Vector3 GetNextDownYCollisionPoint()
        {
            RaycastHit hit;
            if (Physics.Raycast(creature.transform.position, Vector3.down, out hit))
            {
                return hit.point;
            }
            else
            {
                Vector3 dontMoveKeepSustainHeight = creature.transform.position;
                dontMoveKeepSustainHeight.y = sustainHeight;
                return dontMoveKeepSustainHeight;
            }
        }
        private Vector3 GetBeforeCollision(bool _inTime)
        {
            Transform worldOrigin = creature.transform;
            Vector3 relativeVel = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
            RaycastHit hit;
            float distanceZ = float.MaxValue, distanceX = float.MaxValue;
            float rayLength = miscVariables[MiscVariables.AgentMiscVariables.Agent_Detect_Collision_Z_Distance];
            if (Physics.Raycast(worldOrigin.position, creature.transform.forward, out hit, rayLength))
            {
                if(DEBUG)
                    Debug.DrawLine(worldOrigin.position, hit.point, Color.black, .5f);
                distanceZ = Vector3.Distance(worldOrigin.position, hit.point);
            }
            Vector3 direction;
            if (relativeVel.x > 0)
                direction = creature.transform.right;
            else
                direction = -creature.transform.right;
            rayLength = miscVariables[MiscVariables.AgentMiscVariables.Agent_Detect_Collision_X_Distance];
            if (Physics.Raycast(worldOrigin.position + direction, direction, out hit, rayLength))
            {
                if(DEBUG)
                    Debug.DrawLine(worldOrigin.position + direction, hit.point, Color.blue, .5f);
                distanceX = Vector3.Distance(worldOrigin.position, hit.point);
            }
            if (distanceX < 0) distanceX = 0;
            if (distanceZ < 0) distanceZ = 0;
            if(_inTime)
                return new Vector3(distanceX/relativeVel.x, -1, distanceZ/relativeVel.z);
            else
                return new Vector3(distanceX, -1, distanceZ);
        }
        public Vector3 GetDistanceBeforeCollision()
        {
            return GetBeforeCollision(false);
        }
        public Vector3 GetTimeBeforeCollision()
        {
            return GetBeforeCollision(true);
        }
        #endregion CALCULATION METHODS

    // CONSTRUCTOR //
    public CreatureAgent(Creature creature)
        {
            this.creature = creature;

            positionLastUpdate = Vector3.zero;
            lastTimeDistanceChecked = 0;
            distanceMovedInLastFiveSeconds = 0;
            distanceMovedLastUpdate = 0;
            touchingBodies = new List<Transform>();
            MaintainPosition = false;
            miscVariables = MiscVariables.GetAgentMiscVariables(creature);
            Active = true;
        }

        #region MONO METHODS
        // Called from Creature Object //
        public void OnCollisionEnter(Collision collision)
        {
            if (!touchingBodies.Contains(collision.gameObject.transform))
            {
                touchingBodies.Add(collision.gameObject.transform);
            }
            for (int count = 0;count < allParts.Count; count++)
            {
                if (!(allParts[count] is EnginePart)) continue;
                EnginePart currentPart = (EnginePart)allParts[count];
                currentPart.OnCollisionEnter(collision);
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
            for (int count = 0; count < allParts.Count; count++)
            {
                if (!(allParts[count] is EnginePart)) continue;
                EnginePart currentPart = (EnginePart)allParts[count];
                currentPart.OnCollisionExit(collision);
            }
        }
        public void CollisionRemoveIfPresent(Transform collision)
        {
            if(touchingBodies.Contains(collision))
                touchingBodies.Remove(collision);
        }
        // Called from Creature Object //
        public void Update()
        {
            if (!Active) return;
            sinceLastTurnUpdate += Time.deltaTime;
            // Agent updates //
            if (sinceLastTurnUpdate > updateMainAgentEvery)
            {
                updateMainAgentEvery = 0;
                if (landing)
                {
                    if (!OverTarget && Mathf.Abs(GetDistanceZFromDestination()) <
                        miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_OverTarget_If_Z_Dis_Less_Than]
                        && Mathf.Abs(GetDistanceXFromDestination()) <
                        miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_OverTarget_If_X_Dis_Less_Than])
                    {
                        if (rigidbody.velocity.z <
                            miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_OverTarget_If_Z_Vel_Less_Than]
                            && rigidbody.velocity.x <
                            miscVariables[MiscVariables.AgentMiscVariables.Agent_Landing_OverTarget_If_X_Vel_Less_Than])
                        {
                            if (GetDistanceFromGround() > 50)
                            {
                                Debug.LogWarning("Supposed to be over target, but can't see ground");
                            }
                            OverTarget = true;
                            MaintainPosition = true;
                        }
                    }
                }
                if (!ignoreIncomingCollisions && orbitTarget)
                    ignoreIncomingCollisions = true;
            }
            // Part updates //
            foreach (Part part in allParts)
            {
                part.Update();
            }
        }
        #endregion MONO METHODS

    }
    public enum CreaturePart { LEG , FOOT, BODY, ENGINE_Z, ENGINE_Y, ENGINE_X, BRAKE, NONE }
    public enum CreatureLocomotionType { StandardForwardBack, Flight, NONE }
    public enum CreatureAnimationMovementType { Rotation, Inch, NONE }
    public enum CreatureTurnType { Rotate , Shorten, Inch }
}