using rak.creatures.memory;
using System;

namespace rak.creatures
{
    public enum BASE_SPECIES { Gnat, Gagk };
    public enum CONSUMPTION_TYPE { HERBIVORE, CARNIVORE, OMNIVORE }
    public class Species
    {
        private char gender;
        private string speciesName;
        private bool intelligent;
        public Personality personality { get; private set; }
        private BASE_SPECIES baseSpecies;
        private CONSUMPTION_TYPE consumptionType;
        public Memory memory { get; private set; }

        public Species(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            CONSUMPTION_TYPE consumptionType)
        {
            Initialize(gender, speciesName, intelligent, baseSpecies, consumptionType,null);
        }
        public Species(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            CONSUMPTION_TYPE consumptionType,Creature creature)
        {
            Initialize(gender, speciesName, intelligent, baseSpecies, consumptionType,creature);
        }
        private void Initialize(char gender, string speciesName, bool intelligent, BASE_SPECIES baseSpecies,
            CONSUMPTION_TYPE consumptionType,Creature creature)
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
        public CONSUMPTION_TYPE getConsumptionType() { return consumptionType; }
        #endregion
    }







}