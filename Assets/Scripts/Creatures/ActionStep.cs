using rak.creatures;
using rak.creatures.memory;
using UnityEngine;

namespace rak
{
    public class ActionStep
    {
        public enum Actions { None, Wait, Locate, MoveTo, Use, Add, Eat, Land, Sleep }
        public enum FailReason { NA, NoneKnown, InfinityDistance, MoveToWithNoDestination,
            FailureAddingToInventory, ExceededTimeLimit, CouldntGetToTarget
        }

        private Actions action;
        public FailReason failReason { get; private set; }
        public Vector3 _targetPosition { get; private set; }
        public Thing _targetThing { get; private set; }
        public Tasks.TASKS associatedTask { get; private set; }
        public void SetTarget(Thing thing)
        {
            this._targetThing = thing;
            this._targetPosition = thing.transform.position;
            creatureAgentDestinationHasBeenSet = true;
        }
        public void SetTargetPosition(Vector3 target)
        {
            creatureAgentDestinationHasBeenSet = true;
            _targetPosition = target;
        }
        

        private Tasks.TASK_STATUS status = Tasks.TASK_STATUS.Incomplete;
        private bool creatureAgentDestinationHasBeenSet;
        private bool breakExploreForConsumeable;
        private float elapsedTime = 0;
        private float maxAllowedTime = float.MaxValue;
        private float distanceRequiredToCompleteModifier;

        public ActionStep(Actions action,Tasks.TASKS task,float distanceRequiredToCompleteModifier)
        {
            Initialize(action, task, distanceRequiredToCompleteModifier);
        }
        public ActionStep(Actions action, Tasks.TASKS task)
        {
            Initialize(action, task, 1);
        }
        private void Initialize(Actions action, Tasks.TASKS task, float distanceRequiredToCompleteModifier)
        {
            maxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.action = action;
            // No target, set to zero //
            _targetPosition = Vector3.zero;
            failReason = FailReason.NA;
            creatureAgentDestinationHasBeenSet = false;
            this.distanceRequiredToCompleteModifier = distanceRequiredToCompleteModifier;
        }

        // MAIN METHOD //
        public void performAction(Creature performer)
        {
            elapsedTime += Time.deltaTime;
            //Debug.LogWarning("Elapsed Time - " + elapsedTime + " Max - " + maxAllowedTime);
            if (elapsedTime > maxAllowedTime)
            {
                Debug.Log(performer.name + " has exceeded task time for task. Resetting");
                status = Tasks.TASK_STATUS.Failed;
                failReason = FailReason.ExceededTimeLimit;
                if (_targetThing != null) _targetThing.MakeAvailable(performer);
                return;
            }
            //Debug.Log("Performing action " + action + " For Task - " + associatedTask);
            
            // LOCATE //
            if (action == Actions.Locate)
            {
                // LOCATE EAT //
                if (associatedTask == Tasks.TASKS.EAT)
                {
                    Thing target = performer.GetClosestKnownReachableConsumable();

                    if (target == null)
                    {
                        // Target doesn't know of any consumables //
                        failReason = FailReason.NoneKnown;
                        status = Tasks.TASK_STATUS.Failed;
                        return;
                    }
                    else
                    {
                        _targetThing = target;
                        _targetPosition = target.transform.position;
                        _targetThing.MakeUnavailable();
                        status = Tasks.TASK_STATUS.Complete;
                    }
                }
                // LOCATE SLEEP //
                else if (associatedTask == Tasks.TASKS.SLEEP)
                {
                    // Search for target ground //
                    float boxSizeMult = performer.miscVariables[MiscVariables.CreatureMiscVariables.Agent_Locate_Sleep_Area_BoxCast_Size_Multipler];
                    Vector3 raycastHit = performer.BoxCastNotMeGetClosestPoint(5, Vector3.down);
                    
                    if (raycastHit != Vector3.positiveInfinity)
                    {
                        raycastHit.y = performer.transform.position.y;
                        _targetPosition = raycastHit;
                        performer.GetCreatureAgent().SetDestination(_targetPosition);
                        creatureAgentDestinationHasBeenSet = true;
                        status = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                    else
                    {
                        // Get's picked up by the TaskManager Update() //
                        status = Tasks.TASK_STATUS.Failed;
                        failReason = FailReason.InfinityDistance;
                    }
                }
                // LOCATE EXPLORE //
                else if (associatedTask == Tasks.TASKS.EXPLORE)
                {
                    Vector3 explorePoint = performer.transform.position;
                    if (explorePoint.y < performer.GetCreatureAgent().GetSustainHeight())
                        explorePoint.y = performer.GetCreatureAgent().GetSustainHeight();
                    explorePoint.z += Random.Range(0, 50);
                    explorePoint.x += Random.Range(0, 50);
                    _targetPosition = explorePoint;
                    performer.GetCreatureAgent().SetDestination(_targetPosition);
                    creatureAgentDestinationHasBeenSet = true;
                    status = Tasks.TASK_STATUS.Complete;
                    return;
                }
            }
            
            // MOVE TO //
            else if (action == Actions.MoveTo)
            {
                // The agent should have a destination before getting to this point //
                if (!creatureAgentDestinationHasBeenSet)
                {
                    failReason = FailReason.MoveToWithNoDestination;
                    Debug.LogError("ERROR MoveTo with no destination");
                    status = Tasks.TASK_STATUS.Failed;
                    return;
                }
                else // Needs to be an else since it takes a frame for the remaining distance to calculate
                {
                    if (performer.getDistanceFromDestination() == Mathf.Infinity)
                    {
                        Debug.Log("Infinity distance");
                        failReason = FailReason.InfinityDistance;
                        status = Tasks.TASK_STATUS.Failed;
                        return;
                    }
                    
                    // Raycast if we're close enough to the target to where we should be able to see it //
                    float distanceBeforeRayCastCheckOnTarget = performer.miscVariables
                        [MiscVariables.CreatureMiscVariables.Agent_MoveTo_Raycast_For_Target_When_Distance_Below];
                    // Arrived //
                    if (performer.getDistanceFromDestination() <= distanceRequiredToCompleteModifier *
                        performer.getCreatureStats().getDistanceFromTargetBeforeConsideredReached())
                    {
                        status = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                    // Haven't arrived yet
                    else if (_targetThing != null)
                    {
                        // Check if we're close enough to start raycasting //
                        if (performer.getDistanceFromDestination() < distanceBeforeRayCastCheckOnTarget)
                        {
                            if (_targetThing != null)
                            {
                                // Raycast forward //
                                if (!performer.IsTargetInFrontOfMe(_targetThing))
                                {
                                    performer.GetCreatureAgent().SetOrbitTarget(true);
                                    performer.GetCreatureAgent().SetIgnoreCollisions(false);
                                }
                                else
                                {
                                    performer.GetCreatureAgent().SetOrbitTarget(false);
                                    performer.GetCreatureAgent().SetIgnoreCollisions(true);
                                }
                            }
                        }
                        // Not close enough, make sure orbiting is disabled //
                        else
                        {
                            performer.GetCreatureAgent().SetOrbitTarget(false);
                            performer.GetCreatureAgent().SetIgnoreCollisions(false);
                        }
                    }
                }
            }
            // ADD //
            else if (action == Actions.Add)
            {
                if (performer.AddThingToInventory(_targetThing))
                {
                    _targetThing.GetComponent<Renderer>().enabled = false;
                    performer.GetCreatureAgent().CollisionRemoveIfPresent(_targetThing.transform);
                    status = Tasks.TASK_STATUS.Complete;
                    return;
                }
                else
                {
                    Debug.LogWarning("Failed adding thing to inventory");
                    failReason = FailReason.FailureAddingToInventory;
                    status = Tasks.TASK_STATUS.Failed;
                    return;
                }

            }
            // EAT //
            else if (action == Actions.Eat)
            {
                if (_targetThing.beConsumed(performer))
                {
                    performer.PlayOneShot();
                    performer.ConsumeThing(_targetThing);
                    status = Tasks.TASK_STATUS.Complete;
                }
            }
            // Land //
            else if (action == Actions.Land)
            {
                CreatureAgent agent = performer.GetCreatureAgent();
                if (!agent.IsLanding())
                {
                    // Landing failed //
                    agent.Land();
                    return;

                }
                if (agent.HasCompletedLanding())
                {
                    status = Tasks.TASK_STATUS.Complete;
                }
                return;
            }
            // DEACTIVATE SELF //
            else if (action == Actions.Sleep)
            {
                if (performer.GetCurrentState() != Creature.CREATURE_STATE.SLEEP)
                    performer.GetCreatureAgent().Sleep();
                if (!performer.StillNeedsSleep())
                    status = Tasks.TASK_STATUS.Complete;
            }
        }
        #region GETTERS/SETTERS
        public Actions getAction() { return action; }
        public void OverrideMaxTimeAllowed(float maxTimeAllowedForStep)
        {
            this.maxAllowedTime = maxTimeAllowedForStep;
        }
        public bool HasTargetPosition()
        {
            if(_targetPosition != Vector3.zero)
            {
                return true;
            }
            return false;
        }
        public bool HasTargetThing()
        {
            if (_targetThing != null)
                return true;
            return false;
        }
        public bool isStatus(Tasks.TASK_STATUS status)
        {
            return this.status == status;
        }
        #endregion
    }
}
