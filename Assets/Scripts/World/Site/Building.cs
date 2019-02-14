using rak.creatures;
using System.Collections.Generic;
using UnityEngine;

namespace rak.world
{
    public class Building : Thing
    {
        public enum Building_Type { House }

        public static Dictionary<Thing_Types,int> GetResourcesNeededToCreate(Building.Building_Type type)
        {
            Dictionary<Thing_Types, int> buildingMaterialNeeded = new Dictionary<Thing_Types, int>();
            if (type == Building_Type.House)
            {
                buildingMaterialNeeded.Add(Thing_Types.Wood, 50);
            }
            return buildingMaterialNeeded;
        }

        
        public Building_Type BuildingType { get; private set; }

        private List<Thing> contents;
        private BuildingAnimation buildingAnimation;

        public void Initialize(Building_Type type)
        {
            this.BuildingType = type;
            
            contents = new List<Thing>();
            if (type == Building_Type.House)
            {
                buildingAnimation = new BuildingAnimation(type,transform);
            }
        }

        public void AddContents(Thing thing)
        {
            contents.Add(thing);
        }
    }

}