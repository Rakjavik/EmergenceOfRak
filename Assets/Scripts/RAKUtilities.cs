using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace rak
{
    public abstract class RAKUtilities
    {
        public const string NON_TERRAIN_OBJECT_HOUSE1 = "Suburb House Grey";
        public const string NON_TERRAIN_OBJECT_HOUSE2 = "Suburb House Yellow";
        public const string NON_TERRAIN_OBJECT_TREE01 = "tree01";
        public const string NON_TERRAIN_OBJECT_TREE02 = "tree02";
        public const string NON_TERRAIN_OBJECT_TREE03 = "tree03";
        public const string NON_TERRAIN_OBJECT_TREE04_3PACK = "tree043pk";
        public const string NON_TERRAIN_OBJECT_BUSH_01 = "bush01";
        public const string NON_TERRAIN_OBJECT_BUSH_02 = "bush02";
        public const string NON_TERRAIN_OBJECT_BUSH_03 = "bush03";
        public const string NON_TERRAIN_OBJECT_BUSH_04 = "bush04";
        public const string NON_TERRAIN_OBJECT_BUSH_05 = "bush05";
        public const string NON_TERRAIN_OBJECT_BUSH_06 = "bush06";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_TREE_1 = "Tree-1-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_TREE_2 = "Tree-2-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_TREE_3 = "Tree-3-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_TREE_4 = "Tree-4-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_BUSH_1 = "Bush-1-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_BUSH_2 = "Bush-2-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_BUSH_3 = "Bush-3-Green";
        public const string NON_TERRAIN_OBJECT_LOW_POLY_BUSH_4 = "Bush-4-Green";
        public const string NON_TERRAIN_OBJECT_FRUIT_TREE = "FruitTree";
        public const string AUDIO_CLIP_RAIN_LIGHT = "rain_light";
        public const string AUDIO_CLIP_WIND_MEDIUM = "wind_normal1";
        public const string MATERIAL_SKYBOX_FOREST = "forest";
        public const string MATERIAL_SKYBOX_MOSSY_MOUNTAIN = "mossymountains";
        public const string MATERIAL_SKYBOX_SUNSET = "sunset";
        public const string MATERIAL_SKYBOX_SUNRISE = "sunrise";
        public static string[] nonTerrainObjects = {
            NON_TERRAIN_OBJECT_BUSH_01,
            NON_TERRAIN_OBJECT_BUSH_02,
            NON_TERRAIN_OBJECT_BUSH_03,
            NON_TERRAIN_OBJECT_BUSH_04,
            NON_TERRAIN_OBJECT_BUSH_05,
            NON_TERRAIN_OBJECT_BUSH_06,
            NON_TERRAIN_OBJECT_HOUSE1,
            NON_TERRAIN_OBJECT_HOUSE2,
            NON_TERRAIN_OBJECT_TREE01,
            NON_TERRAIN_OBJECT_TREE02,
            NON_TERRAIN_OBJECT_TREE03,
            NON_TERRAIN_OBJECT_TREE04_3PACK,
            NON_TERRAIN_OBJECT_FRUIT_TREE
        };
        public static int GetNonTerrainObjectIndex(string objectName)
        {
            for (int count = 0; count < nonTerrainObjects.Length; count++)
            {
                if(nonTerrainObjects[count].Equals(objectName))
                {
                    return count;
                }
            }
            return -1;
        }
        public static GameObject getPrefab(string prefabName)
        {
            //Debug.Log("Getting prefab - " + prefabName);
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Things/" + prefabName);
            return prefab;
        }
        public static AudioClip getAudioClip(string audioClipName)
        {
            AudioClip clip = (AudioClip)Resources.Load("Audio/" + audioClipName);
            return clip;
        }
        public static Material getMaterial(string materialName)
        {
            Material material = (Material)Resources.Load("Materials/" + materialName);
            return material;
        }
        public static GameObject getCreaturePrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Creatures/" + name);
            return prefab;
        }
        public static GameObject getThingPrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Things/" + name);
            return prefab;
        }
        public static GameObject getTerrainObjectPrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/World/TerrainObjects/" + name);
            return prefab;
        }
        public static GameObject getWorldPrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/World/" + name);
            return prefab;
        }
        public static Texture2D getTexture(string name)
        {
            Texture2D texture = (Texture2D)Resources.Load("Textures/" + name);
            return texture;
        }
        public static GameObject getUIPrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/UI/" + name);
            return prefab;
        }
        public static GameObject GetBuildingPrefab(string name)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/World/Buildings/" + name);
            return prefab;
        }
    }
}
