using rak.creatures;
using rak.creatures.memory;
using UnityEngine;

namespace rak
{
    public class TaskManager
    {
        private int currentStepNum;
        private Tasks.TASKS currentTask;
        private ActionStep[] currentActionSteps;
        private bool busy;
        private Creature creature;
        private Tasks.TASK_STATUS status = Tasks.TASK_STATUS.Incomplete;
        private ActionStep[] _previousActionSteps;

        public TaskManager(Creature creature)
        {
            currentTask = Tasks.TASKS.NONE;
            busy = false;
            this.creature = creature;
        }
        // Get a new task for the specified creature using needs //
        public void GetNewTask(Creature creature)
        {
            Needs.NEEDTYPE highestNeed = Needs.NEEDTYPE.NONE;
            highestNeed = creature.getCreatureStats().getNeeds().getMostUrgent();
            Tasks.TASKS neededTask = Tasks.GetAppropriateTask(highestNeed);
            if(neededTask == Tasks.TASKS.SLEEP && creature.GetCurrentState() == Creature.CREATURE_STATE.SLEEP)
            {
                Debug.LogWarning("Task is sleep, already asleep");
                return;
            }
            // Previous task was failed //
            if (_previousActionSteps != null)
            {
                if (_previousActionSteps[_previousActionSteps.Length - 1].failReason != ActionStep.FailReason.NA)
                {
                    if (neededTask == Tasks.TASKS.EAT &&
                        _previousActionSteps[_previousActionSteps.Length - 1].failReason == ActionStep.FailReason.NoneKnown)
                    {
                        neededTask = Tasks.TASKS.EXPLORE;
                    }
                }
            }
            ActionStep[] steps = CreatureConstants.GetTaskList(neededTask);
            startNewTask(steps,neededTask);
        }
        private void startNewTask(ActionStep[] steps,Tasks.TASKS neededTask)
        {
            currentTask = neededTask;
            currentActionSteps = steps;
            currentStepNum = 0;
            busy = true;
        }
        public bool hasTask()
        {
            return (currentTask != Tasks.TASKS.NONE);
        }
        public Tasks.TASKS getCurrentTask()
        {
            return currentTask;
        }
        public string GetCurrentTaskTargetName()
        {
            if (currentActionSteps == null || currentActionSteps.Length == 0 
                || currentActionSteps[currentStepNum] == null ||
                currentActionSteps[currentStepNum]._targetThing == null) return "None";
            return currentActionSteps[currentStepNum]._targetThing.name;
        }
        public ActionStep.Actions GetCurrentAction()
        {
            if(currentActionSteps == null || currentActionSteps[currentStepNum] == null)
            {
                return ActionStep.Actions.None;
            }
            return currentActionSteps[currentStepNum].getAction();
        }
        public void clearAllTasks()
        {
            if (creature.GetCurrentState() == Creature.CREATURE_STATE.SLEEP)
                creature.ChangeState(Creature.CREATURE_STATE.IDLE);
            busy = false;
            currentStepNum = 0;
            currentTask = Tasks.TASKS.NONE;
            status = Tasks.TASK_STATUS.Cancelled;
        }
        public void performCurrentTask()
        {
            _previousActionSteps = currentActionSteps;
            // DO TASK //
            ActionStep currentStep = currentActionSteps[currentStepNum];
            currentStep.performAction(creature);
            // TASK UPDATE COMPLETE - Check status //
            if (currentStep.isStatus(Tasks.TASK_STATUS.Complete))
            {
                // End of task //
                if (currentStepNum == currentActionSteps.Length - 1)
                {
                    clearAllTasks();
                }
                // Next step //
                else
                {
                    ActionStep previousStep = currentActionSteps[currentStepNum];
                    currentStepNum++;
                    currentStep = currentActionSteps[currentStepNum];
                    // If the new step doesn't have a target, and this one did, copy it over, For example Locate //
                    if(!currentStep.HasTargetThing() && 
                        previousStep.HasTargetThing())
                    {
                        currentStep.SetTarget(previousStep._targetThing);
                    }
                    else if (!currentStep.HasTargetPosition() &&
                        previousStep.HasTargetPosition())
                    {
                        currentStep.SetTargetPosition(previousStep._targetPosition);
                    }
                    creature.SetNavMeshAgentDestination(currentStep._targetPosition);
                    status = Tasks.TASK_STATUS.Incomplete;
                }
            }
            // FAILED //
            else if (currentStep.isStatus(Tasks.TASK_STATUS.Failed))
            {
                // Notify creature of failure to record/discard as needed //
                creature.SoThisFailed(currentStep);
                ActionStep[] steps = CreatureConstants.GetExceptionActions(currentTask, currentStep.failReason);
                if (steps != null)
                    startNewTask(steps, steps[0].associatedTask);
                else
                    status = Tasks.TASK_STATUS.Cancelled;
                return;
            }
            if(status == Tasks.TASK_STATUS.Cancelled)
            {
                clearAllTasks();
            }
        }
        public Tasks.TASK_STATUS GetCurrentTaskStatus()
        {
            return status;
        }
        public bool isStatus(Tasks.TASK_STATUS status) { return status == this.status; }

        private Thing getCurrentTaskStepTarget()
        {
            return currentActionSteps[currentStepNum]._targetThing;
        }
        public Thing GetCurrentTaskTarget()
        {
            return getCurrentTaskStepTarget();
        }
        public ActionStep.FailReason GetCurrentTaskStepFailReason()
        {
            return currentActionSteps[currentStepNum].failReason;
        }
    }
}
