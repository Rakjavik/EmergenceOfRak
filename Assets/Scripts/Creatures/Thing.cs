using rak.creatures;
using rak.creatures.memory;
using rak.world;
using System;
using UnityEngine;

namespace rak
{


    public class Thing : MonoBehaviour
    {
        #region ENUMS
        public enum BOOL_FILTERS { USEABLE, CONSUMEABLE, USE_LOCATE_TARGET }
        public enum Base_Types { CREATURE, PLANT, NON_ORGANIC }
        public enum Thing_Types {Fruit,Wood,Gnat,House,FruitTree }
        public enum Thing_Produces { NA, Food }

        #endregion

        protected Creature ControlledBy;

        public Tribe owner { get; private set; }
        public string thingName { get; protected set; }
        public Base_Types baseType { get; private set; }
        public Thing_Types thingType { get; private set; }
        public Thing_Produces produces { get; private set; }
        public float age { get; private set; }
        public float maxAge { get; private set; }
        private int weight;
        public float bornAt { get; private set; }
        private bool useable;
        private bool consumeable;
        private bool available;

        private void DestroyThisThing()
        {
            available = false;
            World.CurrentArea.RemoveThingFromWorld(this);
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
                Debug.Log(thingName + " not claimed or not consumeable");
            }
            return false;
        }

        public void initialize(string name)
        {
            this.name = name;
            this.thingName = name;
            // Default to no production //
            produces = Thing_Produces.NA;
            if (name.Equals("fruit"))
            {
                baseType = Base_Types.PLANT;
                thingType = Thing_Types.Fruit;
                weight = 1;
                age = 0;
                maxAge = 60*15;
                bornAt = Time.time;
                available = true;
                if (baseType == Base_Types.PLANT)
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
            else if (name.Equals("FruitTree"))
            {
                baseType = Base_Types.PLANT;
                thingType = Thing_Types.FruitTree;
                produces = Thing_Produces.Food;
                name = "FruitTree";
                weight = 500;
                age = 0;
                maxAge = int.MaxValue;
                bornAt = Time.time;
                available = false;
                consumeable = false;
                useable = false;
            }
            else
            {
                baseType = Base_Types.CREATURE;
                thingType = Thing_Types.Gnat;
                name = "Gnat";
                weight = 1;
                age = 0;
                maxAge = int.MaxValue;
                bornAt = Time.time;
                available = true;
                if (baseType == Base_Types.PLANT)
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
            // Move to top if out of bounds //
            if (transform.position.y < Area.MinimumHeight)
            {
                transform.position = new Vector3(transform.position.x,Area.MaximumHeight,transform.position.z);
            }
            if(age >= maxAge)
            {
                Debug.Log("Death by age - " + name);
                DestroyThisThing();
            }
        }

        public bool match(Base_Types baseType, BOOL_FILTERS[] filters)
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

        public bool match(Base_Types baseType)
        {
            return this.baseType == baseType;
        }

        public bool matchesConsumptionType(CONSUMPTION_TYPE consumptionType)
        {
            if (consumptionType == CONSUMPTION_TYPE.OMNIVORE)
            {
                if (consumeable && available)
                {
                    if (baseType == Base_Types.PLANT)
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
        public void setAvailable(bool available)
        {
            if (available != !this.available) Debug.LogWarning("Set available on thing object called setting the same value");
            this.available = available;
        }
        #endregion
    }
}
