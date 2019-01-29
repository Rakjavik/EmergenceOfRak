using rak.creatures;
using UnityEngine.AI;

namespace rak
{
    public class Tasks
    {
        public enum CreatureTasks { EAT,NONE,SLEEP,EXPLORE,MOVE_AND_OBSERVE, GATHER };
        
        public enum TASK_STATUS {
                Incomplete // Task is in progress
                ,Complete // Task is done and ready to move to next Task
                ,Cancelled // Task was cancelled explicitly, not because of an exception
                ,Failed // Exception during handling of task
                ,Started // Awaiting resume
        }

        public static CreatureTasks GetAppropriateTask(Needs.NEEDTYPE taskNeed)
        {
            if(taskNeed == Needs.NEEDTYPE.HUNGER)
            {
                return CreatureTasks.EAT;
            }
            else if (taskNeed == Needs.NEEDTYPE.SLEEP)
            {
                return CreatureTasks.SLEEP;
            }
            else if (taskNeed == Needs.NEEDTYPE.NONE)
            {
                return CreatureTasks.EXPLORE;
            }
            else
            {
                return CreatureTasks.NONE;
            }
        }
    }
}
