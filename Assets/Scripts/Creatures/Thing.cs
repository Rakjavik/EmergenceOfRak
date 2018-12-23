using rak.creatures;
using rak.creatures.memory;
using System;
using UnityEngine;

namespace rak
{
    public class Thing : MonoBehaviour
    {
        #region ENUMS
        public enum BOOL_FILTERS { USEABLE,CONSUMEABLE,USE_LOCATE_TARGET }
        public enum BASE_TYPES { CREATURE,PLANT, NON_ORGANIC }
        #endregion

        
        private string name;
        private BASE_TYPES baseType;
        private float age;
        private int weight;
        private float bornAt;
        private bool useable;
        private bool consumeable;
        private bool available;

        public bool beConsumed(Creature consumer)
        {
            if (consumeable && !available)
            {
                Debug.Log("Destroying - " + gameObject.name);
                Destroy(this.gameObject);
                MemoryInstance eating = new MemoryInstance(Verb.ATE, this, false);
                MemoryInstance itemGone = new MemoryInstance(Verb.SAW, this, true);
                consumer.AddMemory(eating);
                consumer.AddMemory(itemGone);
                return true;
            }
            else
            {
                Debug.Log(name + " not claimed or not consumeable");
            }
            return false;
        }

        public void initialize(string name)
        {
            this.name = name;
            if (name.Equals("fruit"))
            {
                baseType = BASE_TYPES.PLANT;
                weight = 1;
                age = 0;
                bornAt = Time.time;
                available = true;
                if (baseType == BASE_TYPES.PLANT)
                {
                    consumeable = true;
                    useable = false;
                }
                else
                {
                    consumeable = false;
                    useable = false;
                }
            }
            else
            {
                baseType = BASE_TYPES.CREATURE;
                name = "Gnat";
                weight = 1;
                age = 0;
                bornAt = Time.time;
                available = true;
                if (baseType == BASE_TYPES.PLANT)
                {
                    consumeable = true;
                    useable = false;
                }
                else
                {
                    consumeable = false;
                    useable = false;
                }
            }
        }

        public void ManualUpdate(float delta)
        {
            age += delta;
        }

        public bool match(BASE_TYPES baseType,BOOL_FILTERS[] filters)
        {
            if (this.baseType == baseType)
            {
                return match(filters);
            }
            else
            {
                return false;
            }
        }

        public bool match(BOOL_FILTERS[] filters)
        {
            foreach(BOOL_FILTERS filter in filters)
            {
                if(filter == BOOL_FILTERS.CONSUMEABLE)
                {
                    if (!consumeable) return false;
                }
                else if (filter == BOOL_FILTERS.USEABLE)
                {
                    if (!useable) return false;
                }
            }
            return true;
        }

        public bool match(BASE_TYPES baseType)
        {
            return this.baseType == baseType;
        }

        public bool matchesConsumptionType(CONSUMPTION_TYPE consumptionType)
        {
            if(consumptionType == CONSUMPTION_TYPE.OMNIVORE)
            {
                if(consumeable && available)
                {
                    if(baseType == BASE_TYPES.PLANT)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region GETTERS/SETTERS
        public void MakeAvailable(Creature requestor)
        {
            available = true;
        }
        public void MakeUnavailable() { available = false; }
        public int getWeight() { return weight; }
        public void setAvailable(bool available) {
            if (available != !this.available) Debug.LogWarning("Set available on thing object called setting the same value");
            this.available = available; }
        #endregion
    }
}
