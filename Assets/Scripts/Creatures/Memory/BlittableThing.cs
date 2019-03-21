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
        private float age;
        private float bornAt;
        public Vector3 position;

        private BlittableThing(Thing thing)
        {
            //Debug.LogWarning("Constructing " + thing.name);
            if (thing == null || thing.guid == Guid.Empty)
            {
                baseType = Thing.Base_Types.NA;
                age = -1;
                bornAt = -1;
                position = Vector3.zero;
                guid = Guid.Empty;
            }
            else
            {
                guid = thing.guid;
                baseType = thing.baseType;
                age = thing.age;
                bornAt = thing.bornAt;
                position = thing.transform.position;
            }
        }
        public static BlittableThing GetNewEmptyThing()
        {
            return new BlittableThing(null);
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

        public bool IsEmpty()
        {
            if(guid == Guid.Empty)
                return true;

            return false;
        }
    }
}
