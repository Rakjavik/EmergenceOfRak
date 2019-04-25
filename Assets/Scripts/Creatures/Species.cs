using rak.creatures.memory;
using System;

namespace rak.creatures
{
    public enum BASE_SPECIES { Gnat, Gagk };
    public enum ConsumptionType { HERBIVORE, CARNIVORE, OMNIVORE }
    public class Species
    {
        private char gender;
        private string speciesName;
        private bool intelligent;
        public Personality personality { get; private set; }
        private BASE_SPECIES baseSpecies;
        private ConsumptionType consumptionType;
        public Memory memory { get; private set; }

        public Species(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            ConsumptionType consumptionType)
        {
            Initialize(gender, speciesName, intelligent, baseSpecies, consumptionType,null);
        }
        public Species(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            ConsumptionType consumptionType,Creature creature)
        {
            Initialize(gender, speciesName, intelligent, baseSpecies, consumptionType,creature);
        }
        private void Initialize(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            ConsumptionType consumptionType,Creature creature)
        {
            this.memory = new Memory();
            this.speciesName = speciesName;
            this.gender = gender;
            this.intelligent = intelligent;
            this.baseSpecies = baseSpecies;
            this.consumptionType = consumptionType;
            if (intelligent)
            {
                personality = new Personality(memory, creature);
            }
            else
            {
                personality = null;
            }
        }
        
        #region GETTERS, SETTERS
        public BASE_SPECIES getBaseSpecies() { return baseSpecies; }
        public ConsumptionType getConsumptionType() { return consumptionType; }
        #endregion
    }







}