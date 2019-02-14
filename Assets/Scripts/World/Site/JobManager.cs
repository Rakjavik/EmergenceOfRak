using rak.creatures;
using System.Collections.Generic;
using UnityEngine;

namespace rak.world
{
    public enum JobTasks { Build }
    public enum JobActions { DetermineResources, WaitForResources, DeliverResource, Construct }

    public class JobManager
    {
        private List<TribeJobPosting> tribeJobPostings;
        private List<TribeJob> jobQueue;
        private Tribe tribe;
        private float updateEvery = .5f;
        private float lastUpdated = 0;

        public JobManager(Tribe tribe)
        {
            this.tribe = tribe;
            jobQueue = new List<TribeJob>();
            tribeJobPostings = new List<TribeJobPosting>();
        }

        public void Update()
        {
            lastUpdated += Time.deltaTime;
            if(lastUpdated >= updateEvery)
            {
                lastUpdated = 0;
                if(jobQueue.Count == 0)
                {
                    jobQueue.Add(new TribeJob(JobTasks.Build, Thing.Thing_Types.House,tribe));
                }
            }
        }

        public void AddJobPosting(TribeJobPosting posting)
        {
            tribeJobPostings.Add(posting);
        }

        public TribeJobPosting GetJobPosting(Creature creature)
        {
            if(tribeJobPostings.Count > 0)
            {
                return tribeJobPostings[0];
            }
            return null;
        }

    }
    public class TribeJobPosting
    {
        private float urgency;
        public Tasks.CreatureTasks requestedTask { get; private set; }
        private TribeJob associatedJob;

        public TribeJobPosting(float urgency, Tasks.CreatureTasks task,TribeJob parentJob)
        {
            this.urgency = urgency;
            this.requestedTask = task;
            this.associatedJob = parentJob;
        }
    }
}