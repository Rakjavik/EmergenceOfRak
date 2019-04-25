using rak.creatures;
using rak.creatures.memory;
using rak.world;
using Unity.Mathematics;
using UnityEngine;

namespace rak
{
    public struct ActionStep
    {
        public enum Actions { None, Wait, Locate, MoveTo, Use, Add, Eat, Land, Sleep }
        public enum FailReason
        {
            NA, NoKnownFoodProducer, InfinityDistance, MoveToWithNoDestination,
            FailureAddingToInventory, ExceededTimeLimit, CouldntGetToTarget,
            NoKnownFood
        }

        public Actions Action;
        public FailReason failReason { get; private set; }
        public float3 _targetPosition { get; private set; }
        public System.Guid _targetThing { get; private set; }
        public Tasks.CreatureTasks associatedTask { get; private set; }
        public void SetTarget(Thing thing)
        {
            this._targetThing = thing.guid;
            this._targetPosition = thing.transform.position;
            CreatureAgentDestinationHasBeenSet = 1;
        }
        public void SetTargetPosition(float3 target)
        {
            CreatureAgentDestinationHasBeenSet = 1;
            _targetPosition = target;
        }


        public Tasks.TASK_STATUS Status;
        public byte CreatureAgentDestinationHasBeenSet;
        public float ElapsedTime;
        public float MaxAllowedTime;
        public float DistanceRequiredToCompleteModifier;
        private Thing.Base_Types targetBaseType;

        public ActionStep(Actions action, Tasks.CreatureTasks task, float distanceRequiredToCompleteModifier)
        {
            targetBaseType = Thing.Base_Types.NA;
            MaxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.Action = action;
            // No target, set to zero //
            _targetPosition = float3.zero;
            failReason = FailReason.NA;
            CreatureAgentDestinationHasBeenSet = 0;
            DistanceRequiredToCompleteModifier = distanceRequiredToCompleteModifier;
            ElapsedTime = 0;
            Status = Tasks.TASK_STATUS.Started;
        }
        public ActionStep(Actions action, Tasks.CreatureTasks task)
        {
            targetBaseType = Thing.Base_Types.NA;
            MaxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.Action = action;
            // No target, set to zero //
            _targetPosition = float3.zero;
            failReason = FailReason.NA;
            CreatureAgentDestinationHasBeenSet = 0;
            DistanceRequiredToCompleteModifier = 1;
            ElapsedTime = 0;
            Status = Tasks.TASK_STATUS.Started;
        }
        public ActionStep(Actions action, Tasks.CreatureTasks task, Thing.Base_Types targetBaseType)
        {
            this.targetBaseType = targetBaseType;
            MaxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.Action = action;
            // No target, set to zero //
            _targetPosition = float3.zero;
            failReason = FailReason.NA;
            CreatureAgentDestinationHasBeenSet = 0;
            Status = Tasks.TASK_STATUS.Started;
            ElapsedTime = 0;
            DistanceRequiredToCompleteModifier = 1;
        }
        private void Initialize(Actions action, Tasks.CreatureTasks task, float distanceRequiredToCompleteModifier)
        {
            MaxAllowedTime = CreatureConstants.GetMaxAllowedTime(action);
            failReason = FailReason.NA;
            this.associatedTask = task;
            this.Action = action;
            // No target, set to zero //
            _targetPosition = float3.zero;
            failReason = FailReason.NA;
            CreatureAgentDestinationHasBeenSet = 0;
            this.DistanceRequiredToCompleteModifier = distanceRequiredToCompleteModifier;
        }

        // MAIN METHOD //
        public void performAction(Creature performer)
        {
            ElapsedTime += Time.deltaTime;
            //Debug.LogWarning("Elapsed Time - " + elapsedTime + " Max - " + maxAllowedTime);
            if (ElapsedTime > MaxAllowedTime)
            {
                DebugMenu.AppendLine(performer.thingName + " has exceeded task time for task. Resetting");
                Debug.Log(performer.thingName + " has exceeded task time for task. Resetting");
                Status = Tasks.TASK_STATUS.Failed;
                failReason = FailReason.ExceededTimeLimit;
                // TODO LET TARGET THING KNOW IT AVAILABLE AGAIN !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //if (_targetThing != null) _targetThing.MakeAvailable(performer);
                return;
            }
            //Debug.Log("Performing action " + action + " For Task - " + associatedTask);

            // LOCATE //
            if (Action == Actions.Locate)
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
                        Status = Tasks.TASK_STATUS.Failed;
                        return;
                    }
                    else
                    {
                        _targetThing = target.guid;
                        _targetPosition = target.transform.position;
                        // TODO MAKE TARGET UNAVAILABLE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //_targetThing.MakeUnavailable();
                        //Debug.LogWarning("Locate task complete with target - " + _targetThing.name);
                        Status = Tasks.TASK_STATUS.Complete;
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
                    CreatureAgentDestinationHasBeenSet = 1;
                    Status = Tasks.TASK_STATUS.Complete;
                    return;
                }
                // LOCATE GATHER //
                else if (associatedTask == Tasks.CreatureTasks.GATHER)
                {
                    //Thing target = performer.GetClosestKnownReachableThing(targetBaseType);
                }
            }

            // MOVE TO //
            else if (Action == Actions.MoveTo)
            {
                Debug.DrawLine(performer.transform.position, _targetPosition, Color.cyan, .5f);
                // The agent should have a destination before getting to this point //
                if (CreatureAgentDestinationHasBeenSet == 0)
                {
                    failReason = FailReason.MoveToWithNoDestination;
                    //Debug.LogWarning("ERROR MoveTo with no destination");
                    Status = Tasks.TASK_STATUS.Failed;
                    return;
                }
                else // Needs to be an else since it takes a frame for the remaining distance to calculate
                {
                    if (performer.getDistanceFromDestination() == Mathf.Infinity)
                    {
                        Debug.Log("Infinity distance");
                        failReason = FailReason.InfinityDistance;
                        Status = Tasks.TASK_STATUS.Failed;
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
                        Status = Tasks.TASK_STATUS.Complete;
                        return;
                    }
                }
            }
            // ADD //
            else if (Action == Actions.Add)
            {
                CreatureAgent agent = performer.GetCreatureAgent();
                // Tractor Beam //
                if (agent.grabType == CreatureGrabType.TractorBeam)
                {
                    float distance = 20; // IMPLEMENT TARGET COMPONENTS DISTANCE
                    //float distance = Vector3.Distance(_targetThing.transform.position, performer.transform.position);
                    // Touching Target //
                    if (distance < .5f)
                    {
                        // ADD THING TO INVENTORY //
                        Status = Tasks.TASK_STATUS.Complete;
                        return;

                        /*if (performer.AddThingToInventory(_targetThing))
                        {
                            _targetThing.transform.SetParent(performer.transform);
                            Status = Tasks.TASK_STATUS.Complete;
                            return;
                        }
                        else
                        {
                            Debug.LogWarning("Failed adding thing to inventory");
                            failReason = FailReason.FailureAddingToInventory;
                            Status = Tasks.TASK_STATUS.Failed;
                            return;
                        }*/
                    }
                }

            }
            // EAT //
            else if (Action == Actions.Eat)
            {
                // IMPLEMENT CONSUME THING !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                Status = Tasks.TASK_STATUS.Complete;
                /*if (_targetThing.beConsumed(performer))
                {
                    //performer.PlayOneShot();
                    performer.ConsumeThing(_targetThing);
                    Status = Tasks.TASK_STATUS.Complete;
                    return;
                }*/
            }
            // Land //
            else if (Action == Actions.Land)
            {
                return;
            }
            // DEACTIVATE SELF //
            else if (Action == Actions.Sleep)
            {
                if (performer.GetCurrentState() != Creature.CREATURE_STATE.SLEEP)
                    performer.GetCreatureAgent().Sleep();
                if (!performer.StillNeedsSleep())
                    Status = Tasks.TASK_STATUS.Complete;
            }
        }
        #region GETTERS/SETTERS
        public Actions getAction() { return Action; }
        public void OverrideMaxTimeAllowed(float maxTimeAllowedForStep)
        {
            MaxAllowedTime = maxTimeAllowedForStep;
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
            return this.Status == status;
        }
        public void ResetAgentDestionation()
        {
            CreatureAgentDestinationHasBeenSet = 0;
        }
        #endregion
    }
}