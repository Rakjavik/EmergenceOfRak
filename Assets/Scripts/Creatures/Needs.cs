using System.Collections.Generic;
using UnityEngine;

namespace rak.creatures
{
    public class Needs
    {
        public enum NEEDTYPE { NONE, HUNGER, THIRST, TEMPERATURE, SLEEP, REPRODUCTION }
        private Dictionary<NEEDTYPE, Need> currentNeeds;
        public Needs(BASE_SPECIES baseSpecies)
        {
            currentNeeds = CreatureConstants.NeedsInitialize(baseSpecies);
        }
        public void IncreaseRelativeNeed(NEEDTYPE need,int relativeAmount)
        {
            currentNeeds[need].IncreaseNeedRelative(relativeAmount);
        }
        public void IncreaseNeed(NEEDTYPE need, float amount)
        {
            currentNeeds[need].IncreaseNeed(amount);
        }
        public void DecreaseNeed(NEEDTYPE need, float amount)
        {
            currentNeeds[need].DecreaseNeed(amount);
        }
        public Need getNeed(NEEDTYPE need)
        {
            return currentNeeds[need];
        }
        public NEEDTYPE getMostUrgent()
        {
            NEEDTYPE highest = NEEDTYPE.NONE;
            foreach(NEEDTYPE need in currentNeeds.Keys)
            {
                if(currentNeeds[need].CurrentAmount >= currentNeeds[highest].CurrentAmount)
                {
                    highest = getMoreImportant(need, highest);
                }
            }
            // No needs if we're fine //
            if (currentNeeds[highest].CurrentAmount == NeedAmount.Fine)
                return NEEDTYPE.NONE;
            return highest;
        }
        private NEEDTYPE getMoreImportant(NEEDTYPE newNeed,NEEDTYPE highestNeed)
        {
            if (newNeed == NEEDTYPE.NONE) return highestNeed;
            else if (highestNeed == NEEDTYPE.NONE) return newNeed;
            if((newNeed == NEEDTYPE.SLEEP && highestNeed == NEEDTYPE.HUNGER) ||
                    (newNeed == NEEDTYPE.HUNGER && highestNeed == NEEDTYPE.SLEEP))
            {
                NeedAmount sleepAmount = currentNeeds[NEEDTYPE.SLEEP].CurrentAmount;
                NeedAmount hungerAmount = currentNeeds[NEEDTYPE.HUNGER].CurrentAmount;
                if(hungerAmount >= NeedAmount.Major)
                {
                    return NEEDTYPE.HUNGER;
                }
                else if (sleepAmount >= NeedAmount.Moderate && hungerAmount <= NeedAmount.Little)
                {
                    return NEEDTYPE.SLEEP;
                }
            }
            return highestNeed;
        }
    }
}