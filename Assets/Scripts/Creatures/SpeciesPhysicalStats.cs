using UnityEngine;

namespace rak.creatures
{
    public partial class SpeciesPhysicalStats
    {
        public enum MOVEMENT_TYPE { INCH, CRAWL, WALK, FLY}

        private Needs needs;
        private Creature creature;
        private int size;
        private int growth;
        private int insulation;
        private int amountOfFoodRequired;
        private int needsSleepAfterInSeconds;
        private int speed;
        private MOVEMENT_TYPE movementType;
        private int reproductionRate;
        private int gestationTime;
        private int numberPerBirth;
        private int typicalMaxAge;
        private bool _initialized = false;
        public bool IsInitialized() { return _initialized; }
        public int updateEvery;
        private float distanceFromTargetBeforeConsideredReached;

        public SpeciesPhysicalStats(Creature parent, MOVEMENT_TYPE movementType,int size, int growth, int insulation,
            int amountOfFoodRequried, int speed, int reproductionRate,int gestationTime, int numberPerBirth,
            int typicalMaxAge,int updateEvery,float distanceFromTargetBeforeConsideredReached,int needsSleepAfter)
        {
            this.creature = parent;
            this.size = size;
            this.growth = growth;
            this.insulation = insulation;
            this.amountOfFoodRequired = amountOfFoodRequried;
            this.speed = speed;
            this.movementType = movementType;
            this.reproductionRate = reproductionRate;
            this.gestationTime = gestationTime;
            this.numberPerBirth = numberPerBirth;
            this.typicalMaxAge = typicalMaxAge;
            this.updateEvery = updateEvery;
            this.distanceFromTargetBeforeConsideredReached = distanceFromTargetBeforeConsideredReached;
            this.needsSleepAfterInSeconds = needsSleepAfter;
        }
        public void Initialize(BASE_SPECIES baseSpecies)
        {
            needs = new Needs(baseSpecies);
            _initialized = true;
        }
        
        /// <summary>
        /// Track regular increments of needs, should only be called on each creature update
        /// </summary>
        public void Update()
        {
            if (!_initialized) return;
            float hungerIncrement = (size * amountOfFoodRequired)*.01f;
            needs.IncreaseNeed(Needs.NEEDTYPE.HUNGER, hungerIncrement);
            if (creature.GetCurrentState() == Creature.CREATURE_STATE.SLEEP)
            {
                needs.DecreaseNeed(Needs.NEEDTYPE.SLEEP, Time.deltaTime*2);
            }
            else
            {
                needs.IncreaseNeed(Needs.NEEDTYPE.SLEEP, Time.deltaTime);
            }
        }

        #region GETTERS/SETTERS
        public Needs getNeeds() { return needs; }
        public int GetSpeed() { return speed; }
        public int getMaxWeight()
        {
            return size * 5;
        }
        public float getDistanceFromTargetBeforeConsideredReached()
        {
            return distanceFromTargetBeforeConsideredReached;
        }
        public MOVEMENT_TYPE GetMovementType() { return movementType; }
        #endregion
    }







}