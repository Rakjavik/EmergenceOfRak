using System;

namespace rak.creatures.memory
{
    [Serializable]
    public class MemoryInstance
    {
        public Verb verb { get; private set; }
        public bool invertVerb { get; private set; }
        public HistoricalThing subject { get; private set; }
        public long timeStamp { get; private set; }
        public int iterations { get; private set; }
        public MemoryInstance(Verb verb,Thing subject,bool invertVerb)
        {
            this.invertVerb = invertVerb;
            this.verb = verb;
            this.subject = new HistoricalThing(subject);
            timeStamp = DateTime.Now.ToBinary();
            iterations = 0;
        }
        public void AddIteration() { iterations++; }
        public bool IsSameAs(Verb verb, Thing subject, bool invertVerb)
        {
            if (subject != null)
            {
                if (invertVerb == this.invertVerb &&
                    subject == this.subject.GetThing() &&
                    verb == this.verb)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsSameAs(MemoryInstance instance)
        {
            return IsSameAs(instance.verb, instance.subject.GetThing(), instance.invertVerb);
        }
    }
}
