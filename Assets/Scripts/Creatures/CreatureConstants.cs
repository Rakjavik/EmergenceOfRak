using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public abstract class CreatureConstants
    {
        public static bool CreatureIsIncapacitatedState(Creature.CREATURE_STATE state)
        {
            bool _inCapacitated;
            if (state == Creature.CREATURE_STATE.DEAD) _inCapacitated = true;
            else if (state == Creature.CREATURE_STATE.IDLE) _inCapacitated = false;
            else if (state == Creature.CREATURE_STATE.MOVE) _inCapacitated = false;
            else if (state == Creature.CREATURE_STATE.SLEEP) _inCapacitated = true;
            else if (state == Creature.CREATURE_STATE.WAIT) _inCapacitated = false;
            else _inCapacitated = true;
            return _inCapacitated;
        }

        public static Dictionary<Needs.NEEDTYPE, Need> NeedsInitialize(BASE_SPECIES baseSpecies)
        {
            Dictionary<Needs.NEEDTYPE, Need> currentNeeds = new Dictionary<Needs.NEEDTYPE, Need>();
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                currentNeeds.Add(Needs.NEEDTYPE.HUNGER, new Need(Needs.NEEDTYPE.HUNGER, .01f, false));
                currentNeeds.Add(Needs.NEEDTYPE.REPRODUCTION, new Need(Needs.NEEDTYPE.REPRODUCTION, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.SLEEP, new Need(Needs.NEEDTYPE.SLEEP, 100f, true));
                currentNeeds.Add(Needs.NEEDTYPE.TEMPERATURE, new Need(Needs.NEEDTYPE.TEMPERATURE, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.THIRST, new Need(Needs.NEEDTYPE.THIRST, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.NONE, new Need(Needs.NEEDTYPE.NONE, 0, false));
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                currentNeeds.Add(Needs.NEEDTYPE.HUNGER, new Need(Needs.NEEDTYPE.HUNGER, 1f, false));
                currentNeeds.Add(Needs.NEEDTYPE.REPRODUCTION, new Need(Needs.NEEDTYPE.REPRODUCTION, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.SLEEP, new Need(Needs.NEEDTYPE.SLEEP, 1f, true));
                currentNeeds.Add(Needs.NEEDTYPE.TEMPERATURE, new Need(Needs.NEEDTYPE.TEMPERATURE, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.THIRST, new Need(Needs.NEEDTYPE.THIRST, 1, false));
                currentNeeds.Add(Needs.NEEDTYPE.NONE, new Need(Needs.NEEDTYPE.NONE, 0, false));
            }
            return currentNeeds;
        }

        public static float GetDistanceFromTargetBeforeConsideredReached(Creature creature)
        {
            return creature.getCreatureStats().getDistanceFromTargetBeforeConsideredReached();
        }

        public static SpeciesPhysicalStats PhysicalStatsInitialize
            (BASE_SPECIES baseSpecies, Creature creature)
        {
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                return new SpeciesPhysicalStats(
                    creature, // Parent creature
                    SpeciesPhysicalStats.MOVEMENT_TYPE.FLY, // Movement type
                    1, // Size 
                    1, // Growth
                    1, // Insulation
                    1, // Amount of Food Required
                    10, // Speed
                    1, // Reproduction Rate
                    1, // Gestation Time
                    10, // Number Per Birth
                    1, // Typical max age
                    1,  // Update Every in Seconds
                    10f, // The distance from target before it's considered interactable
                    1); // Needs to sleep after being awake for this many seconds
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                return new SpeciesPhysicalStats(
                    creature, // Parent creature
                    SpeciesPhysicalStats.MOVEMENT_TYPE.FLY, // Movement type
                    10, // Size 
                    1, // Growth
                    1, // Insulation
                    1, // Amount of Food Required
                    1, // Speed
                    1, // Reproduction Rate
                    1, // Gestation Time
                    10, // Number Per Birth
                    1, // Typical max age
                    1,  // Update Every in Seconds
                    1f, // The distance from target before it's considered interactable
                    1); // Needs to sleep after being awake for this many seconds
            }
            else
            {
                return null;
            }
        }

        public static void CreatureAgentInitialize(BASE_SPECIES baseSpecies, CreatureAgent agent)
        {
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                agent.GetRigidBody().constraints = RigidbodyConstraints.None;
                agent.GetRigidBody().maxAngularVelocity = 5;
                agent.SetExploreRadiusModifier(200);
                agent.SetGrabType(CreatureGrabType.TractorBeam);
                BuildCreatureParts(baseSpecies, agent);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                agent.SetExploreRadiusModifier(200);
                BuildCreatureParts(baseSpecies, agent);
            }
        }

        public static void SetPropertiesForParticleSystemByCreature(ParticleSystem ps, Creature creature)
        {
            ps.Stop();
            if (creature.getSpecies().getBaseSpecies() == BASE_SPECIES.Gnat)
            {
                ParticleSystem.MainModule main = ps.main;
                main.duration = .3f;
                main.loop = false;
                main.prewarm = false;
                main.startDelay = 0;
                main.startLifetime = .3f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(10, 20);
                main.startSize = .01f;
                main.startColor = Color.yellow;
                ParticleSystem.EmissionModule emission = ps.emission;
                emission.rateOverTime = 750;
                emission.rateOverDistance = 0;
                ParticleSystem.ShapeModule shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Donut;
                shape.radius = 1;
                shape.donutRadius = .2f;
                shape.radiusThickness = 1;
                shape.arc = 360;
                shape.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;
                shape.arcSpread = 0;
                shape.rotation = new Vector3(90, 0, 0);
                shape.scale = new Vector3(.04f, .04f, .1f);
                shape.randomDirectionAmount = 1;
                ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.normalDirection = 1;
                renderer.material = RAKUtilities.getMaterial("GnatSpark");
                renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
                renderer.minParticleSize = .01f;
                renderer.maxParticleSize = .05f;
                renderer.lengthScale = .2f;
            }
        }

        public static ThingAnimationPart[] GetPartsForThingAgent(Thing thing)
        {
            List<ThingAnimationPart> parts = new List<ThingAnimationPart>();
            ThingAnimationPart part = new ThingAnimationPart(ThingPartAnimationType.RotatePart,
                Vector3.up,thing.transform.GetChild(1),50);
            parts.Add(part);
            return parts.ToArray();
        }

        public static void BuildCreatureParts(BASE_SPECIES baseSpecies, CreatureAgent agent)
        {
            Creature creature = agent.creature;
            Rigidbody rigidbody = agent.GetRigidBody();
            List<Part> allParts = new List<Part>();
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                Transform gnatMaster = creature.transform.GetChild(0);
                // Body with constant force //
                EnginePart bodyFlight = new EnginePart(PartAudioPropToModify.PITCH, CreaturePart.BODY, 
                    gnatMaster.transform.parent, CreatureLocomotionType.Flight, .2f);

                // Body with Rotation turning //
                TurnPart bodyTurning = new TurnPartRotation
                    (CreaturePart.BODY, gnatMaster.transform.parent, CreatureTurnType.Rotate, .2f);

                // Backward Propeller //
                AnimationPart zPropeller = new AnimationPart(CreaturePart.ENGINE_Z, gnatMaster.transform.GetChild(1),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceZ, PartAnimationType.Movement, true);

                // Y Propellers //
                AnimationPart yPropeller = new AnimationPart(CreaturePart.ENGINE_Y, gnatMaster.transform.GetChild(3),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller2 = new AnimationPart(CreaturePart.ENGINE_Y, gnatMaster.transform.GetChild(4),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller3 = new AnimationPart(CreaturePart.ENGINE_Y, gnatMaster.transform.GetChild(5),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller4 = new AnimationPart(CreaturePart.ENGINE_Y, gnatMaster.transform.GetChild(6),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);

                // Antigrav Shield //
                AntiGravityShieldPart shieldPart = new AntiGravityShieldPart(CreaturePart.SHIELD, gnatMaster.transform.GetChild(7),
                    .2f, creature.GetCreatureAgent().GetRigidBody(), new ActionStep.Actions[] {ActionStep.Actions.Add,
                        ActionStep.Actions.Locate,ActionStep.Actions.None,ActionStep.Actions.Wait});
                AnimationPart antiGravShieldAnimation = new AnimationPart(CreaturePart.SHIELD, gnatMaster.transform.GetChild(7),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.right, 10, new ActionStep.Actions[]
                    {ActionStep.Actions.Add,ActionStep.Actions.MoveTo,ActionStep.Actions.Add,ActionStep.Actions.Eat
                    ,ActionStep.Actions.Locate,ActionStep.Actions.None,ActionStep.Actions.Wait},
                    PartMovesWith.IsKinematic, PartAnimationType.Movement, false);

                // Tractor Beam //
                TractorBeamPart tractorBeam = new TractorBeamPart(gnatMaster.transform, .2f, 30);
                TractorBeamAnimationPart tractorAnimation = new TractorBeamAnimationPart(CreaturePart.TRACTORBEAM,
                    gnatMaster.transform.GetChild(8), .2f, Vector3.forward, .3f);

                // Light Arm //
                LightArmPart lightArm = new LightArmPart(CreaturePart.LIGHT_ARM, 
                    gnatMaster.transform.parent.GetChild(1),.05f);

                allParts.Add(bodyFlight);
                allParts.Add(bodyTurning);
                allParts.Add(zPropeller);
                allParts.Add(yPropeller);
                allParts.Add(yPropeller2);
                allParts.Add(yPropeller3);
                allParts.Add(yPropeller4);
                allParts.Add(shieldPart);
                allParts.Add(antiGravShieldAnimation);
                allParts.Add(tractorBeam);
                allParts.Add(tractorAnimation);
                allParts.Add(lightArm);
                agent.SetCreatureTurnType(CreatureTurnType.Rotate);
                agent.setParts(allParts);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {

            }
        }

        public static float GetMaxAllowedTime(ActionStep.Actions action)
        {
            float maxAllowed = 30f; // Default to 30 seconds
            if (action == ActionStep.Actions.Eat)
            {
                maxAllowed = 60f; // 1 minute
            }
            else if (action == ActionStep.Actions.Land)
            {
                maxAllowed = 60f; // one minute
            }
            else if (action == ActionStep.Actions.Sleep)
            {
                maxAllowed = float.MaxValue;
            }
            return maxAllowed;
        }

        public static ActionStep[] GetTaskList(Tasks.CreatureTasks task)
        {
            ActionStep[] steps = null;
            if (task == Tasks.CreatureTasks.EAT)
            {
                steps = new ActionStep[4];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task);
                steps[2] = new ActionStep(ActionStep.Actions.Add, task);
                steps[3] = new ActionStep(ActionStep.Actions.Eat, task);
            }
            else if (task == Tasks.CreatureTasks.SLEEP)
            {
                steps = new ActionStep[4];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task, 5);
                steps[2] = new ActionStep(ActionStep.Actions.Land, task);
                steps[3] = new ActionStep(ActionStep.Actions.Sleep, task);
            }
            else if (task == Tasks.CreatureTasks.EXPLORE)
            {
                steps = new ActionStep[2];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task, 30);
            }
            else if (task == Tasks.CreatureTasks.GATHER)
            {
                steps = new ActionStep[2];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task, Thing.Base_Types.PLANT);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task);
            }
            else if (task == Tasks.CreatureTasks.MOVE_AND_OBSERVE)
            {
                steps = new ActionStep[1];
                steps[0] = new ActionStep(ActionStep.Actions.MoveTo, task,30);
            }
            else
            {
                steps = new ActionStep[0];
            }
            return steps;
        }

        private static Tasks.CreatureTasks GetExceptionTask(Tasks.CreatureTasks task, ActionStep.FailReason failReason)
        {
            return Tasks.CreatureTasks.EXPLORE;
        }
        
        // EXCEPTION ACTIONS, Do these when tasks fail for a certain reason //
        public static ActionStep[] GetExceptionActions(Tasks.CreatureTasks task, ActionStep.FailReason failReason,
            Creature creature)
        {
            Tasks.CreatureTasks exceptionTask = Tasks.CreatureTasks.NONE;
            // Couldn't locate Sleeping spot //
            if (task == Tasks.CreatureTasks.SLEEP && failReason == ActionStep.FailReason.InfinityDistance)
            {
                exceptionTask = Tasks.CreatureTasks.EXPLORE;
            }
            // Don't know of any food //
            if (task == Tasks.CreatureTasks.EAT && failReason == ActionStep.FailReason.NoKnownFoodProducer)
            {
                exceptionTask = Tasks.CreatureTasks.EXPLORE;
            }
            else if (task == Tasks.CreatureTasks.EAT && failReason == ActionStep.FailReason.NoKnownFood)
            {
                exceptionTask = Tasks.CreatureTasks.MOVE_AND_OBSERVE;
            }
            ActionStep[] steps = GetTaskList(exceptionTask);
            // If we're exploring looking for food, only explore for a little bit //
            if (task == Tasks.CreatureTasks.EAT && failReason == ActionStep.FailReason.NoKnownFoodProducer)
            {
                //steps[1].OverrideMaxTimeAllowed(6f);
            }
            else if (task == Tasks.CreatureTasks.EAT && failReason == ActionStep.FailReason.NoKnownFood)
            {
                Thing[] foodProducers = creature.GetKnownConsumeableProducers();
                
                List<Thing> validThings = new List<Thing>();
                for (int count = 0; count < foodProducers.Length; count++)
                {
                    Thing producer = foodProducers[count];
                    //Debug.LogWarning("Distance - " + Vector3.Distance(producer.transform.position, creature.transform.position));
                    if(Vector3.Distance(producer.transform.position,creature.transform.position) > 300)
                    {
                        validThings.Add(producer);
                    }
                }
                Thing foodProducer = null;
                if(validThings.Count > 0)
                {
                    foodProducer = validThings[Random.Range(0, validThings.Count)];
                }
                if (foodProducer != null)
                {
                    steps[0].SetTargetPosition(foodProducer.transform.position);
                    creature.SetNavMeshAgentDestination(foodProducer.transform.position);
                }
                else
                {
                    //Debug.LogWarning("NO valid producers");
                    return GetExceptionActions(Tasks.CreatureTasks.EAT, ActionStep.FailReason.NoKnownFoodProducer, creature);
                }
            }
            return steps;
        }

        public static List<MovementState> GetStatesCanSwithTo(MovementState currentState)
        {
            List<MovementState> possibleStates = new List<MovementState>();
            // Destroyed can be switched to at any point //
            possibleStates.Add(MovementState.DESTROYED);
            if (currentState == MovementState.FORWARD)
            {
                possibleStates.Add(MovementState.IDLE);
                possibleStates.Add(MovementState.REVERSE);
                possibleStates.Add(MovementState.POWER_DOWN);
            }
            else if (currentState == MovementState.IDLE)
            {
                possibleStates.Add(MovementState.FORWARD);
                possibleStates.Add(MovementState.POWER_DOWN);
                possibleStates.Add(MovementState.REVERSE);
            }
            else if (currentState == MovementState.REVERSE)
            {
                possibleStates.Add(MovementState.IDLE);
                possibleStates.Add(MovementState.FORWARD);
                possibleStates.Add(MovementState.POWER_DOWN);
            }
            else if (currentState == MovementState.POWER_DOWN)
            {
                possibleStates.Add(MovementState.UNINITIALIZED);
                possibleStates.Add(MovementState.STARTING);
            }
            else if (currentState == MovementState.UNINITIALIZED)
            {
                possibleStates.Add(MovementState.STARTING);
            }
            else if (currentState == MovementState.STARTING)
            {
                possibleStates.Add(MovementState.IDLE);
                possibleStates.Add(MovementState.FORWARD);
                possibleStates.Add(MovementState.REVERSE);
            }
            return possibleStates;
        }

        public static AudioClip GetCreaturePartAudioClip(BASE_SPECIES species, CreaturePart part)
        {
            if (species == BASE_SPECIES.Gnat)
            {
                if (part == CreaturePart.BRAKE)
                {
                    return RAKUtilities.getAudioClip("GnatBrake");
                }
            }
            return null;
        }
    }
}