using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public abstract class CreatureConstants
    {
        public static bool IsThisFarAwayFromWallsIfNotFixIt(float distance, Vector3 point)
        {
            return IsThisFarAwayFromAnyWalls(distance, point, true);
        }
        private static bool IsThisFarAwayFromAnyWalls(float distance, Vector3 point,bool fixAsWeGo)
        {
            bool noWalls = true;
            RaycastHit hit;
            if (Physics.Raycast(point, Vector3.left, out hit))
            {
                if (Vector3.Distance(point, hit.point) < distance && !fixAsWeGo)
                    if(!fixAsWeGo)
                        noWalls = false;
                    else
                        point.x += 5;
            }
            if (Physics.Raycast(point, -Vector3.left, out hit))
            {
                if (Vector3.Distance(point, hit.point) < distance && !fixAsWeGo)
                    if (!fixAsWeGo)
                        noWalls = false;
                    else
                        point.x -= 5;
            }
            if (Physics.Raycast(point, Vector3.forward, out hit))
            {
                // Adjust origin so we're not hitting floor //
                point.y += .1f;
                if (Vector3.Distance(point, hit.point) < distance && !fixAsWeGo)
                    if (!fixAsWeGo)
                        noWalls = false;
                    else
                        point.z -= 5;
            }
            if (Physics.Raycast(point, -Vector3.forward, out hit))
            {
                // Adjust height of origin so we're not hitting floor //
                point.y += .1f;
                if (Vector3.Distance(point, hit.point) < distance && !fixAsWeGo)
                {
                    if (!fixAsWeGo)
                        noWalls = false;
                    else
                        point.z += 5;
                }
            }
            return noWalls;
        }

        public static bool IsAtLeastThisFarAwayFromAnyWalls(float distance,Vector3 point)
        {
            if (point == Vector3.zero) return false;
            return IsThisFarAwayFromAnyWalls(distance, point, false);
        }

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

        public static Dictionary<Needs.NEEDTYPE,Need> NeedsInitialize(BASE_SPECIES baseSpecies)
        {
            Dictionary<Needs.NEEDTYPE,Need> currentNeeds = new Dictionary<Needs.NEEDTYPE, Need>();
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                currentNeeds.Add(Needs.NEEDTYPE.HUNGER, new Need(Needs.NEEDTYPE.HUNGER, 100f, false));
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
            (BASE_SPECIES baseSpecies,Creature creature)
        {
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                return new SpeciesPhysicalStats(
                    creature, // Parent creature
                    SpeciesPhysicalStats.MOVEMENT_TYPE.FLY, // Movement type
                    1, // Size 
                    1, // Growth
                    1, // Insulation
                    10, // Amount of Food Required
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

        public static void CreatureAgentInitialize(BASE_SPECIES baseSpecies,CreatureAgent agent)
        {
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                agent.GetRigidBody().constraints = RigidbodyConstraints.None;
                agent.SetTurnSpeed(.3f);
                agent.SetSustainHeight(5);
                agent.SetMaxAngularVel(10);
                agent.SetMaxVelocityMagnitude(20);
                agent.SetCruisingSpeed(new Vector3(2,1,15));

                // Maximum force of the constant force component //
                agent.maxForce.y = 15;
                agent.maxForce.z = 8;
                agent.maxForce.x = 4;
                // Amount of force needed to hold the objects weight //
                agent.SetMinimumForceToHover(8);
                agent.SetSlowDownModifier(1);
                agent.GetRigidBody().maxAngularVelocity = 5;
                agent.SetExploreRadiusModifier(200);
                agent.SetGrabType(CreatureGrabType.TractorBeam);
                BuildCreatureParts(baseSpecies, agent);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                agent.SetTurnSpeed(1f);
                agent.SetSustainHeight(1);
                agent.SetMaxAngularVel(15);
                agent.SetMaxVelocityMagnitude(10);
                agent.SetCruisingSpeed(new Vector3(0, 0, 5));

                // Maximum force of the constant force component //
                agent.maxForce.y = 0;
                agent.maxForce.z = 5;
                agent.maxForce.x = 0;
                // Amount of force needed to hold the objects weight //
                agent.SetMinimumForceToHover(0);
                agent.SetSlowDownModifier(15);
                agent.SetExploreRadiusModifier(200);
                BuildCreatureParts(baseSpecies, agent);
            }
        }
        public static void SetPropertiesForParticleSystemByCreature(ParticleSystem ps,Creature creature)
        {
            ps.Stop();
            if(creature.getSpecies().getBaseSpecies() == BASE_SPECIES.Gnat)
            {
                ParticleSystem.MainModule main = ps.main;
                main.duration = .3f;
                main.loop = false;
                main.prewarm = false;
                main.startDelay = 0;
                main.startLifetime = .3f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(10, 20);
                main.startSize = .1f;
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
                shape.scale = new Vector3(.4f, .4f, 1);
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
        public static void BuildCreatureParts(BASE_SPECIES baseSpecies,CreatureAgent agent)
        {
            Creature creature = agent.creature;
            Rigidbody rigidbody = agent.GetRigidBody();
            List<Part> allParts = new List<Part>();
            if (baseSpecies == BASE_SPECIES.Gnat)
            {
                // Body with constant force //
                EnginePart bodyFlight = new EnginePart
                    (CreaturePart.BODY, creature.transform.GetChild(0), CreatureLocomotionType.Flight, .2f);
                
                // Body with Rotation turning //
                TurnPart bodyTurning = new TurnPartRotation
                    (CreaturePart.BODY, creature.transform.GetChild(0), CreatureTurnType.Rotate, .2f);

                // Backward Propeller //
                AnimationPart zPropeller = new AnimationPart(CreaturePart.ENGINE_Z, creature.transform.GetChild(1),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceZ,PartAnimationType.Movement,true);

                // Y Propellers //
                AnimationPart yPropeller = new AnimationPart(CreaturePart.ENGINE_Y, creature.transform.GetChild(3),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller2 = new AnimationPart(CreaturePart.ENGINE_Y, creature.transform.GetChild(4),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller3 = new AnimationPart(CreaturePart.ENGINE_Y, creature.transform.GetChild(5),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);
                AnimationPart yPropeller4 = new AnimationPart(CreaturePart.ENGINE_Y, creature.transform.GetChild(6),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.ConstantForceY, PartAnimationType.Movement, true);

                /* Brake disc //
                AnimationPart brakeDisc = new AnimationPart(CreaturePart.BRAKE, creature.transform.GetChild(2),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    { ActionStep.Actions.Add,ActionStep.Actions.Land,ActionStep.Actions.MoveTo,ActionStep.Actions.Wait,ActionStep.Actions.Sleep},
                    PartMovesWith.Braking, PartAnimationType.Particles,creature.transform.GetChild(8).GetComponent<Light>(), true);
                BrakePart brakePart = new BrakePart(CreaturePart.BRAKE, creature.transform.GetChild(2), .1f);
                */
                // Antigrav Shield //
                AntiGravityShieldPart shieldPart = new AntiGravityShieldPart(CreaturePart.SHIELD, creature.transform.GetChild(9),
                    .2f,creature.GetCreatureAgent().GetRigidBody(), new ActionStep.Actions[] {ActionStep.Actions.Add,
                        ActionStep.Actions.Locate,ActionStep.Actions.None,ActionStep.Actions.Wait});
                AnimationPart antiGravShieldAnimation = new AnimationPart(CreaturePart.SHIELD, creature.transform.GetChild(9),
                    CreatureAnimationMovementType.Rotation, .05f, Vector3.up, 10, new ActionStep.Actions[]
                    {ActionStep.Actions.Add,ActionStep.Actions.MoveTo,ActionStep.Actions.Add,ActionStep.Actions.Eat
                    ,ActionStep.Actions.Locate,ActionStep.Actions.None,ActionStep.Actions.Wait}, 
                    PartMovesWith.IsKinematic, PartAnimationType.Movement, false);

                // Tractor Beam //
                TractorBeamPart tractorBeam = new TractorBeamPart(creature.transform, .2f,5);
                TractorBeamAnimationPart tractorAnimation = new TractorBeamAnimationPart(CreaturePart.TRACTORBEAM,
                    creature.transform.GetChild(10),.5f,Vector3.forward,.3f);

                allParts.Add(bodyFlight);
                allParts.Add(bodyTurning);
                allParts.Add(zPropeller);
                allParts.Add(yPropeller);
                allParts.Add(yPropeller2);
                allParts.Add(yPropeller3);
                allParts.Add(yPropeller4);
                //allParts.Add(brakeDisc);
                allParts.Add(shieldPart);
                allParts.Add(antiGravShieldAnimation);
                allParts.Add(tractorBeam);
                allParts.Add(tractorAnimation);
                agent.SetCreatureTurnType(CreatureTurnType.Rotate);
                agent.setParts(allParts);
            }
            else if (baseSpecies == BASE_SPECIES.Gagk)
            {
                // Constant force for Z //
                EnginePart mainEngine = new EnginePart(CreaturePart.ENGINE_Z, 
                    creature.transform.GetChild(0),
                    CreatureLocomotionType.StandardForwardBack, 1);
                TurnPart inchSpine = new TurnPartInching(CreaturePart.BODY,
                    creature.transform.GetChild(0),
                    CreatureTurnType.Inch, .5f, Direction.X,
                    creature.transform.GetChild(1));
                allParts.Add(mainEngine);
                allParts.Add(inchSpine);
                agent.SetCreatureTurnType(CreatureTurnType.Inch);
                agent.setParts(allParts);
            }
        }

        public static float GetMaxAllowedTime(ActionStep.Actions action)
        {
            float maxAllowed = .30f;
            if (action == ActionStep.Actions.Eat)
            {
                maxAllowed = 1f; // 1 minute
            }
            else if (action == ActionStep.Actions.Land)
            {
                maxAllowed = 1f; // one minute
            }
            else if (action == ActionStep.Actions.Sleep)
            {
                maxAllowed = float.MaxValue;
            }
            return maxAllowed;
        }

        public static ActionStep[] GetTaskList(Tasks.TASKS task)
        {
            ActionStep[] steps = null;
            if (task == Tasks.TASKS.EAT)
            {
                steps = new ActionStep[4];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task);
                steps[2] = new ActionStep(ActionStep.Actions.Add, task);
                steps[3] = new ActionStep(ActionStep.Actions.Eat, task);
            }
            else if (task == Tasks.TASKS.SLEEP)
            {
                steps = new ActionStep[4];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task,5);
                steps[2] = new ActionStep(ActionStep.Actions.Land, task);
                steps[3] = new ActionStep(ActionStep.Actions.Sleep, task);
            }
            else if (task == Tasks.TASKS.EXPLORE)
            {
                steps = new ActionStep[2];
                steps[0] = new ActionStep(ActionStep.Actions.Locate, task);
                steps[1] = new ActionStep(ActionStep.Actions.MoveTo, task,30);
            }
            return steps;
        }

        private static Tasks.TASKS GetExceptionTask(Tasks.TASKS task, ActionStep.FailReason failReason)
        {
            return Tasks.TASKS.EXPLORE;
        }
        public static ActionStep[] GetExceptionActions(Tasks.TASKS task, ActionStep.FailReason failReason)
        {
            Tasks.TASKS exceptionTask = Tasks.TASKS.NONE;
            // Couldn't locate Sleeping spot //
            if (task == Tasks.TASKS.SLEEP && failReason == ActionStep.FailReason.InfinityDistance)
            {
                exceptionTask = Tasks.TASKS.EXPLORE;
            }
            // Don't know of any food //
            if(task == Tasks.TASKS.EAT && failReason == ActionStep.FailReason.NoneKnown)
            {
                exceptionTask = Tasks.TASKS.EXPLORE;
            }
            ActionStep[] steps = GetTaskList(exceptionTask);
            // If we're exploring looking for food, only explore for a little bit //
            if (task == Tasks.TASKS.EAT && exceptionTask == Tasks.TASKS.EXPLORE)
            {
                steps[1].OverrideMaxTimeAllowed(.3f);
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
        public static AudioClip GetCreaturePartAudioClip(BASE_SPECIES species,CreaturePart part)
        {
            if(species == BASE_SPECIES.Gnat)
            {
                if(part == CreaturePart.BRAKE)
                {
                    return RAKUtilities.getAudioClip("GnatBrake");
                }
            }
            return null;
        }
    }
}