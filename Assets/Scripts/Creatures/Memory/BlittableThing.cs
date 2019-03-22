using rak.world;
using System;
using UnityEngine;

namespace rak.creatures.memory
{
    [Serializable]
    public struct BlittableThing
    {
        private Guid guid;
        private Thing.Base_Types baseType;
        public Thing.Thing_Produces produces;
        private float age;
        private float bornAt;
        public Vector3 position;

        public BlittableThing(Thing.Base_Types baseType,Guid guid,float age,float bornAt,Vector3 position,
            Thing.Thing_Produces produces)
        {
            if (guid.Equals(Guid.Empty))
            {
                this.baseType = Thing.Base_Types.NA;
                this.age = -1;
                this.bornAt = -1;
                this.position = Vector3.zero;
                this.guid = Guid.Empty;
                this.produces = Thing.Thing_Produces.NA;
            }
            else
            {
                this.guid = guid;
                this.baseType = baseType;
                this.age = age;
                this.bornAt = bornAt;
                this.position = position;
                this.produces = produces;
            }
        }
        
        public static BlittableThing GetNewEmptyThing()
        {
            return new BlittableThing(Thing.Base_Types.NA,Guid.Empty,-1,-1,Vector3.zero,Thing.Thing_Produces.NA);
        }
        public Thing GetThing()
        {
            return Area.GetThingByGUID(guid);
        }
        public Guid GetGuid()
        {
            return guid;
        }
        public void RefreshValue(Thing thing)
        {
            guid = thing.guid;
            baseType = thing.baseType;
            age = thing.age;
            bornAt = thing.bornAt;
            position = thing.transform.position;
        }
        public void SetToEmpty()
        {
            guid = Guid.Empty;
        }
        public bool IsEmpty()
        {
            if(guid.Equals(Guid.Empty))
                return true;

            return false;
        }
    }
}
