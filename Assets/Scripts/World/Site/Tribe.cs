using rak.creatures;

namespace rak.world
{
    public class Tribe
    {
        public string TribeName { get; private set; }

        private Civilization memberOf;
        private int population;
        private BASE_SPECIES race;
        private HexCell currentHome;

        public Tribe(int population,BASE_SPECIES race)
        {
            this.population = population;
            this.race = race;
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
    }
}
