using rak;
using rak.world;
using Unity.Mathematics;
using UnityEngine;

public struct GridSector
{
    public float3 WorldPositionStart { get; private set; }
    public float3 WorldPositionEnd { get; private set; }
    public string name { get; private set; }
    public Coord2 gridPosition { get; private set; }
    public System.Guid guid { get; private set; }

    private System.Guid parentTerrain;
    

    public GridSector(Coord2 gridPosition, float3 worldPositionStart, 
        float3 worldPositionEnd,string terrainName,RAKTerrain terrain)
    {
        guid = System.Guid.NewGuid();
        name = (terrainName + "-" + gridPosition.x + "-" + gridPosition.y);
        this.gridPosition = gridPosition;
        this.WorldPositionStart = new Vector3(worldPositionStart.x,worldPositionStart.y,worldPositionStart.z);
        this.WorldPositionEnd = new Vector3(worldPositionEnd.x, worldPositionEnd.y, worldPositionEnd.z); ;
        if (terrain != null)
        {
            this.parentTerrain = terrain.guid;
            terrain.AddGridSectorHash(this);
        }
        else
            this.parentTerrain = System.Guid.Empty;
        
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionEnd.x,0,WorldPositionStart.z),
            Color.yellow, 1);
        Debug.DrawLine(WorldPositionStart, new Vector3(WorldPositionStart.x, 0, WorldPositionEnd.z),
            Color.yellow, 1);
    }

    public Vector3 GetSectorPosition()
    {
        float3 position = new Vector3(WorldPositionStart.x+(Grid.CurrentElementSize.x/2), 0, WorldPositionStart.z + 
            (Grid.CurrentElementSize.y / 2));
        return position;
    }
    
    public float GetTerrainHeightFromGlobalPos(float3 position)
    {
        return RAKTerrainMaster.GetTerrainByGuid(parentTerrain).GetHeightAt(position);
    }
    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(new Vector2(WorldPositionStart.x,WorldPositionStart.z), 
            new Vector2(WorldPositionEnd.x,WorldPositionEnd.z),.5f);
    }
    public static GridSector Empty { get
        {
            {
                return new GridSector(new Coord2(-1,-1), float3.zero, float3.zero, "", null);
            }
        }
    }
    public bool IsEmpty()
    {
        return gridPosition.x == -1;
    }
}
