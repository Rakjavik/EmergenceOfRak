using rak.creatures;
using rak.creatures.memory;
using rak.ecs.AgentComponents;
using rak.world;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace rak
{
    public class Thing : MonoBehaviour
    {
        public void AddComponents(Entity entity)
        {
            Area.EntityManager.AddComponentData(entity, new Age { Value = 0,MaxAge=10 });
            Area.EntityManager.AddComponentData(entity, new Enabled { Value = 1 });
        }
        
        #region ENUMS
        public enum BOOL_FILTERS { USEABLE, CONSUMEABLE, USE_LOCATE_TARGET }
        public enum Base_Types { CREATURE, PLANT, NON_ORGANIC, NA }
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
        public float bornAt { get; private set; }
        public Guid guid { get; private set; }
        public int entityIndex { get; set; }

        private BlittableThing blittableThing = BlittableThing.GetNewEmptyThing();
        public BlittableThing GetBlittableThing()
        {
            blittableThing.RefreshValue(this);
            return blittableThing;
        }
        protected Rigidbody rb;
        private ThingAgent thingAgent;
        private int weight;
        private bool useable;
        private bool consumeable;
        private bool available;

        private void DestroyThisThing()
        {
            available = false;
            world.World.CurrentArea.RemoveThingFromWorld(this);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            available = false;
            if(this is Creature)
            {
                Creature creature = (Creature)this;
                creature.DeactivateAllParts();
            }
        }

        public bool beConsumed(Creature consumer)
        {
            if (consumeable && !available)
            {
                Debug.Log("Destroying - " + gameObject.name);
                DestroyThisThing();
                consumer.AddMemory(Verb.ATE,this,false);
                consumer.AddMemory(Verb.SAW,this,true);
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
            rb = null;
            guid = Guid.NewGuid();
            this.thingName = name + "-" + guid.ToString().Substring(0,5);
            // Default to no production //
            produces = Thing_Produces.NA;
            if (name.Equals("fruit"))
            {
                rb = GetComponent<Rigidbody>();
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
            else if (name.Equals(RAKUtilities.NON_TERRAIN_OBJECT_FRUIT_TREE))
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
                thingAgent = new ThingAgent(this);
            }
            else if (name.Equals(RAKUtilities.NON_TERRAIN_OBJECT_BUSH_01))
            {
                baseType = Base_Types.PLANT;
                thingType = Thing_Types.FruitTree;
                produces = Thing_Produces.Food;
                name = "FruitBush";
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
            // Age should be handled by ECS now //
            Debug.LogWarning("Age - " + age);
            if (rb != null && !rb.IsSleeping())
            {
                if (transform.position.y < Area.MinimumHeight)
                    transform.position = new 
                        Vector3(transform.position.x, Area.MaximumHeight, transform.position.z);
            }
            if (age >= maxAge)
            {
                Debug.Log("Death by age - " + name);
                DestroyThisThing();
            }
            if(thingAgent != null)
            {
                thingAgent.ManualUpdate(delta);
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
