using rak.creatures;
using rak.creatures.memory;
using rak.world;
using UnityEngine;

namespace rak
{
    public class TaskManager
    {

        private CreatureTaskInstance currentTask;

        private bool busy;
        private Creature creature;

        public TaskManager(Creature creature)
        {
            currentTask = new CreatureTaskInstance(Tasks.CreatureTasks.NONE, new ActionStep[0], new Thing[0], creature);
            busy = false;
            this.creature = creature;
        }
        // Get a new task for the specified creature using needs //
        public void GetNewTask(Creature creature)
        {
            Needs.NEEDTYPE highestNeed = Needs.NEEDTYPE.NONE;
            highestNeed = creature.getCreatureStats().getNeeds().getMostUrgent();
            Tasks.CreatureTasks neededTask = Tasks.CreatureTasks.NONE;
            // No Needs based tasked //
            /*if (highestNeed == Needs.NEEDTYPE.NONE)
            {
                TribeJobPosting posting = creature.GetTribe().GetJobPosting(creature);
                if(posting != null)
                {
                    neededTask = posting.requestedTask;
                }
            }
            else*/
            neededTask = Tasks.GetAppropriateTask(highestNeed);

            if (neededTask == Tasks.CreatureTasks.SLEEP && creature.GetCurrentState() == Creature.CREATURE_STATE.SLEEP)
            {
                Debug.LogWarning("Task is sleep, already asleep");
                return;
            }
            ActionStep.FailReason failReason = currentTask.GetPreviousStepsFailReason();
            // Previous task was failed //
            if (failReason != ActionStep.FailReason.NA)
            {
                if (neededTask == Tasks.CreatureTasks.EAT &&
                    failReason == ActionStep.FailReason.NoneKnown)
                {
                    neededTask = Tasks.CreatureTasks.EXPLORE;
                }
            }
            ActionStep[] steps = CreatureConstants.GetTaskList(neededTask);
            startNewTask(neededTask);
        }
        private void startNewTask(Tasks.CreatureTasks neededTask)
        {
            currentTask = new CreatureTaskInstance(neededTask, currentTask.currentActionSteps, new Thing[0], creature);
            busy = true;
        }
        public bool hasTask()
        {
            return currentTask.taskType != Tasks.CreatureTasks.NONE && !currentTask.isStatus(Tasks.TASK_STATUS.Complete);
        }
        public Tasks.CreatureTasks getCurrentTaskType()
        {
            return currentTask.taskType;
        }


        public void clearAllTasks()
        {
            if (creature.GetCurrentState() == Creature.CREATURE_STATE.SLEEP)
                creature.ChangeState(Creature.CREATURE_STATE.IDLE);
            busy = false;
            currentTask.CancelTask();
        }

        public void PerformCurrentTask()
        {
            currentTask.performCurrentTask();
        }

        public Tasks.TASK_STATUS GetCurrentTaskStatus()
        {
            return currentTask.GetCurrentTaskStatus();
        }

        public ActionStep.Actions GetCurrentAction()
        {
            return currentTask.GetCurrentAction();
        }

        public Thing GetCurrentTaskTarget()
        {
            return currentTask.GetCurrentTaskTarget();
        }
        public Vector3 GetCurrentTaskDestination()
        {
            return currentTask.GetCurrentTaskDestination();
        }
        public string GetCurrentTaskTargetName()
        {
            return currentTask.GetCurrentTaskTargetName();
        }
    }
}