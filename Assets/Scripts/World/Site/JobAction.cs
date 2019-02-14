using System.Collections.Generic;
using UnityEngine;

namespace rak.world
{
    public class JobAction
    {
        private JobActions jobAction;
        private Vector3 _targetPosition;
        private Transform _targetTransform;
        private TribeJob _parentJob;
        public Dictionary<Thing.Thing_Types, int> resourcesRequired { get; private set; }

        public JobAction(JobActions action,Vector3 targetPosition,TribeJob job)
        {
            initialize(action, targetPosition, job);
        }
        public JobAction(JobActions action, Transform target, TribeJob job)
        {
            initialize(action, target, job);
        }
        public JobAction(JobActions action, TribeJob job)
        {
            initialize(action, job);
        }
        private void initialize(JobActions action, Vector3 targetPosition, TribeJob job)
        {
            this._targetPosition = targetPosition;
            this._targetTransform = null;
            initialize(action, job);
        }
        private void initialize(JobActions action, Transform target, TribeJob job)
        {
            this._targetTransform = target;
            this._targetPosition = target.position;
            initialize(action, job);
        }
        private void initialize(JobActions action,TribeJob job)
        {
            this._parentJob = job;
            this.jobAction = action;
            resourcesRequired = new Dictionary<Thing.Thing_Types, int>();
        }


        public void PerformAction()
        {
            if(jobAction == JobActions.DetermineResources)
            {
                Dictionary<Thing.Thing_Types, int> resourcesStillRequired = new Dictionary<Thing.Thing_Types, int>();
                Dictionary<Thing.Thing_Types, int> resourcesNeededForThing;
                if (_parentJob.target is Building)
                {
                    Building building = (Building)_parentJob.target;
                    resourcesNeededForThing = Building.GetResourcesNeededToCreate(building.BuildingType);
                    Debug.LogWarning("Resources needed - " + resourcesNeededForThing);
                }
                else
                    resourcesNeededForThing = new Dictionary<Thing.Thing_Types, int>();
                foreach (Thing.Thing_Types type in resourcesNeededForThing.Keys)
                {
                    int needed = resourcesNeededForThing[type];
                    int owned = _parentJob.tribe.GetAmountOfThingOwned(type);
                    if(needed > owned)
                    {
                        resourcesStillRequired.Add(type, needed - owned);
                    }
                }
                resourcesRequired = resourcesStillRequired;
                _parentJob.tribe.AddTribeJobPosting(new TribeJobPosting
                    (1, Tasks.CreatureTasks.GATHER, _parentJob));
                jobAction = JobActions.WaitForResources;
            }
        }

        public static JobAction[] GetActionsForTask(JobTasks task,TribeJob job)
        {
            JobAction[] actions = null;
            if (task == JobTasks.Build)
            {
                actions = new JobAction[1];
                actions[0] = new JobAction(JobActions.DetermineResources,job);
            }
            if (actions == null)
                Debug.LogWarning("Action list empty for task - " + task);
            return actions;
        }
    }

}