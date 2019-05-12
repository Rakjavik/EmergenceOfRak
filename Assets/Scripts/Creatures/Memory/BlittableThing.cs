using rak.world;
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rak.creatures.memory
{
    [Serializable]
    public struct BlittableThing
    {
        private Entity entity;
        public Thing.Base_Types BaseType;
        public Thing.Thing_Produces produces;
        private float age;
        private float bornAt;
        public float3 position;
        public float Mass;

        public BlittableThing(Thing.Base_Types baseType,Entity entity,float age,float bornAt, float3 position,
            Thing.Thing_Produces produces,float mass)
        {
            if (entity.Equals(Entity.Null))
            {
                this.BaseType = Thing.Base_Types.NA;
                this.age = -1;
                this.bornAt = -1;
                this.position = float3.zero;
                this.entity = Entity.Null;
                this.produces = Thing.Thing_Produces.NA;
                this.Mass = 0;
            }
            else
            {
                this.entity = entity;
                this.BaseType = baseType;
                this.age = age;
                this.bornAt = bornAt;
                this.position = position;
                this.produces = produces;
                this.Mass = mass;
            }
        }
        
        public static BlittableThing GetNewEmptyThing()
        {
            return new BlittableThing(Thing.Base_Types.NA,Entity.Null,-1,-1, float3.zero,Thing.Thing_Produces.NA,0);
        }
        public Thing GetThing()
        {
            return Area.GetThingByEntity(entity);
        }
        public Entity GetEntity()
        {
            return entity;
        }
        public void RefreshValue(Thing thing)
        {
            entity = thing.ThingEntity;
            BaseType = thing.baseType;
            age = thing.age;
            bornAt = thing.bornAt;
            position = thing.transform.position;
            Mass = thing.getWeight();
        }
        public void SetToEmpty()
        {
            entity = Entity.Null;
        }
        public bool IsEmpty()
        {
            if(entity.Equals(Entity.Null))
                return true;

            return false;
        }
    }

    public struct ObservableThing
    {
        public float3 position;
        public int index;
        public Entity entity;
        public Thing.Base_Types BaseType;
        public float Mass;
    }
}
