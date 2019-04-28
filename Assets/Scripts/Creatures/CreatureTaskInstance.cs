using rak.creatures;
using rak.ecs.ThingComponents;
using rak.world;
using UnityEngine;

namespace rak
{
    public class CreatureTaskInstance
    {
        public Tasks.CreatureTasks taskType { get; private set; }
        private Tasks.TASK_STATUS taskStatus = Tasks.TASK_STATUS.Incomplete;
        public ActionStep[] currentActionSteps { get; private set; }
        private ActionStep[] _previousActionSteps;
        private int _currentStepNum { get; set; }
        private Thing[] _targets;
        private Creature creature;

        public CreatureTaskInstance(Tasks.CreatureTasks taskType, ActionStep[] previousSteps,
            Thing[] targets, Creature actingCreature)
        {
            this.taskType = taskType;
            this._previousActionSteps = previousSteps;
            this.currentActionSteps = CreatureConstants.GetTaskList(taskType);
            this._targets = targets;
            this.creature = actingCreature;
            _currentStepNum = 0;
        }

        public void CancelTask()
        {
            taskStatus = Tasks.TASK_STATUS.Cancelled;
        }

        public string GetCurrentTaskTargetName()
        {
            if (currentActionSteps.Length == 0) return "None";
            return Area.GetThingByGUID(currentActionSteps[_currentStepNum]._targetThing).thingName;
        }
        public ActionStep.FailReason GetPreviousStepsFailReason()
        {
            if (_previousActionSteps != null && _previousActionSteps.Length > 0)
            {
                return _previousActionSteps[_previousActionSteps.Length - 1].failReason;
            }
            return ActionStep.FailReason.NA;
        }

        public ActionStep.Actions GetCurrentAction()
        {
            if (currentActionSteps.Length == 0)
            {
                return ActionStep.Actions.None;
            }
            return currentActionSteps[_currentStepNum].getAction();
        }

        public void performCurrentTask()
        {
            _previousActionSteps = currentActionSteps;
            // DO TASK //
            ActionStep currentStep = currentActionSteps[_currentStepNum];
            //Debug.LogWarning("Current action - " + currentStep.getAction());
            currentStep.performAction(creature);
            // TASK UPDATE COMPLETE - Check status //
            if (currentStep.isStatus(Tasks.TASK_STATUS.Complete))
            {
                //Debug.LogWarning("Step complete - " + currentStep.getAction());
                // Next step //
                if (_currentStepNum != currentActionSteps.Length - 1)
                {
                    ActionStep previousStep = currentActionSteps[_currentStepNum];
                    _currentStepNum++;
                    currentStep = currentActionSteps[_currentStepNum];
                    // If the new step doesn't have a target, and this one did, copy it over, For example Locate //
                    if (!currentStep.HasTargetThing() &&
                        previousStep.HasTargetThing())
                    {
                        currentStep.SetTarget(Area.GetThingByGUID(previousStep._targetThing));
                    }
                    else if (!currentStep.HasTargetPosition() &&
                        previousStep.HasTargetPosition())
                    {
                        currentStep.SetTargetPosition(previousStep._targetPosition);
                    }
                    else
                    {
                        //Debug.LogWarning("No step target from previous, currentstep-previousstep " + currentStep.getAction() + "-" + previousStep.getAction());
                    }
                    creature.SetNavMeshAgentDestination(currentStep._targetPosition);
                    taskStatus = Tasks.TASK_STATUS.Incomplete;
                }
                else
                {
                    taskStatus = Tasks.TASK_STATUS.Complete;
                    creature.RequestObservationUpdate();
                }
                return;
            }
            // FAILED //
            else if (currentStep.isStatus(Tasks.TASK_STATUS.Failed))
            {
                // Notify creature of failure to record/discard as needed //
                creature.SoThisFailed(currentStep);
                ActionStep[] steps = CreatureConstants.GetExceptionActions(taskType, currentStep.failReason,creature);
                if (steps != null)
                {
                    _previousActionSteps = currentActionSteps;
                    currentActionSteps = steps;
                    _currentStepNum = 0;
                    taskStatus = Tasks.TASK_STATUS.Started;
                }
                else
                    taskStatus = Tasks.TASK_STATUS.Cancelled;
                return;
            }
        }
        public Tasks.TASK_STATUS GetCurrentTaskStatus()
        {
            return taskStatus;
        }
        public bool isStatus(Tasks.TASK_STATUS status) { return status == this.taskStatus; }

        private Thing getCurrentTaskStepTarget()
        {
            return Area.GetThingByGUID(currentActionSteps[_currentStepNum]._targetThing);
        }
        public Vector3 GetCurrentTaskDestination()
        {
            Target target = Unity.Entities.World.Active.EntityManager.
                GetComponentData<Target>(creature.ThingEntity);
            return target.targetPosition;
        }
        public Thing GetCurrentTaskTarget()
        {
            return getCurrentTaskStepTarget();
        }
        public ActionStep.FailReason GetCurrentTaskStepFailReason()
        {
            return currentActionSteps[_currentStepNum].failReason;
        }
    }
}