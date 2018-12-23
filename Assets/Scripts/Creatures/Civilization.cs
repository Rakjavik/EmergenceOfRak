using rak.world;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rak.creatures
{
    public class Civilization
    {
        public static string GenerateCivName(BASE_SPECIES species)
        {
            if(species == BASE_SPECIES.Gnat)
            {
                return "Gnat Civ " + UnityEngine.Random.Range(0, 50);
            }
            else
            {
                return "No names";
            }
        }

        public const string PREFABTENT = "modelTent";
        public const string PREFABTOWNHOUSE1 = "Town House 01";
        public const string PREFABTOWNHOUSE2 = "Town House 02";
        public const string PREFABTOWNHOUSE3 = "Town House 03";
        public const string PREFABTOWNHOUSE4 = "Town House 04";
        public const string PREFABTOWNWINDMILL = "Town Windmill 01";
        public const int STARTBUILDINGROADSATPOPULATION = 500;

        private BASE_SPECIES race;
        public string CivName { get
            {
                return civName;
            } }
        private string civName;
        private Species[] members;
        private int population;
        private HexCell currentHome;
        
        public Civilization(BASE_SPECIES race,string civName,bool defaults,int minPop,int maxPop)
        {
            if(defaults)
            {
                this.race = race;
                this.civName = civName;
                members = new Species[0];
                population = 0;
            }
            else
            {
                this.race = BASE_SPECIES.Gnat;
                this.civName = "DaGnats";
                members = new Species[5];
                for (int count = 0; count < members.Length; count++)
                {
                    members[count] = new Species('m', "GnatPrime", false,
                        BASE_SPECIES.Gnat, CONSUMPTION_TYPE.OMNIVORE);
                }
                population = UnityEngine.Random.Range(minPop,maxPop);
            }
        }

        public bool FindHome(World world, bool replaceCurrent)
        {
            if (currentHome != null && !replaceCurrent) return false;
            HexCell[] results = world.FindUncivilizedHexCellsNotUnderwater();
            HexCell newHome = null;
            for(int count = 0; count < results.Length; count++)
            {
                newHome = results[count];
            }
            if(newHome)
            {
                currentHome = newHome;
                //newHome.CurrentOccupants = this;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return civName + " - " + race + "\n Population - " + population;
        }

        public int getPopulation()
        {
            return population;
        }
    }
}
