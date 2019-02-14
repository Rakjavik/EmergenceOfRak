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
                5, // House1
                5, // House2
                1, // Tree01
                1, // Tree02
                1, // Tree03
                1, // Tree04
                1, // Bush01
                1, // Bush02
                1, // Bush03
                1 // Bush04
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