using UnityEngine;

namespace rak.creatures
{
    public enum NeedAmount
    {
        Invalid,Fine,Little,Moderate,Major,Critical
    }
    public class Need
    {
        public Needs.NEEDTYPE needType { get; private set; }
        public float currentAmount { get; private set; }
        public bool TimeBased { get; private set; }
        public NeedAmount CurrentAmount
        {
            get
            {
                if (needType == Needs.NEEDTYPE.NONE)
                    return NeedAmount.Invalid;
                if (relativeAmount == 0)
                    return NeedAmount.Fine;
                else if (relativeAmount < 25)
                    return NeedAmount.Little;
                else if (relativeAmount < 50)
                    return NeedAmount.Moderate;
                else if (relativeAmount < 75)
                    return NeedAmount.Major;
                else
                    return NeedAmount.Critical;
            }
        }
        private int relativeAmount { get
            {
                return (int)(currentAmount / relativeFactor);
            } }
        private float relativeFactor;

        public Need(Needs.NEEDTYPE needType,float relativeFactor,bool timeBased)
        {
            this.relativeFactor = relativeFactor;
            this.needType = needType;
            this.TimeBased = timeBased;
            currentAmount = 0;
        }
        public void IncreaseNeedRelative(int relativeAmount)
        {
            if (TimeBased)
            {
                Debug.LogError("Needs : Call on increase relative amount when Time based");
                return;
            }
            IncreaseNeed(relativeFactor * relativeAmount);
        }
        public void IncreaseTimeBasedNeedInSeconds(float seconds)
        {
            IncreaseNeed(seconds);
        }
        public void DecreaseTimeBasedNeedInSeconds(float seconds)
        {
            DecreaseNeed(seconds);
        }
        public void IncreaseNeed(float amount)
        {
            currentAmount += amount;
        }
        public void DecreaseNeedRelative(int relativeAmount)
        {
            if (TimeBased)
            {
                Debug.LogError("Needs : Call on decrease relative amount when Time based");
                return;
            }
            DecreaseNeed(relativeFactor * relativeAmount);
        }
        public void DecreaseNeed(float amount)
        {
            currentAmount -= amount;
            if (currentAmount < 0) currentAmount = 0;
        }
    }
}