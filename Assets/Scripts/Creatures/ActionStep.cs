using rak.creatures;
using rak.creatures.memory;
using rak.world;
using Unity.Mathematics;
using UnityEngine;

namespace rak
{
    public class ActionStep
    {
        public enum Actions { None, Wait, Locate, MoveTo, Use, Add, Eat, Land, Sleep }
        public enum FailReason
        {
            NA, NoKnownFoodProducer, InfinityDistance, MoveToWithNoDestination,
            FailureAddingToInventory, ExceededTimeLimit, CouldntGetToTarget,
            NoKnownFood
        }

        private Actions action;
        public FailReason failReason { get; private set; }
        public float3 _targetPosition { get; private set; }
        public Thing _targetThing { get; private set; }
        public Tasks.CreatureTasks associatedTask { get; private set; }
        public void SetTarget(Thing thing)
        {
            this._targetThing = thing;
            this._targetPosition = thing.transform.position;
            creatureAgentDestinationHasBeenSet = true;
        }
        public void SetTargetPosition(float3 target)
        {
            creatureAgentDestinationHasBeenSet = true;
            _targetPosition = target;
        }


        private Tasks.TASK_STATUS status = Tasks.TASK_STATUS.Incomplete;
        private bool creatureAgentDestinationHasBeenSet;
        private float elapsedTime = 0;
        private float maxAllowedTime = float.MaxValue;
        private float distanceRequiredToCompleteModifier;
        private Thing.Base_Types targetBaseType;

        public ActionStep(Actions action, Tasks.CreatureTasks task, float distanceRequiredToCompleteModifier)
        {
            Initialize(action, task, distanceRequiredToCompleteModifier);
        }
        public ActionStep(Actions action, Tasks.CreatureTasks task)
        {
            Initialize(action, task, 1);
        }
        public ActionStep(Actions action, Tasks.CreatureTasks task, Thing.Base_Types targetBaseType)
        {
            this.targetBaseType = targetBaseType;
            Initialize(action, task, 1);
        }
        private void Initialize(Actions action, Tasks.CreatureTasks task, float distanceRequiredToCompleteModifier)
        {
            maxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.action = action;
            // No target, set to zero //
            _targetPosition = float3.zero;
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
                DebugMenu.AppendLine(performer.thingName + " has exceeded task time for task. Resetting");
                Debug.Log(performer.thingName + " has exceeded task time for task. Resetting");
                status = Tasks.TASK_STATUS.Failed;
                failReason = FailReason.ExceededTimeLimit;
                if (_targetThing != null) _targetThing.MakeAvailable(performer);
                return;
            }
            //Debug.Log("Performing action " + action + " For Task - " + associatedTask);

            // LOCATE //
            if (action == Actions.Locate)
            {
                performer.RequestObservationUpdate();
                // LOCATE EAT //
                if (associatedTask == Tasks.CreatureTasks.EAT)
                {
                    //Debug.LogWarning("Locating consumeable");
                    Thing target = performer.GetClosestKnownReachableConsumable();
                    if (target == null)
                    {
                        // Target doesn't know of any consumables //
                        failReason = FailReason.NoKnownFood;
                        status = Tasks.TASK_STATUS.Failed;
                        return;
                    }
                    else
                    {
                        _targetThing = target;
                        _targetPosition = target.transform.position;
                        _targetThing.MakeUnavailable();
                        //Debug.LogWarning("Locate task complete with target - " + _targetThing.name);
                        status = Tasks.TASK_STATUS.Complete;
                    }
                }
                // LOCATE SLEEP //
                else if (associatedTask == Tasks.CreatureTasks.SLEEP)
                {
                    
                }
                // LOCATE EXPLORE //
                else if (associatedTask == Tasks.CreatureTasks.EXPLORE)
                {
                    //Debug.LogWarning("Locating explore sector");
                    GridSector sector = performer.GetClosestUnexploredSector();
                    if (!sector.IsEmpty())
                    {
                        float3 explorePoint = sector.GetRandomPositionInSector();
                        float terrainHeight = sector.GetTerrainHeightFromGlobalPos(explorePoint);
                        explorePoint = new float3(
                            explorePoint.x,
                            terrainHeight + performer.GetCreatureAgent().GetSustainHeight(),
                            explorePoint.z);
                        _targetPosition = explorePoint;
                    }
                    else
                    {
                        //Debug.Log("No unexplored sectors!");
                        _targetPosition = performer.GetRandomKnownSectorPosition();
                    }
                    performer.GetCreatureAgent().SetDestination(_targetPosition);
                    creatureAgentDestinationHasBeenSet = true;
                    status = Tasks.TASK_STATUS.Complete;
                    return;
                }
                // LOCATE GATHER //
                else if (associatedTask == Tasks.CreatureTasks.GATHER)
                {
                    //Thing target = performer.GetClosestKnownReachableThing(targetBaseType);
                }
            }

            // MOVE TO //
            else if (action == Actions.MoveTo)
            {
                Debug.DrawLine(performer.transform.position, _targetPosition, Color.cyan, .5f);
                // The agent should have a destination before getting to this point //
                if (!creatureAgentDestinationHasBeenSet)
                {
                    failReason = FailReason.MoveToWithNoDestination;
                    //Debug.LogWarning("ERROR MoveTo with no destination");
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
                    if (associatedTask == Tasks.CreatureTasks.MOVE_AND_OBSERVE)
                        performer.RequestObservationUpdate();
                    // Raycast if we're close enough to the target to where we should be able to see it //
                    float distanceBeforeRayCastCheckOnTarget = performer.miscVariables
                        [MiscVariables.CreatureMiscVariables.Agent_MoveTo_Raycast_For_Target_When_Distance_Below];
                    // Arrived //
                    float distanceToCompleteArrival;
                    if (associatedTask != Tasks.CreatureTasks.EXPLORE)
                        distanceToCompleteArrival = performer.getCreatureStats().getDistanceFromTargetBeforeConsideredReached();
                    else
                        distanceToCompleteArrival = 10;
                    if (performer.getDistanceFromDestination() <= distanceToCompleteArrival)
                    {
                        status = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                }
            }
            // ADD //
            else if (action == Actions.Add)
            {
                CreatureAgent agent = performer.GetCreatureAgent();
                // Tractor Beam //
                if (agent.grabType == CreatureGrabType.TractorBeam)
                {
                    float distance = Vector3.Distance(_targetThing.transform.position, performer.transform.position);
                    // Touching Target //
                    if (distance < .5f)
                    {
                        if (performer.AddThingToInventory(_targetThing))
                        {
                            performer.GetCreatureAgent().CollisionRemoveIfPresent(_targetThing.transform);
                            _targetThing.transform.SetParent(performer.transform);
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
                }

            }
            // EAT //
            else if (action == Actions.Eat)
            {
                if (_targetThing.beConsumed(performer))
                {
                    //performer.PlayOneShot();
                    performer.ConsumeThing(_targetThing);
                    status = Tasks.TASK_STATUS.Complete;
                    return;
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
            if ((Vector3)_targetPosition != Vector3.zero)
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
        public void ResetAgentDestionation()
        {
            creatureAgentDestinationHasBeenSet = false;
        }
        #endregion
    }
}