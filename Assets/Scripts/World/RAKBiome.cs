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
                RAKUtilities.NON_TERRAIN_OBJECT_LOW_POLY_BUSH_4},
                new int[] {
                0, // House1
                0, // House2
                2, // Tree01
                2, // Tree02
                2, // Tree03
                2, // Tree04_3k
                5, // Bush01
                5, // Bush02
                5, // Bush03
                5 // Bush04
                });
            biome.depth = 80;
            biome.numberOfTrees = 0;
            biome.offsetX = UnityEngine.Random.Range(0, 100);
            biome.offsetY = UnityEngine.Random.Range(0, 100);
            biome.scale = UnityEngine.Random.Range(1, 3);
            biome.type = BIOMETYPE.Forest;
            return biome;
        }
        public RAKBiome(string[] prefabNames, int[] objectCounts)
        {
            this.prefabNames = prefabNames;
            this.objectCounts = objectCounts;
        }
        public RAKBiome() { }
    }
}