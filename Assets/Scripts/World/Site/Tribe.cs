using rak.creatures;
using System.Collections.Generic;

namespace rak.world
{
    public class Tribe
    {
        public string TribeName { get; private set; }

        private JobManager jobManager;

        private Civilization memberOf;
        private int population;
        private BASE_SPECIES race;
        private HexCell currentHome;

        public Tribe(int population,BASE_SPECIES race)
        {
            this.population = population;
            this.race = race;
        }
        public void Initialize()
        {
            jobManager = new JobManager(this);
        }
        public void AddTribeJobPosting(TribeJobPosting posting)
        {
            jobManager.AddJobPosting(posting);
        }
        public TribeJobPosting GetJobPosting(Creature creature)
        {
            return jobManager.GetJobPosting(creature);
        }
        public HexCell FindHome(World world, bool replaceCurrent)
        {
            if (currentHome != null && !replaceCurrent) return currentHome;
            HexCell[] results = world.FindUncivilizedHexCellsNotUnderwater();
            HexCell newHome = null;
            for (int count = 0; count < results.Length; count++)
            {
                newHome = results[count];
                if (newHome) break;
            }
            if (newHome)
            {
                currentHome = newHome;
                currentHome.CurrentOccupants = this;
            }
            return currentHome;
        }
        public int GetPopulation() { return population; }

        public void Update()
        {
            jobManager.Update();
        }

        public Thing[] GetThingsOwnedByTribe()
        {
            Thing[] allThings = Area.GetAllThings().ToArray();
            List<Thing> owned = new List<Thing>();

            foreach (Thing thing in allThings)
            {
                if(thing.owner == this)
                {
                    owned.Add(thing);
                }
            }
            return owned.ToArray();
        }
        public int GetAmountOfThingOwned(Thing.Thing_Types type)
        {
            Thing[] owned = GetThingsOwnedByTribe();
            int numberOwned = 0;
            foreach (Thing thing in owned)
            {
                if(thing.thingType == type)
                {
                    numberOwned++;
                }
            }
            return numberOwned;
        }
    }
}
