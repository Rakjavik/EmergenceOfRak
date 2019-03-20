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
    public struct HistoricalThing
    {
        private Guid guid;
        private string name;
        private Thing.Base_Types baseType;
        private float age;
        private float bornAt;
        [NonSerialized]
        private Thing thing;

        public HistoricalThing(Thing thing)
        {
            guid = Guid.NewGuid();
            name = thing.thingName;
            baseType = thing.baseType;
            age = thing.age;
            bornAt = thing.bornAt;
            this.thing = thing;
        }
        public Thing GetThing() { return thing; }
    }

    public class Memory
    {
        private List<MemoryInstance> longTermMemory;
        private MemoryInstance[] shortTermMemory;
        private int currentMemoryIndex;
        private const int SHORT_TERM_MEMORY_SIZE = 100;

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
            Thing[] food = GetFoodFromMemory(cType);
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
        public Thing GetClosestFoodProducerFromMemory(Vector3 origin, Thing[] exclusions)
        {
            return getClosestFoodProducerFromMemory(origin, exclusions,0);
        }
        public Thing GetClosestFoodProducerFromMemory(Vector3 origin, float discludeDistanceLessThan)
        {
            return getClosestFoodProducerFromMemory(origin, null, discludeDistanceLessThan);
        }
        public Thing[] GetKnownConsumeableProducers()
        {
            return getFoodProducersFromMemory();
        }
        public Thing GetClosestFoodProducerFromMemory(Vector3 origin)
        {
            return getClosestFoodProducerFromMemory(origin, null,0);
        }
        public MemoryInstance[] GetShortTermMemory()
        {
            return shortTermMemory;
        }
        private Thing getClosestFoodProducerFromMemory(Vector3 origin,Thing[] exclusions,float discludeDistanceLessThan)
        {
            Thing closest = null;
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                MemoryInstance memory = shortTermMemory[count];
                if (memory == null) continue;
                if (memory.verb == Verb.SAW && memory.invertVerb == false &&
                    memory.subject.GetThing() != null && memory.subject.GetThing()
                    .produces == Thing.Thing_Produces.Food)
                {
                    Thing currentThing = memory.subject.GetThing();
                    float currentDistance = Vector3.Distance(origin, currentThing.transform.position);
                    float closestDistance = float.MaxValue;
                    if(closest != null)
                        closestDistance = Vector3.Distance(origin, closest.transform.position);
                    if (currentDistance < closestDistance)
                    {
                        if(exclusions == null && discludeDistanceLessThan == 0)
                            closest = currentThing;
                        else
                        {
                            // Discard if an exclusion //
                            bool exclude = false;
                            if (exclusions != null)
                            {
                                foreach (Thing exclusion in exclusions)
                                {
                                    if (exclusion == currentThing)
                                    {
                                        exclude = true;
                                        break;
                                    }
                                }
                            }
                            // Discard if below min distance //
                            if(discludeDistanceLessThan > 0)
                            {
                                if (currentDistance <= discludeDistanceLessThan)
                                {
                                    exclude = true;
                                }
                            }
                            if (!exclude)
                                closest = currentThing;
                        }
                    }
                    
                }
            }
            return closest;
        }

        private Thing[] GetFoodFromMemory(CONSUMPTION_TYPE consumptionType)
        {
            List<Thing> memoriesOfFood = new List<Thing>();
            for(int count = 0; count < shortTermMemory.Length; count++)
            {
                MemoryInstance memory = shortTermMemory[count];
                if (memory == null) continue;
                if (memory.verb == Verb.SAW && memory.invertVerb == false && 
                    memory.subject.GetThing() != null && memory.subject.GetThing()
                    .matchesConsumptionType(consumptionType))
                {
                    memoriesOfFood.Add(memory.subject.GetThing());
                }
            }
            return memoriesOfFood.ToArray();
        }
        private Thing[] getFoodProducersFromMemory()
        {
            List<Thing> producers = new List<Thing>();
            for(int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count] == null) continue;
                Thing currentThing = shortTermMemory[count].subject.GetThing();
                if (currentThing != null && currentThing.produces == Thing.Thing_Produces.Food)
                {
                    producers.Add(shortTermMemory[count].subject.GetThing());
                }
            }
            return producers.ToArray();
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
            if (currentMemoryIndex + 1 == shortTermMemory.Length)
                currentMemoryIndex = 0;
            if(currentMemoryIndex < shortTermMemory.Length)
            {
                shortTermMemory[currentMemoryIndex] = memory;
                currentMemoryIndex++;
                if (World.ISDEBUGSCENE)
                {
                    StringBuilder builder = new StringBuilder("I will remember I ");
                    if (memory.invertVerb) builder.Append(" NOT ");
                    builder.Append(memory.verb);
                    builder.Append(" " + memory.subject.GetThing().thingName);
                    //Debug.LogWarning(builder.ToString());
                    DebugMenu.AppendLine(builder.ToString());
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
                if (memory != null && memory.subject.GetThing() == thing)
                    foundThings.Add(memory);
            }
            foreach(MemoryInstance memory in longTermMemory)
            {
                if (memory.subject.GetThing() == thing)
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
                        shortTermMemory[count].subject.GetThing().matchesConsumptionType(consumptionType))
                    { 
                        memories.Add(shortTermMemory[count]);
                    }
                }
                for(count = 0; count < longTermMemory.Count; count++)
                {
                    if(longTermMemory[count].verb == verb &&
                        longTermMemory[count].subject.GetThing().matchesConsumptionType(consumptionType))
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
                    shortTermMemory[count].subject.GetThing().matchesConsumptionType(consumptionType))
                {
                    return shortTermMemory[count];
                }
            }
            for (int count = 0; count < longTermMemory.Count; count++)
            {
                if(longTermMemory[count].verb == Verb.SAW &&
                    longTermMemory[count].subject.GetThing().matchesConsumptionType(consumptionType))
                {
                    Debug.LogWarning("I remember food - " + longTermMemory[count].subject.GetThing().thingName);
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
            return isRecentMemory(memory.verb, memory.subject.GetThing(), memory.invertVerb);
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
