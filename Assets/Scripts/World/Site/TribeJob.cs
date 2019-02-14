namespace rak.world
{
    public class TribeJob
    {
        private JobAction[] actions;
        private JobTasks task;
        public Thing target { get; private set; }
        public Thing.Thing_Types targetType { get; private set; }
        private int currentAction;
        public Tribe tribe { get; private set; }

        public TribeJob(JobTasks task, Thing target,Tribe tribe)
        {
            this.tribe = tribe;
            this.task = task;
            this.target = target;
            actions = JobAction.GetActionsForTask(task,this);
            currentAction = 0;
        }
        public TribeJob(JobTasks task, Thing.Thing_Types type,Tribe tribe)
        {
            this.tribe = tribe;
            this.task = task;
            this.targetType = type;
            actions = JobAction.GetActionsForTask(task, this);
            currentAction = 0;
        }
    }

}