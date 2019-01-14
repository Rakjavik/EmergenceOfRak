using rak.world;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rak
{
    public class FruitTree : Thing
    {
        private float spawnsThingEvery = 60;
        private float timeSincelastSpawned { get; set; }
        private List<GameObject> fruitInstances { get; set; }

        public FruitTree()
        {
            timeSincelastSpawned = -1;
            fruitInstances = new List<GameObject>();
        }

        public void Update()
        {
            if(timeSincelastSpawned == -1)
                timeSincelastSpawned = Random.Range(0, spawnsThingEvery);
            timeSincelastSpawned += Time.deltaTime;
            if(timeSincelastSpawned >= spawnsThingEvery)
            {
                Vector3 newPosition = new Vector3(Random.Range(-5,5), 10, Random.Range(-5, 5));
                newPosition += transform.position;
                GameObject fruit = World.CurrentArea.addThingToWorld("fruit", newPosition, false);
                fruitInstances.Add(fruit);
                timeSincelastSpawned = 0;
            }
        }
    }
}
