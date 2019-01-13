using rak.world;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace rak.creatures.memory
{
    public enum Verb { SAW, USED, ATE, SLEPT, WALKED, RAN, MOVEDTO }

    [Serializable]
    public class MemoryInstance
    {
        public Verb verb { get; private set; }
        public bool invertVerb { get; private set; }
        public Thing subject { get; private set; }
        public long timeStamp { get; private set; }
        public int iterations { get; private set; }
        public MemoryInstance(Verb verb,Thing subject,bool invertVerb)
        {
            this.invertVerb = invertVerb;
            this.verb = verb;
            this.subject = subject;
            timeStamp = DateTime.Now.ToBinary();
            iterations = 0;
        }
        public void AddIteration() { iterations++; }
        public bool IsSameAs(Verb verb, Thing subject, bool invertVerb)
        {
            if (subject != null)
            {
                if (invertVerb == this.invertVerb &&
                    subject == this.subject &&
                    verb == this.verb)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsSameAs(MemoryInstance instance)
        {
            return IsSameAs(instance.verb, instance.subject, instance.invertVerb);
        }
    }

    public class Memory
    {
        private List<MemoryInstance> longTermMemory;
        private MemoryInstance[] shortTermMemory;
        private int currentMemoryIndex;
        private const int SHORT_TERM_MEMORY_SIZE = 50;

        public Memory()
        {
            longTermMemory = new List<MemoryInstance>();
            shortTermMemory = new MemoryInstance[SHORT_TERM_MEMORY_SIZE];
            currentMemoryIndex = 0;
        }
        public Thing GetClosestFoodFromMemory(bool filterOutMoveToFailuresFromShortTerm,CONSUMPTION_TYPE cType,
            Vector3 originPosition)
        {
            Thing closest = null;
            float closestDist = float.MaxValue;
            Thing[] food = FoodFromMemory(cType);
            for(int count = 0; count < food.Length; count++)
            {
                // We remember not being able to access this previously //
                if (HasRecentMemoryOf(Verb.MOVEDTO, food[count], true) && filterOutMoveToFailuresFromShortTerm)
                    continue;
                // Object has since been destroyed //
                if (food[count] == null)
                {
                    continue;
                }
                float thisDist = Vector3.Distance(originPosition, food[count].transform.position);
                if(thisDist < closestDist)
                {
                    closestDist = thisDist;
                    closest = food[count];
                }
            }
            return closest;
        }

        private Thing[] FoodFromMemory(CONSUMPTION_TYPE consumptionType)
        {
            List<Thing> memoriesOfFood = new List<Thing>();
            for(int count = 0; count < shortTermMemory.Length; count++)
            {
                MemoryInstance memory = shortTermMemory[count];
                if (memory == null) continue;
                if (memory.verb == Verb.SAW && memory.invertVerb == false && 
                    memory.subject.matchesConsumptionType(consumptionType))
                {
                    memoriesOfFood.Add(memory.subject);
                }
            }
            return memoriesOfFood.ToArray();
        }

        public bool HasRecentMemoryOf(Verb verb,Thing subject,bool invertVerb)
        {
            return isRecentMemory(verb, subject, invertVerb) != null;
        }
        public bool AddMemory(MemoryInstance memory)
        {
            MemoryInstance recentMemory = isRecentMemory(memory);
            if (recentMemory != null)
            {
                recentMemory.AddIteration();
                //Debug.LogWarning("Iterating memory");
                return true;
            }
            if(currentMemoryIndex < shortTermMemory.Length)
            {
                shortTermMemory[currentMemoryIndex] = memory;
                currentMemoryIndex++;
                if (World.ISDEBUGSCENE)
                {
                    StringBuilder builder = new StringBuilder("I will remember I ");
                    if (memory.invertVerb) builder.Append(" NOT ");
                    builder.Append(memory.verb);
                    builder.Append(" " + memory.subject.name);
                    //Debug.LogWarning(builder.ToString());
                }
                return true;
            }
            return false;
        }

        public MemoryInstance[] GetAllMemoriesOf(Thing thing)
        {
            List<MemoryInstance> foundThings = new List<MemoryInstance>();
            foreach(MemoryInstance memory in shortTermMemory)
            {
                if (memory != null && memory.subject == thing)
                    foundThings.Add(memory);
            }
            foreach(MemoryInstance memory in longTermMemory)
            {
                if (memory.subject == thing)
                    foundThings.Add(memory);
            }
            return foundThings.ToArray();
        }
        public bool HasAnyMemoryOfThing(Thing thing)
        {
            return GetAllMemoriesOf(thing).Length > 0;
        }
        public MemoryInstance[] HasAnyMemoriesOf(Verb verb,CONSUMPTION_TYPE consumptionType)
        {
            List<MemoryInstance> memories = new List<MemoryInstance>();
            for(int count = 0; count < shortTermMemory.Length; count++)
            {
                if(shortTermMemory[count] != null)
                {
                    if (shortTermMemory[count].verb == verb &&
                        shortTermMemory[count].subject.matchesConsumptionType(consumptionType))
                    { 
                        memories.Add(shortTermMemory[count]);
                    }
                }
                for(count = 0; count < longTermMemory.Count; count++)
                {
                    if(longTermMemory[count].verb == verb &&
                        longTermMemory[count].subject.matchesConsumptionType(consumptionType))
                    {
                        memories.Add(longTermMemory[count]);
                    }
                }
            }
            return memories.ToArray();
        }
        public MemoryInstance HasAnyMemoryOf(Verb verb,CONSUMPTION_TYPE consumptionType,bool invertVerb)
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count] == null) continue;
                if (shortTermMemory[count].verb == Verb.SAW &&
                    shortTermMemory[count].subject.matchesConsumptionType(consumptionType))
                {
                    return shortTermMemory[count];
                }
            }
            for (int count = 0; count < longTermMemory.Count; count++)
            {
                if(longTermMemory[count].verb == Verb.SAW &&
                    longTermMemory[count].subject.matchesConsumptionType(consumptionType))
                {
                    Debug.LogWarning("I remember food - " + longTermMemory[count].subject.name);
                    return longTermMemory[count];
                }
            }
            return null;
        }
        private MemoryInstance isRecentMemory(Verb verb, Thing subject, bool invertVerb)
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count] != null)
                {
                    if (shortTermMemory[count].IsSameAs(verb,subject,invertVerb))
                    {
                        return shortTermMemory[count];
                    }
                }
            }
            return null;
        }
        private MemoryInstance isRecentMemory(MemoryInstance memory)
        {
            return isRecentMemory(memory.verb, memory.subject, memory.invertVerb);
        }
        private void CopyShortTermToLongTimeAndReset()
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if(shortTermMemory[count] != null)
                {
                    longTermMemory.Add(shortTermMemory[count]);
                }
            }
            currentMemoryIndex = 0;
        }
    }
}
