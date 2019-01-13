using rak.creatures;
using rak.creatures.memory;
using System;
using UnityEngine;

namespace rak
{


    public class Thing : MonoBehaviour
    {
        #region ENUMS
        public enum BOOL_FILTERS { USEABLE, CONSUMEABLE, USE_LOCATE_TARGET }
        public enum BASE_TYPES { CREATURE, PLANT, NON_ORGANIC }
        #endregion

        protected Creature ControlledBy;

        private string name;
        private BASE_TYPES baseType;
        private float age;
        private int weight;
        private float bornAt;
        private bool useable;
        private bool consumeable;
        private bool available;

        private void DestroyThisThing()
        {
            world.Area.removeThingFromWorld(this);
        }

        public bool beConsumed(Creature consumer)
        {
            if (consumeable && !available)
            {
                Debug.Log("Destroying - " + gameObject.name);
                DestroyThisThing();
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
                available = false;
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
            // Destory if out of bounds //
            if (transform.position.y < -100)
            {
                DestroyThisThing();
            }
        }

        public bool match(BASE_TYPES baseType, BOOL_FILTERS[] filters)
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
            foreach (BOOL_FILTERS filter in filters)
            {
                if (filter == BOOL_FILTERS.CONSUMEABLE)
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
            if (consumptionType == CONSUMPTION_TYPE.OMNIVORE)
            {
                if (consumeable && available)
                {
                    if (baseType == BASE_TYPES.PLANT)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool RequestControl(Creature requestor)
        {
            return true;
        }
        public virtual Rigidbody RequestRigidBodyAccess(Creature requestor)
        {
            Rigidbody body = GetComponent<Rigidbody>();
            return body;
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
