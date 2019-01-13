using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rak
{
    public class FruitTree : Thing
    {
        private float spawnsThingEvery = 5;
        private float timeSincelastSpawned { get; set; }
        private GameObject fruitPrefab = null;
        private List<GameObject> fruitInstances { get; set; }

        public FruitTree()
        {
            timeSincelastSpawned = 0;
            fruitInstances = new List<GameObject>();
        }

        public void Update()
        {
            timeSincelastSpawned += Time.deltaTime;
            if(timeSincelastSpawned >= spawnsThingEvery)
            {
                if (fruitPrefab == null)
                    fruitPrefab = RAKUtilities.getThingPrefab("fruit");
                GameObject fruit = Instantiate(fruitPrefab);
                fruit.transform.SetParent(transform);
                Vector3 newPosition = new Vector3(transform.position.x, 10, transform.position.z);
                fruit.transform.position = newPosition;
                fruit.GetComponent<Thing>().initialize("fruit");
                timeSincelastSpawned = 0;
            }
        }
    }
}
