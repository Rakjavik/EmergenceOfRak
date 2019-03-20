using UnityEngine;
using System;
using rak;
using rak.world;

public class RAKTerrainObject : MonoBehaviour, Resource
{
    public ResourceType resourceType { get; private set; }
    public float maxSteepness = -1; // Initialization value
    public float minimiumDistanceBetweenObjects = 100;
    public float minDistFromEdge = 100;
    public float heightOffset = .8563528f;
    public bool freeRotate = false;
    public int numberOfTimesToSearchForLocation = 250;
    public Vector3 sizeOverride = Vector3.zero;
    public string prefabObjectName;

    public void UpdatePropertiesBasedOnName(string prefabObjectName)
    {
        this.prefabObjectName = prefabObjectName;
        switch (prefabObjectName)
        {
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_01:
                maxSteepness = 50;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 0;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_02:
                maxSteepness = 50;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 0;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_03:
                maxSteepness = 50;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 0;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_04:
                maxSteepness = 50;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 0;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_05:
                maxSteepness = 50;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 0;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_BUSH_06:
                maxSteepness = 30;
                minimiumDistanceBetweenObjects = 5;
                minDistFromEdge = 0;
                heightOffset = 5;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                resourceType = ResourceType.Matter;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_HOUSE1:
                maxSteepness = 2;
                minimiumDistanceBetweenObjects = 30;
                minDistFromEdge = 8;
                heightOffset = .8563528f;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 500;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_HOUSE2:
                maxSteepness = 2;
                minimiumDistanceBetweenObjects = 30;
                minDistFromEdge = 8;
                heightOffset = .8563528f;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 500;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_TREE01:
                maxSteepness = 40;
                minimiumDistanceBetweenObjects = 4;
                minDistFromEdge = 0;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                resourceType = ResourceType.Matter;
                sizeOverride = new Vector3(2, 15, 2);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_TREE02:
                maxSteepness = 40;
                minimiumDistanceBetweenObjects = 4;
                minDistFromEdge = 0;
                heightOffset = 2;
                freeRotate = false;
                resourceType = ResourceType.Matter;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(2, 15, 2);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_TREE03:
                maxSteepness = 40;
                minimiumDistanceBetweenObjects = 4;
                minDistFromEdge = 0;
                heightOffset = 2;
                resourceType = ResourceType.Matter;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(2, 15, 2);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_TREE04_3PACK:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 10;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(5, 15, 5);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_1:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(5, 15, 5);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_2:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 10;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(5, 15, 5);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_3:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 10;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(5, 15, 5);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_4:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 10;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = new Vector3(5, 15, 5);
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_1:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 3;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_2:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 3;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_3:
                maxSteepness = 15;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 2;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_4:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 3;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                break;
            case RAKUtilities.NON_TERRAIN_OBJECT_FRUIT_TREE:
                maxSteepness = 35;
                minimiumDistanceBetweenObjects = 20;
                minDistFromEdge = 0;
                resourceType = ResourceType.Matter;
                heightOffset = 3;
                freeRotate = false;
                numberOfTimesToSearchForLocation = 250;
                sizeOverride = Vector3.zero;
                break;
        }
    }

    public string getPrefabObjectName()
    {
        return prefabObjectName;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }
}
[Serializable]
public class RAKTerrainObjectSaveData
{
    public int prefabObjectIndex;
    public RAKTerrainMaster.RAKTerrainSavedData.RAKVector3 position;
    public RAKTerrainMaster.RAKTerrainSavedData.RAKVector3 rotationEulers;

    public static RAKTerrainObjectSaveData createSaveData(RAKTerrainObject terrainObject)
    {
        RAKTerrainObjectSaveData saveData = new RAKTerrainObjectSaveData();
        saveData.prefabObjectIndex = RAKUtilities.GetNonTerrainObjectIndex(terrainObject.prefabObjectName);
        Vector3 position = terrainObject.transform.position;
        saveData.position = new RAKTerrainMaster.RAKTerrainSavedData.RAKVector3(position);
        Quaternion rotation = terrainObject.transform.rotation;
        saveData.rotationEulers = new RAKTerrainMaster.RAKTerrainSavedData.RAKVector3(rotation);
        return saveData;
    }
}
