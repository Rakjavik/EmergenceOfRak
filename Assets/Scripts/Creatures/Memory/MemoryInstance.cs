using System;

namespace rak.creatures.memory
{
    [Serializable]
    public struct MemoryInstance
    {
        public Verb verb { get; private set; }
        public bool invertVerb { get; private set; }
        public BlittableThing subject { get; private set; }
        public long timeStamp { get; private set; }
        public int iterations { get; private set; }
        private MemoryInstance(Verb verb,BlittableThing subject,bool invertVerb)
        {
            this.invertVerb = invertVerb;
            this.verb = verb;
            this.subject = subject;
            timeStamp = DateTime.Now.ToBinary();
            iterations = 0;
        }
        public static MemoryInstance GetNewEmptyMemory()
        {
            return new MemoryInstance(Verb.NA, BlittableThing.GetNewEmptyThing(), false);
        }
        public void ReplaceMemory(Verb verb, BlittableThing subject, bool invertVerb)
        {
            this.verb = verb;
            this.subject = subject;
            this.invertVerb = invertVerb;
            timeStamp = DateTime.Now.ToBinary();
            iterations = 0;
        }

        public void AddIteration() { iterations++; }
        public bool IsSameAs(Verb verb, BlittableThing subject, bool invertVerb)
        {
            if (!subject.IsEmpty())
            {
                if (invertVerb == this.invertVerb &&
                    subject.GetGuid() == this.subject.GetGuid() &&
                    verb == this.verb)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
