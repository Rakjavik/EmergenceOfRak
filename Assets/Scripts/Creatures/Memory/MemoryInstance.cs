using System;
using Unity.Mathematics;

namespace rak.creatures.memory
{
    [Serializable]
    public struct MemoryInstance
    {
        public Verb verb { get; private set; }
        public byte invertVerb { get; set; }
        public Guid subject { get; private set; }
        public float timeStamp { get; private set; }
        public int iterations { get; private set; }
        public byte Edible { get; private set; }
        public float3 Position { get; private set; }

        public MemoryInstance(Verb verb, Guid subject, bool invertVerb, float timestamp,
            Thing.Base_Types subjectType, ConsumptionType creatureConsumeType,float3 position)
        {
            if (invertVerb)
                this.invertVerb = 1;
            else
                this.invertVerb = 0;
            this.verb = verb;
            this.subject = subject;
            this.timeStamp = timestamp;
            iterations = 0;
            Edible = 0;
            if (creatureConsumeType == ConsumptionType.OMNIVORE)
            {
                if (subjectType == Thing.Base_Types.PLANT)
                {
                    Edible = 1;
                }
            }
            Position = position;

        }
        private void setInvertVerb(bool invertVerb)
        {
            if (invertVerb)
                this.invertVerb = 1;
            else
                this.invertVerb = 0;
        }
        public bool GetInvertVerb()
        {
            return getInvertVerb(invertVerb);
        }
        private bool getInvertVerb(short invertVerb)
        {
            if (invertVerb == 0)
                return false;
            else
                return true;
        }
        public void MakeEmpty()
        {
            verb = Verb.NA;
        }
        public bool IsEmpty()
        {
            if (verb == Verb.NA)
                return true;
            return false;
        }
        public static MemoryInstance GetNewEmptyMemory()
        {
            return new MemoryInstance(Verb.NA, Guid.Empty, false,0,Thing.Base_Types.NA,
                ConsumptionType.CARNIVORE,float3.zero);
        }
        public void ReplaceMemory(Verb verb, Guid subject, bool invertVerb)
        {
            this.verb = verb;
            this.subject = subject;
            setInvertVerb(invertVerb);
            timeStamp = DateTime.Now.ToBinary();
            iterations = 0;
        }

        public void AddIteration() { iterations++; }
        public bool IsSameAs(Verb verb, Guid subject, bool invertVerb)
        {
            if (subject != Guid.Empty)
            {
                if (invertVerb == getInvertVerb(this.invertVerb) &&
                    subject == this.subject &&
                    verb == this.verb)
                {
                    return true;
                }
            }
            return false;
        }
        public void SetNewMemory(MemoryInstance newMemory)
        {
            verb = newMemory.verb;
            invertVerb = newMemory.invertVerb;
            subject = newMemory.subject;
            timeStamp = newMemory.timeStamp;
            iterations = newMemory.iterations;
            Edible = newMemory.Edible;
        }
    }
}