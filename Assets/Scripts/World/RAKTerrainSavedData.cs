using UnityEngine;
using System.IO;
using System;

public partial class RAKTerrainMaster
{
    [Serializable]
    public class RAKTerrainSavedData
    {
        private float[] alphaMapFlat;
        private float[] heightMapFlat;
        public static int bufferSize = 1024*4;
        public RAKVector3 tdSize;
        public RAKSplatPrototype[] splatPrototypes;
        public RAKTreeProtoType[] treeProtoTypes;
        public RAKTreeInstance[] trees;
        public RAKTerrainObjectSaveData[] nonTerrainObjectsSaveData;
        public RAKTerrainSavedData() { }
        public RAKTerrainSavedData(float[,,] alphaMap, float[,] heightMap, SplatPrototype[] splatPrototypes, TreePrototype[] treePrototypes, TreeInstance[] trees
            , RAKTerrainObject[] nonTerrainObjects,Vector3 terraindDataSize)
        {
            this.alphaMapFlat = new float[alphaMap.Length];
            this.tdSize = new RAKVector3(terraindDataSize); 
            int flatCount = 0;
            for (int x = 0; x < alphaMap.GetLength(0); x++)
            {
                for (int y = 0; y < alphaMap.GetLength(1); y++)
                {
                    for (int z = 0; z < alphaMap.GetLength(2); z++)
                    {
                        this.alphaMapFlat[flatCount] = alphaMap[x, y, z];
                        flatCount++;
                    }
                }
            }
            this.heightMapFlat = new float[heightMap.Length];
            flatCount = 0;
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    heightMapFlat[flatCount] = heightMap[x, y];
                    flatCount++;
                }
            }
            
            this.splatPrototypes = new RAKSplatPrototype[splatPrototypes.Length];
            this.treeProtoTypes = new RAKTreeProtoType[treePrototypes.Length];
            this.trees = new RAKTreeInstance[trees.Length];
            for (int count = 0; count < splatPrototypes.Length; count++)
            {
                this.splatPrototypes[count] = new RAKSplatPrototype(splatPrototypes[count].texture.name, splatPrototypes[count].tileSize);
            }
            for (int count = 0; count < treeProtoTypes.Length; count++)
            {
                this.treeProtoTypes[count] = new RAKTreeProtoType(treePrototypes[count]);
            }
            for (int count = 0; count < trees.Length; count++)
            {
                this.trees[count] = new RAKTreeInstance(trees[count]);
            }
            this.nonTerrainObjectsSaveData = new RAKTerrainObjectSaveData[nonTerrainObjects.Length];
            for (int count = 0; count < nonTerrainObjects.Length; count++)
            {
                this.nonTerrainObjectsSaveData[count] = RAKTerrainObjectSaveData.createSaveData(nonTerrainObjects[count]);
            }
        }
        public TerrainData generateTerrainDataFromFlatMap()
        {
            TerrainData td = new TerrainData();
            td.splatPrototypes = getSplatProtoTypes();
            float[,,] alphaMap = new float[(int)tdSize.x - 1, (int)tdSize.z - 1, splatPrototypes.Length];
            int flatCount = 0;
            for (int x = 0; x < alphaMap.GetLength(0); x++)
            {
                for (int z = 0; z < alphaMap.GetLength(1); z++)
                {
                    for (int y = 0; y < splatPrototypes.Length; y++)
                    {
                        alphaMap[x, z, y] = alphaMapFlat[flatCount];
                        flatCount++;
                    }
                }
            }
            td.SetAlphamaps(0, 0, alphaMap);
            float[,] heightMap = new float[(int)(tdSize.x), ((int)tdSize.z)];
            flatCount = 0;
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    heightMap[x, y] = heightMapFlat[flatCount];
                    flatCount++;
                }
            }
            td.heightmapResolution = heightMap.GetLength(0);
            td.size = new Vector3(tdSize.x, tdSize.y, tdSize.z);
            td.SetHeights(0, 0, heightMap);
            td.treePrototypes = getTreeProtoTypes();
            td.treeInstances = getTreeInstances();
            return td;
        }

        private static bool saveTerrainObject(RAKTerrainSavedData terrain, string path)
        {
            binarySerialize(path, terrain);
            return true;
        }
        public static bool saveTerrainBytes(RAKTerrain terrain, string path)
        {
            TerrainData data = terrain.getTerrainData();
            float[,,] alphaMap = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
            float[,] heightMap = data.GetHeights(0, 0, data.heightmapWidth, data.heightmapHeight);
            SplatPrototype[] splats = data.splatPrototypes;
            Debug.LogWarning("Saving terrain");
            
            Vector3 terrainDataSize = terrain.terrain.terrainData.size;
            RAKTerrainSavedData saveData = new RAKTerrainSavedData(alphaMap, heightMap, splats, data.treePrototypes, 
                data.treeInstances, terrain.nonTerrainObjects,terrainDataSize);
            saveTerrainObject(saveData, path);
            terrain.savedData = saveData;
            return true;
        }
        public static void binarySerialize(string serializationFile, System.Object objectToSerialize)
        {
            //serialize
            using (Stream stream = File.Open(serializationFile, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, objectToSerialize);
            }
        }
        public static RAKTerrainSavedData loadTerrain(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (RAKTerrainSavedData)bformatter.Deserialize(stream);
            }
        }

        public bool appendToArray(bool isAlphaNotHeight,float[] data,int startIndex)
        {
            if (isAlphaNotHeight)
                Array.Copy(data, 0, alphaMapFlat, startIndex, data.Length);
            else
                Array.Copy(data, 0, heightMapFlat, startIndex, data.Length);
            if (isAlphaNotHeight)
                return startIndex + data.Length >= alphaMapFlat.Length;
            else
                return startIndex + data.Length >= heightMapFlat.Length+(tdSize.x-1*tdSize.z-1);// Height map has an extra element
        }

        public float[] getAlphaMapFlat()
        {
            return alphaMapFlat;
        }
        public float[] getHeightMapFlat()
        {
            return heightMapFlat;
        }
        private SplatPrototype[] getSplatProtoTypes()
        {
            SplatPrototype[] returnArray = new SplatPrototype[splatPrototypes.Length];
            for (int count = 0; count < splatPrototypes.Length; count++)
            {
                returnArray[count] = new SplatPrototype();
                returnArray[count].tileSize = splatPrototypes[count].getTileSize();
                returnArray[count].texture = splatPrototypes[count].getTexture();
            }
            return returnArray;
        }
        private TreePrototype[] getTreeProtoTypes()
        {
            TreePrototype[] returnArray = new TreePrototype[treeProtoTypes.Length];
            for (int count = 0; count < treeProtoTypes.Length; count++)
            {
                returnArray[count] = treeProtoTypes[count].getTreePrototype();
            }
            return returnArray;
        }
        private TreeInstance[] getTreeInstances()
        {
            TreeInstance[] returnArray = new TreeInstance[trees.Length];
            for (int count = 0; count < trees.Length; count++)
            {
                returnArray[count] = trees[count].getTreeInstance();
            }
            return returnArray;
        }
        [Serializable]
        public class RAKVector3
        {
            public float x, y, z;
            public RAKVector3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            public RAKVector3(Vector3 vector3)
            {
                this.x = vector3.x;
                this.y = vector3.y;
                this.z = vector3.z;
            }
            public RAKVector3(Quaternion rotation)
            {
                Vector3 euler = rotation.eulerAngles;
                this.x = euler.x;
                this.y = euler.y;
                this.z = euler.z;
            }
            public Quaternion getRotation()
            {
                return Quaternion.Euler(x, y, z);
            }
            public Vector3 getVector3()
            {
                return new Vector3(x, y, z);
            }
            public RAKVector3() { }
        }
        [Serializable]
        public class RAKSplatPrototype
        {
            public string textureName;
            public RAKVector3 tileSize;

            public RAKSplatPrototype(string textureName, Vector2 tileSize)
            {
                this.textureName = textureName;
                this.tileSize = new RAKVector3(tileSize.x, tileSize.y, 0);
            }
            public Texture2D getTexture()
            {
                return (Texture2D)Resources.Load("Textures/" + textureName);
            }
            public Vector2 getTileSize()
            {
                return new Vector2(tileSize.x, tileSize.y);
            }
            public RAKSplatPrototype() { }
        }
        [Serializable]
        public class RAKTreeProtoType
        {
            public float bendFactor;
            public string prefabName;

            public RAKTreeProtoType(TreePrototype tree)
            {
                this.bendFactor = tree.bendFactor;
                this.prefabName = tree.prefab.name;
            }
            public TreePrototype getTreePrototype()
            {
                TreePrototype tree = new TreePrototype();
                tree.bendFactor = bendFactor;
                tree.prefab = (GameObject)Resources.Load("Prefabs/" + prefabName);
                return tree;
            }
            public RAKTreeProtoType() { }
        }
        [Serializable]
        public class RAKTreeInstance
        {
            public byte[] color = new byte[4];
            public byte[] lightingColor = new byte[4];
            public float heightScale;
            public float widthScale;
            public RAKVector3 position;
            public int protoypeIndex;

            public TreeInstance getTreeInstance()
            {
                TreeInstance instance = new TreeInstance();
                instance.color = new Color32(color[3], color[2], color[1], color[0]);
                instance.lightmapColor = new Color32(lightingColor[3], lightingColor[2], lightingColor[1], lightingColor[0]);
                instance.heightScale = heightScale;
                instance.widthScale = widthScale;
                instance.position = new Vector3(position.x, position.y, position.z);
                instance.prototypeIndex = protoypeIndex;
                return instance;
            }

            public RAKTreeInstance(TreeInstance tree)
            {
                color[0] = tree.color.a;
                color[1] = tree.color.b;
                color[2] = tree.color.g;
                color[3] = tree.color.r;
                lightingColor[0] = tree.lightmapColor.a;
                lightingColor[1] = tree.lightmapColor.b;
                lightingColor[2] = tree.lightmapColor.g;
                lightingColor[3] = tree.lightmapColor.r;
                this.heightScale = tree.heightScale;
                this.widthScale = tree.widthScale;
                this.position = new RAKVector3(tree.position.x, tree.position.y, tree.position.z);
                this.protoypeIndex = tree.prototypeIndex;
            }
            public RAKTreeInstance() { }
        }
    }
}
