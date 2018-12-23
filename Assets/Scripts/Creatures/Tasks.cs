using rak.creatures;
using UnityEngine.AI;

namespace rak
{
    public class Tasks
    {
        public enum TASKS { EAT,NONE,SLEEP,EXPLORE,MOVE_AND_OBSERVE };

        public enum TASK_STATUS {
                Incomplete // Task is in progress
                ,Complete // Task is done and ready to move to next Task
                ,Cancelled // Task was cancelled explicitly, not because of an exception
                ,Failed // Exception during handling of task
                ,Started // Awaiting resume
        }

        public static TASKS GetAppropriateTask(Needs.NEEDTYPE taskNeed)
        {
            if(taskNeed == Needs.NEEDTYPE.HUNGER)
            {
                return TASKS.EAT;
            }
            else if (taskNeed == Needs.NEEDTYPE.SLEEP)
            {
                return TASKS.SLEEP;
            }
            else if (taskNeed == Needs.NEEDTYPE.NONE)
            {
                return TASKS.EXPLORE;
            }
            else
            {
                return TASKS.NONE;
            }
        }
    }
}
