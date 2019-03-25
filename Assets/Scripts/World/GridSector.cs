using rak;
using rak.world;
using UnityEngine;

public struct GridSector
{
    public Vector3 WorldPositionStart { get; private set; }
    public Vector3 WorldPositionEnd { get; private set; }
    public string name { get; private set; }
    public Vector2 gridPosition { get; private set; }
    public System.Guid guid { get; private set; }

    private System.Guid parentTerrain;
    

    public GridSector(Vector2 gridPosition, Vector3 worldPositionStart, 
        Vector3 worldPositionEnd,string terrainName,RAKTerrain terrain)
    {
        guid = System.Guid.NewGuid();
        name = (terrainName + "-" + gridPosition.x + "-" + gridPosition.y);
        this.gridPosition = new Vector2((int)gridPosition.x,(int)gridPosition.y);
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
        Vector3 position = new Vector3(WorldPositionStart.x+(Grid.CurrentElementSize.x/2), 0, WorldPositionStart.z + 
            (Grid.CurrentElementSize.y / 2));
        return position;
    }
    
    public float GetTerrainHeightFromGlobalPos(Vector3 position)
    {
        return RAKTerrainMaster.GetTerrainByGuid(parentTerrain).GetHeightAt(position);
    }
    public Vector2 GetTwoDLerpOfSector()
    {
        return Vector2.Lerp(WorldPositionStart, WorldPositionEnd,.5f);
    }
    public static GridSector Empty { get
        {
            {
                return new GridSector(Vector2.zero, Vector3.zero, Vector3.zero, "", null);
            }
        }
    }
    public bool IsEmpty()
    {
        if(WorldPositionStart == Vector3.zero && WorldPositionEnd == Vector3.zero)
        {
            return true;
        }
        return false;
    }
}
