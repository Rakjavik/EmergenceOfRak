using rak;
using rak.creatures;
using rak.world;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour {

    public Tribe CurrentOccupants
    {
        get
        {
            return currentOccupants;
        }
        set
        {
            if(currentOccupants != value)
            {
                currentOccupants = value;
            }
        }
    }
    private Tribe currentOccupants;

	public HexCoordinates coordinates;
	public RectTransform uiRect;
	public HexGridChunk chunk;

    private Color color;
    public Color Color {
        get
        {
            return color;
        }
        set
        {
            this.color = value;
            RefreshSelfOnly();
        }
    }

    private Text cellLabel;

    public int TerrainTypeIndex {
        get {
            return terrainTypeIndex;
        }
        set
        {
            if (terrainTypeIndex != value)
            {
                terrainTypeIndex = value;
                Refresh();
            }
        }
    }
	public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }
            elevation = value;
            if (elevation > HexMetrics.hexCellMaxElevation)
                elevation = HexMetrics.hexCellMaxElevation;
            else if (elevation < -HexMetrics.hexCellMaxElevation)
                elevation = -HexMetrics.hexCellMaxElevation;
            
            RefreshPosition();
            ValidateRivers();

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad(i, false);
                }
            }
            Refresh();
        }
    }
    private void RefreshPosition() {
        Vector3 position = transform.localPosition;
        position.y = elevation * HexMetrics.elevationStep;
        position.y +=
            (HexMetrics.SampleNoise(position).y * 2f - 1f) *
            HexMetrics.elevationPerturbStrength;
        transform.localPosition = position;

        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }

	public int WaterLevel {
		get {
			return waterLevel;
		}
		set {
			if (waterLevel == value) {
				return;
			}
			waterLevel = value;
			ValidateRivers();
			Refresh();
		}
	}

	public bool IsUnderwater {
		get {
			return waterLevel > elevation;
		}
	}

	public bool HasIncomingRiver {
		get {
			return hasIncomingRiver;
		}
	}

	public bool HasOutgoingRiver {
		get {
			return hasOutgoingRiver;
		}
	}

	public bool HasRiver {
		get {
			return hasIncomingRiver || hasOutgoingRiver;
		}
	}

	public bool HasRiverBeginOrEnd {
		get {
			return hasIncomingRiver != hasOutgoingRiver;
		}
	}

	public HexDirection RiverBeginOrEndDirection {
		get {
			return hasIncomingRiver ? incomingRiver : outgoingRiver;
		}
	}

	public bool HasRoads {
		get {
			for (int i = 0; i < roads.Length; i++) {
				if (roads[i]) {
					return true;
				}
			}
			return false;
		}
	}

	public HexDirection IncomingRiver {
		get {
			return incomingRiver;
		}
	}

	public HexDirection OutgoingRiver {
		get {
			return outgoingRiver;
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}

    public void Save(BinaryWriter writer)
    {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)elevation);
        writer.Write((byte)waterLevel);
        writer.Write(hasIncomingRiver);
        writer.Write((byte)incomingRiver);
        writer.Write(hasOutgoingRiver);
        writer.Write((byte)outgoingRiver);

        for (int count = 0; count < roads.Length; count++)
        {
            writer.Write(roads[count]);
        }
    }
    public void Load(BinaryReader reader)
    {
        terrainTypeIndex = reader.ReadByte();
        elevation = reader.ReadByte();
        waterLevel = reader.ReadByte();
        hasIncomingRiver = reader.ReadBoolean();
        incomingRiver = (HexDirection)reader.ReadByte();
        hasOutgoingRiver = reader.ReadBoolean();
        outgoingRiver = (HexDirection)reader.ReadByte();

        for (int count = 0; count < roads.Length; count++)
        {
            roads[count] = reader.ReadBoolean();
        }
    }

	public float StreamBedY {
		get {
			return
				(elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float RiverSurfaceY {
		get {
			return
				(elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float WaterSurfaceY {
		get {
			return
				(waterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

    int terrainTypeIndex;
	int elevation = int.MinValue;
	int waterLevel;

	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outgoingRiver;

	[SerializeField]
	HexCell[] neighbors;

	[SerializeField]
	bool[] roads;

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}

	public bool HasRiverThroughEdge (HexDirection direction) {
		return
			hasIncomingRiver && incomingRiver == direction ||
			hasOutgoingRiver && outgoingRiver == direction;
	}

	public void RemoveIncomingRiver () {
		if (!hasIncomingRiver) {
			return;
		}
		hasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(incomingRiver);
		neighbor.hasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveOutgoingRiver () {
		if (!hasOutgoingRiver) {
			return;
		}
		hasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(outgoingRiver);
		neighbor.hasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveRiver () {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void SetOutgoingRiver (HexDirection direction) {
		if (hasOutgoingRiver && outgoingRiver == direction) {
			return;
		}

		HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) {
			return;
		}

		RemoveOutgoingRiver();
		if (hasIncomingRiver && incomingRiver == direction) {
			RemoveIncomingRiver();
		}
		hasOutgoingRiver = true;
		outgoingRiver = direction;

		neighbor.RemoveIncomingRiver();
		neighbor.hasIncomingRiver = true;
		neighbor.incomingRiver = direction.Opposite();

		SetRoad((int)direction, false);
	}

	public bool HasRoadThroughEdge (HexDirection direction) {
		return roads[(int)direction];
	}

	public void AddRoad (HexDirection direction) {
		if (
			!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
			GetElevationDifference(direction) <= 1
		) {
			SetRoad((int)direction, true);
		}
	}

	public void RemoveRoads () {
		for (int i = 0; i < neighbors.Length; i++) {
			if (roads[i]) {
				SetRoad(i, false);
			}
		}
	}

	public int GetElevationDifference (HexDirection direction) {
		int difference = elevation - GetNeighbor(direction).elevation;
		return difference >= 0 ? difference : -difference;
	}

	bool IsValidRiverDestination (HexCell neighbor) {
		return neighbor && (
			elevation >= neighbor.elevation || waterLevel == neighbor.elevation
		);
	}

	void ValidateRivers () {
		if (
			hasOutgoingRiver &&
			!IsValidRiverDestination(GetNeighbor(outgoingRiver))
		) {
			RemoveOutgoingRiver();
		}
		if (
			hasIncomingRiver &&
			!GetNeighbor(incomingRiver).IsValidRiverDestination(this)
		) {
			RemoveIncomingRiver();
		}
	}

	void SetRoad (int index, bool state) {
		roads[index] = state;
		neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
		neighbors[index].RefreshSelfOnly();
		RefreshSelfOnly();
	}

	void Refresh () {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
		}
	}

	void RefreshSelfOnly () {
		chunk.Refresh();
	}

    private Area area;
    public Area GetArea() { return area; }
    public void SetArea(Area area) { this.area = area; }
    private void Update()
    {
        string roadText = "";
        for(int direction = 0; direction < neighbors.Length; direction++)
        {
            roadText += System.Enum.GetNames(typeof(HexDirection))[direction]
                + "-" + roads[direction] + "\n";
        }
        uiRect.GetComponent<Text>().text = roadText;

    }

    private HexGridChunk.ChunkMaterial chunkMaterial;
    public HexGridChunk.ChunkMaterial GetChunkMaterial() { return chunkMaterial; }
    private HexGridChunk.ChunkElevationVariance chunkElevationVariance;
    public HexGridChunk.ChunkElevationVariance GetChunkElevationVariance() { return chunkElevationVariance; }

    public void Initialize(HexGridChunk.ChunkMaterial chunkMaterial,
        HexGridChunk.ChunkElevationVariance chunkElevation)
    {
        this.chunkElevationVariance = chunkElevation;
        this.chunkMaterial = chunkMaterial;
        if (chunkMaterial == HexGridChunk.ChunkMaterial.GRASS)
        {
            this.color = Color.green;
        }
        if (chunkElevation == HexGridChunk.ChunkElevationVariance.FLAT)
        {
            Elevation = 0;
        }
        
        int offset = 0;
        int currentHeight = 0;
        if (HasAtLeastOneNeighbor())
        {
            currentHeight = GetAverageHeightFromValidNeighbors();
            if (chunkElevation == HexGridChunk.ChunkElevationVariance.HILLY)
            {
                if (Random.value > .5)
                {
                    if (Random.value > .5)
                    {
                        offset = 1;
                    }
                    else
                    {
                        offset = -1;
                    }
                }

            }
            else if (chunkElevation == HexGridChunk.ChunkElevationVariance.STEEP)
            {
                if (Random.value > .5)
                {
                    if (Random.value > .5)
                    {
                        offset = Random.Range(0,10);
                    }
                    else
                    {
                        offset = Random.Range(-10,0);
                    }
                }
            }
        }
        else
        {
            offset = 0;
        }
        Elevation = currentHeight + offset;
    }
    public int GetAverageHeightFromValidNeighbors()
    {
        int neighborCount = 0;
        int totalElevation = 0;
        for (int count = 0; count < neighbors.Length; count++)
        {
            if (neighbors[count] != null)
            {
                neighborCount++;
                totalElevation += neighbors[count].elevation;
            }
        }
        return totalElevation / neighborCount;
    }
    public bool HasAtLeastOneNeighbor()
    {
        for(int count = 0; count < neighbors.Length; count++)
        {
            if (neighbors[count] != null) return true;
        }
        return false;
    }
    public bool HasAllNeighbors()
    {
        for (int count = 0; count < neighbors.Length; count++)
        {
            if (neighbors[count] == null) return false;
        }
        return true;
    }
    public void GenerateDetails()
    {
        if (IsUnderwater)
        {
            return;
        }

        // POPULATION BUILDINGS //
        if(currentOccupants != null) { 
        GameObject buildingPrefab = null;
        int buildingsToGenerate = 0;
            // TENTS //
            if (currentOccupants.GetPopulation() > 10 &&
                currentOccupants.GetPopulation() < 100)
            {
                buildingsToGenerate = currentOccupants.GetPopulation() / 15;
                if (buildingsToGenerate == 0) buildingsToGenerate = 1;
                buildingPrefab = RAKUtilities.getWorldPrefab(Civilization.PREFABTENT);
            }
            // VILLAGE //
            else if (currentOccupants.GetPopulation() >= 100)
            {
                // BUILD ROADS //
                if (currentOccupants.GetPopulation() >=
                    Civilization.STARTBUILDINGROADSATPOPULATION)
                {
                    for (int count = 0; count < neighbors.Length; count++)
                    {
                        if (neighbors[count] != null &&
                            neighbors[count].CurrentOccupants != null &&
                            neighbors[count].CurrentOccupants.GetPopulation() >= 100)
                        {
                            roads[count] = true;
                        }
                    }
                }
                // BUILD HOUSES //
                if (currentOccupants.GetPopulation() <= 1000)
                {
                    buildingsToGenerate = currentOccupants.GetPopulation() / 150;
                    if (buildingsToGenerate == 0)
                    {
                        buildingsToGenerate = 1;

                    }
                    buildingPrefab = RAKUtilities.getWorldPrefab(Civilization.PREFABTOWNHOUSE1);
                }
                // Big Village //
                else
                {
                    buildingsToGenerate = 1;
                    buildingPrefab = RAKUtilities.getWorldPrefab(Civilization.PREFABTOWNWINDMILL);
                }

                // INSTANTIATE BUILDINGS //
                if (buildingPrefab != null && buildingsToGenerate > 0)
                {
                    GameObject building = null;
                    if (buildingsToGenerate > 1)
                    {
                        for (int count = 0; count < buildingsToGenerate; count++)
                        {
                            building = Instantiate(buildingPrefab);
                            building.transform.SetParent(transform, false);
                            building.transform.RotateAround(transform.position, Vector3.up, 60 * count);
                            building.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        }
                    }
                    // Center if only building //
                    else
                    {
                        building = Instantiate(buildingPrefab);
                        building.transform.SetParent(transform, false);
                        building.transform.position = transform.position;
                        building.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    }
                }
            }
        }
    }
    public string GetCellSaveFileName(string worldName)
    {
        string fileName = World.WORLD_DATAPATH + worldName;
        fileName += coordinates.X;
        fileName += coordinates.Z;
        return fileName;
    }

    public Area MakeArea(World world,Tribe tribe)
    {
        area = new Area(this, world);
        area.Initialize(tribe);
        return area;
    }
}