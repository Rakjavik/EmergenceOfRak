using rak.creatures.memory;
using System.Collections.Generic;

namespace rak.creatures
{
    public enum Disposition { Friend,Enemy,Unknown,EMPTY}
    public class Personality
    {
        private Dictionary<Creature, Disposition> dispositions;
        private int openness; //  Openness reflects the degree of intellectual curiosity, creativity and a preference for novelty and variety
        private int conscientiousness; // Tendency to be organized and dependable
        private int extraversion; // Energy, positive emotions, surgency, assertiveness, sociability and the tendency to seek stimulation in the company of others
        private int agreeableness; // Tendency to be compassionate and cooperative rather than suspicious and antagonistic towards others
        private int neuroticism; // The tendency to experience unpleasant emotions easily, such as anger, anxiety, depression, and vulnerability

        private Memory memory;
        private Creature parentCreature;

        public Personality(Memory memory,Creature creature)
        {
            this.memory = memory;
            this.parentCreature = creature;
            dispositions = new Dictionary<Creature, Disposition>();
        }

        public Disposition GetDispositionToward(Creature otherCreature)
        {
            Disposition currentDisposition;
            if(dispositions.TryGetValue(parentCreature,out currentDisposition))
            {
                return currentDisposition;
            }
            else
            {
                return CalculateMyDispositionToward(otherCreature);
            }
        }
        public Disposition CalculateMyDispositionToward(Creature otherCreature)
        {
            Disposition saveThisDisposition = Disposition.Unknown;
            if (otherCreature.GetTribe() == parentCreature.GetTribe())
            {
                saveThisDisposition = Disposition.Friend;
            }
            else
            {
                MemoryInstance[] memories = memory.GetAllMemoriesOf(otherCreature);
                if (memories.Length == 0)
                {
                    saveThisDisposition = Disposition.Unknown;
                }
                else
                {
                    saveThisDisposition = Disposition.Enemy;
                }
            }
            dispositions[otherCreature] = saveThisDisposition;
            return saveThisDisposition;
        }
    }
}