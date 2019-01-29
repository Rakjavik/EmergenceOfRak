using rak.creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace rak
{
    public class Inventory
    {
        public List<Thing> things;
        private Thing owner;
        private int currentWeight;

        public Inventory(Thing owner)
        {
            this.owner = owner;
            things = new List<Thing>();
        }

        public bool addThing(Thing thing)
        {
            int maxWeight = getMaxWeight();
            if (maxWeight > -1 && thing.getWeight()+currentWeight < maxWeight)
            {
                things.Add(thing);
                currentWeight += thing.getWeight();
                return true;
            }
            else
            {
                Debug.Log(owner.thingName + " cannot pick up " + thing.thingName + " because it weighs too much");
            }
            return false;
        }
        public bool removeThing(Thing thing)
        {
            bool success = things.Remove(thing);
            if (success) currentWeight -= thing.getWeight();
            return success;
        }
        private int getMaxWeight()
        {
            if(typeof(Creature) == owner.GetType())
            {
                Creature ownerCreature = (Creature) owner;
                return ownerCreature.getCreatureStats().getMaxWeight();
            }
            return -1;
        }
    }
}
