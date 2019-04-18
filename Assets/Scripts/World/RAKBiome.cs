using System;
using rak;

public partial class RAKTerrainMaster
{
    [Serializable]
    public class RAKBiome
    {
        public enum BIOMETYPE { Forest }

        public BIOMETYPE type { get; set; }
        public string[] prefabNames { get; set; }
        public int[] objectCounts { get; set; }

        public int depth { get; set; }// 150;
        public int numberOfTrees { get; set; }// 0;                 
        public float offsetX { get; set; }// 100f;
        public float offsetY { get; set; }// 100f;
        public float scale { get; set; }// 2f;

        public static RAKBiome getForestBiome()
        {
            RAKBiome biome = new RAKBiome(new string[]
                {RAKUtilities.NON_TERRAIN_OBJECT_HOUSE1,RAKUtilities.NON_TERRAIN_OBJECT_HOUSE2,RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_1,
                RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_2,RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_3,RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_TREE_4,
                RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_1,RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_2,RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_3,
                RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_4,RAKUtilities.NON_TERRAIN_OBJECT_FRUIT_TREE,RAKUtilities.NON_TERRAIN_OBJECT_TREE01,
                RAKUtilities.NON_TERRAIN_OBJECT_TREE02,RAKUtilities.NON_TERRAIN_OBJECT_TREE03,RAKUtilities.NON_TERRAIN_OBJECT_TREE04_3PACK,
                RAKUtilities.NON_TERRAIN_OBJECT_BUSH_01,RAKUtilities.NON_TERRAIN_OBJECT_BUSH_02,RAKUtilities.NON_TERRAIN_OBJECT_BUSH_03,
                RAKUtilities.NON_TERRAIN_OBJECT_BUSH_04,RAKUtilities.NON_TERRAIN_OBJECT_BUSH_05,RAKUtilities.NON_TERRAIN_OBJECT_BUSH_06},
                new int[] {
                1, // House1
                1, // House2
                0, // Tree01LP
                0, // Tree02LP
                0, // Tree03LP
                0, // Tree04LP
                0, // Bush01
                0, // Bush02
                0, // Bush03
                0, // Bush04
                1, // Fruit Tree
                0, // Tree01
                0, // Tree02
                0, // Tree03
                0, // Tree04Pack
                3, // Bush01
                3, // Bush02
                3, // Bush03
                3, // Bush04
                3, // Bush05
                3 // Bush06
                });
            biome.depth = 80;
            biome.numberOfTrees = 0;
            biome.offsetX = UnityEngine.Random.Range(0, 100);
            biome.offsetY = UnityEngine.Random.Range(0, 100);
            biome.scale = UnityEngine.Random.Range(1, 1.5f);
            biome.type = BIOMETYPE.Forest;
            return biome;
        }
        public void GetNewOffsets()
        {
            offsetX = UnityEngine.Random.Range(0, 100);
            offsetY = UnityEngine.Random.Range(0, 100);
        }
        public RAKBiome(string[] prefabNames, int[] objectCounts)
        {
            this.prefabNames = prefabNames;
            this.objectCounts = objectCounts;
        }
        public RAKBiome() { }
    }
}