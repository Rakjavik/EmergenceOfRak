using System;

namespace rak.creatures.memory
{
    [Serializable]
    public struct MemoryInstance
    {
        public Verb verb { get; private set; }
        private byte invertVerb { get; set; }
        public Guid subject { get; private set; }
        public float timeStamp { get; private set; }
        public int iterations { get; private set; }
        public MemoryInstance(Verb verb, Guid subject, bool invertVerb, float timestamp)
        {
            if (invertVerb)
                this.invertVerb = 1;
            else
                this.invertVerb = 0;
            this.verb = verb;
            this.subject = subject;
            this.timeStamp = timestamp;
            iterations = 0;
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
            return new MemoryInstance(Verb.NA, Guid.Empty, false,0);
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
    }
}