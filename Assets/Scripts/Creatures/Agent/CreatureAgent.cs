using rak.ecs.ThingComponents;
using rak.world;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures
{
    public class CreatureAgent
    {
        public static bool DEBUG = rak.world.World.ISDEBUGSCENE;
        private bool initialized = false;
        
        // Not active skips update method //
        public bool Active { get; private set; }
        // Size of the boxcast when looking for explore targets //
        public float ExploreRadiusModifier { get; private set; }
        // Creature object being controlled //
        public Creature creature { get; private set; }
        // Type of turning this creature uses //
        public CreatureTurnType creatureTurnType { get; private set; }
        public CreatureLocomotionType locomotionType { get; private set; }
        public CreatureGrabType grabType { get; private set; }

        private Rigidbody rigidbody;
        // All body parts in agent //
        private Part[] allParts;
        private float lastUpdate = 0;
        private Dictionary<MiscVariables.AgentMiscVariables, float> miscVariables;

        #region GETTERS/SETTERS
        public CreatureLocomotionType GetMoveType()
        {
            return locomotionType;
        }
        public Thing GetCurrentActionTarget()
        {
            return creature.GetCurrentActionTarget();
        }
        public ActionStep.Actions GetCurrentCreatureAction()
        {
            return creature.GetCurrentAction();
        }
        public void SetExploreRadiusModifier(float exploreRadiusModifier)
        {
            this.ExploreRadiusModifier = exploreRadiusModifier;
        }
        public void SetGrabType(CreatureGrabType grabType)
        {
            this.grabType = grabType;
        }
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
        public ConstantForce GetConstantForceComponent() { return rigidbody.GetComponent<ConstantForce>(); }
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
        public float GetSustainHeight()
        {
            EntityManager em = Unity.Entities.World.Active.EntityManager;
            Engine engine = em.GetComponentData<Engine>(creature.ThingEntity);
            return engine.sustainHeight;
        }
        public void SetDestination(float3 destination)
        {
            EntityManager em = Unity.Entities.World.Active.EntityManager;
            Target target = new Target
            {
                targetEntity = Entity.Null,
                targetPosition = destination
            };
            em.SetComponentData(creature.ThingEntity, target);

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
        public void EnableAgent()
        {
            if (Active)
                Debug.LogError("Call to enable agent when already active");
            foreach(Part part in allParts)
            {
                part.Enable();
            }
            rigidbody.constraints = RigidbodyConstraints.None;
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
                Active = false;
            }
            else
                Debug.LogWarning("Call to deactivate Creature agent when already disabled");
        }
        public void Sleep()
        {
            creature.ChangeState(Creature.CreatureState.SLEEP);
        }
        #endregion MISC METHODS

        #region CALCULATION METHODS
        public float GetDistanceFromDestination()
        {
            EntityManager manager = Unity.Entities.World.Active.EntityManager;
            Target target = manager.GetComponentData<Target>(creature.ThingEntity);
            return target.distance;
        }
        #endregion CALCULATION METHODS

        // CONSTRUCTOR //
        public CreatureAgent(Creature creature)
        {
            this.creature = creature;
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
            if (baseSpecies == BASE_SPECIES.Gnat)
                locomotionType = CreatureLocomotionType.Flight;
            else if (baseSpecies == BASE_SPECIES.Gagk)
                locomotionType = CreatureLocomotionType.StandardForwardBack;
            initialized = true;
        }

        #region MONO METHODS
        // Called from Creature Object //
        public void Update(float delta, bool visible)
        {
            if (!Active) return;
            lastUpdate += delta;
            EntityManager manager = Unity.Entities.World.Active.EntityManager;

            Visible agentVariables = manager.
                GetComponentData<Visible>(creature.ThingEntity);
            float3 relativeVel = creature.transform.InverseTransformDirection(rigidbody.velocity);
            Quaternion rotation = creature.transform.rotation;
            float4 currentRot = new float4(rotation.x, rotation.y, rotation.z, rotation.w);
            byte visibleByte = 0;
            if (visible) visibleByte = 1;
            Position pos = new Position { Value = creature.transform.position };
            manager.SetComponentData(creature.ThingEntity, pos);
            Visible agentData = new Visible
            {
                Value = visibleByte,
            };
            manager.SetComponentData(creature.ThingEntity, agentData);
            manager.SetComponentData(creature.ThingEntity, new Rotation { Value = currentRot });
            manager.SetComponentData(creature.ThingEntity, new Velocity
            {
                RelativeVelocity = relativeVel,
                NormalVelocity = rigidbody.velocity,
                AngularVelocity = rigidbody.angularVelocity,
            });
            // If In View update everything normally //
            if (visible)
            {
                // Part updates //
                foreach (Part part in allParts)
                {
                    part.Update(delta);
                }
                
            }
            // If not in view, manually move without physics //
            else if (false == true)
            {
                ActionStep.Actions currentAction = creature.GetCurrentAction();
                if (currentAction == ActionStep.Actions.MoveTo)
                {
                    Engine engine = manager.GetComponentData<Engine>(creature.ThingEntity);
                    Vector3 newPosition = engine.NonPhysicsPositionUpdate;
                    GridSector currentSector = creature.currentSector;

                    if (!currentSector.IsEmpty())
                    {
                        float terrainY = creature.currentSector.GetTerrainHeightFromGlobalPos(creature.transform.position);
                        creature.transform.position = new Vector3
                        {
                            x = newPosition.x,
                            y = terrainY + engine.sustainHeight,
                            z = newPosition.z
                        };
                        pos.Value = creature.transform.position;
                    }
                    manager.SetComponentData(creature.ThingEntity, agentData);
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