using rak.world;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace rak.creatures.memory
{
    public enum Verb { SAW, USED, ATE, SLEPT, WALKED, RAN, MOVEDTO, NA }

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
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                shortTermMemory[count] = MemoryInstance.GetNewEmptyMemory();
            }
            currentMemoryIndex = 0;
        }
        public Thing GetClosestFoodFromMemory(bool filterOutMoveToFailuresFromShortTerm, ConsumptionType cType,
            Vector3 originPosition)
        {
            Thing closest = null;
            float closestDist = float.MaxValue;
            Thing[] food = GetFoodFromMemory(cType);
            for (int count = 0; count < food.Length; count++)
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
                if (thisDist < closestDist)
                {
                    closestDist = thisDist;
                    closest = food[count];
                }
            }
            return closest;
        }
        public Thing GetClosestFoodProducerFromMemory(Vector3 origin, Thing[] exclusions)
        {
            return getClosestFoodProducerFromMemory(origin, exclusions, 0);
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
            return getClosestFoodProducerFromMemory(origin, null, 0);
        }
        public MemoryInstance[] GetShortTermMemory()
        {
            return shortTermMemory;
        }
        private Thing getClosestFoodProducerFromMemory(Vector3 origin, Thing[] exclusions, float discludeDistanceLessThan)
        {
            Thing closest = null;
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                MemoryInstance memory = shortTermMemory[count];
                bool currentMemoryinvertVerb = memory.GetInvertVerb();
                if (memory.Verb == Verb.NA) continue;
                if (memory.Verb == Verb.SAW && currentMemoryinvertVerb == false &&
                    memory.Subject != System.Guid.Empty && Area.GetThingByGUID(memory.Subject).produces == Thing.Thing_Produces.Food)
                {
                    Thing currentThing = Area.GetThingByGUID(memory.Subject);
                    float currentDistance = Vector3.Distance(origin, currentThing.transform.position);
                    float closestDistance = float.MaxValue;
                    if (closest != null)
                        closestDistance = Vector3.Distance(origin, closest.transform.position);
                    if (currentDistance < closestDistance)
                    {
                        if (exclusions == null && discludeDistanceLessThan == 0)
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
                            if (discludeDistanceLessThan > 0)
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

        private Thing[] GetFoodFromMemory(ConsumptionType consumptionType)
        {
            List<Thing> memoriesOfFood = new List<Thing>();
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                MemoryInstance memory = shortTermMemory[count];
                bool currentMemoryInvertVerb = memory.GetInvertVerb();
                if (memory.Verb == Verb.NA) continue;
                if (memory.Verb == Verb.SAW && currentMemoryInvertVerb == false &&
                    memory.Subject != System.Guid.Empty && Area.GetThingByGUID(memory.Subject)
                    .matchesConsumptionType(consumptionType))
                {
                    memoriesOfFood.Add(Area.GetThingByGUID(memory.Subject));
                }
            }
            return memoriesOfFood.ToArray();
        }
        private Thing[] getFoodProducersFromMemory()
        {
            List<Thing> producers = new List<Thing>();
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count].Verb == Verb.NA) continue;
                Thing currentThing = Area.GetThingByGUID(shortTermMemory[count].Subject);
                if (currentThing != null && currentThing.produces == Thing.Thing_Produces.Food)
                {
                    producers.Add(Area.GetThingByGUID(shortTermMemory[count].Subject));
                }
            }
            return producers.ToArray();
        }
        public bool HasRecentMemoryOf(Verb verb, Thing subject, bool invertVerb)
        {
            return isRecentMemory(verb, subject.GetBlittableThing(), invertVerb).Verb != Verb.NA;
        }
        public bool AddMemory(MemoryInstance memory)
        {
            Thing memorySubject = Area.GetThingByGUID(memory.Subject);
            if (memorySubject == null)
            {
                return false;
            }
            return AddMemory(memory.Verb, memorySubject.GetBlittableThing(), memory.GetInvertVerb());
        }
        public bool AddMemory(Verb verb, BlittableThing subject, bool invertVerb)
        {
            MemoryInstance recentSubjectMemory = isRecentMemory(verb, subject, invertVerb);
            if (!recentSubjectMemory.IsEmpty())
            {
                recentSubjectMemory.AddIteration();
                return true;
            }
            if (currentMemoryIndex + 1 == shortTermMemory.Length)
                currentMemoryIndex = 0;
            if (currentMemoryIndex < shortTermMemory.Length)
            {
                if (subject.IsEmpty())
                {
                    Debug.Break();
                    Debug.Log("Null subject to add memory " + verb.ToString());
                }
                shortTermMemory[currentMemoryIndex].ReplaceMemory(verb, subject.GetGuid(), invertVerb);
                currentMemoryIndex++;
                return true;
            }
            return false;
        }
        /*public bool AddMemory(MemoryInstance memory)
        {
            MemoryInstance recentMemory = isRecentMemory(memory.verb,memory.subject.GetThing(),memory.invertVerb);
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
                    if(memory.subject.GetThing() != null)
                        builder.Append(" " + memory.subject.GetThing().thingName);
                    //Debug.LogWarning(builder.ToString());
                    DebugMenu.AppendLine(builder.ToString());
                }
                return true;
            }
            return false;
        }*/

        public MemoryInstance[] GetAllMemoriesOf(Thing thing)
        {
            List<MemoryInstance> foundThings = new List<MemoryInstance>();
            foreach (MemoryInstance memory in shortTermMemory)
            {
                if (!memory.IsEmpty() && memory.Subject.Equals(thing.guid))
                    foundThings.Add(memory);
            }
            foreach (MemoryInstance memory in longTermMemory)
            {
                if (memory.Subject.Equals(thing.guid))
                    foundThings.Add(memory);
            }
            return foundThings.ToArray();
        }
        public bool HasAnyMemoryOfThing(Thing thing)
        {
            return GetAllMemoriesOf(thing).Length > 0;
        }
        public MemoryInstance[] HasAnyMemoriesOf(Verb verb, ConsumptionType consumptionType)
        {
            List<MemoryInstance> memories = new List<MemoryInstance>();
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count].Verb != Verb.NA)
                {
                    if (shortTermMemory[count].Verb == verb &&
                        Area.GetThingByGUID(shortTermMemory[count].Subject)
                        .matchesConsumptionType(consumptionType))
                    {
                        memories.Add(shortTermMemory[count]);
                    }
                }
                for (count = 0; count < longTermMemory.Count; count++)
                {
                    if (longTermMemory[count].Verb == verb &&
                        Area.GetThingByGUID(longTermMemory[count].Subject)
                        .matchesConsumptionType(consumptionType))
                    {
                        memories.Add(longTermMemory[count]);
                    }
                }
            }
            return memories.ToArray();
        }
        public MemoryInstance HasAnyMemoryOf(Verb verb, ConsumptionType consumptionType, bool invertVerb)
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count].Verb == Verb.NA) continue;
                if (shortTermMemory[count].Verb == Verb.SAW &&
                    Area.GetThingByGUID(shortTermMemory[count].Subject)
                    .matchesConsumptionType(consumptionType))
                {
                    return shortTermMemory[count];
                }
            }
            for (int count = 0; count < longTermMemory.Count; count++)
            {
                if (longTermMemory[count].Verb == Verb.SAW &&
                    Area.GetThingByGUID(longTermMemory[count].Subject)
                    .matchesConsumptionType(consumptionType))
                {
                    return longTermMemory[count];
                }
            }
            return MemoryInstance.GetNewEmptyMemory();
        }
        private MemoryInstance isRecentMemory(Verb verb, BlittableThing subject, bool invertVerb)
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count].Verb != Verb.NA)
                {
                    if (shortTermMemory[count].IsSameAs(verb, subject.GetGuid(), invertVerb))
                    {
                        return shortTermMemory[count];
                    }
                }
            }
            return MemoryInstance.GetNewEmptyMemory();
        }
        private void CopyShortTermToLongTimeAndReset()
        {
            for (int count = 0; count < shortTermMemory.Length; count++)
            {
                if (shortTermMemory[count].Verb != Verb.NA)
                {
                    longTermMemory.Add(shortTermMemory[count]);
                }
            }
            currentMemoryIndex = 0;
        }
    }
}