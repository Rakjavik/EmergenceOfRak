using rak;
using rak.world;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    public const bool DEBUG = true;

    public static HexGrid generate(World world)
    {
        HexGrid grid = Instantiate(RAKUtilities.getWorldPrefab("HexGrid").GetComponent<HexGrid>());

        if (world.worldType == World.WorldType.CLASSM)
        {
            grid.cellPrefab = RAKUtilities.getWorldPrefab("Hex Cell").GetComponent<HexCell>();
            grid.cellLabelPrefab = RAKUtilities.getWorldPrefab("Hex Cell Label").GetComponent<Text>();
            grid.chunkPrefab = RAKUtilities.getWorldPrefab("Hex Grid Chunk").GetComponent<HexGridChunk>();
            grid.cellCountX = grid.chunkCountX * HexMetrics.chunkSizeX;
            grid.cellCountZ = grid.chunkCountZ * HexMetrics.chunkSizeZ;
        }
        grid.SetWorld(world);
        grid.CreateChunks(world.worldType);
        grid.CreateCells();
        return grid;
    }

	public int chunkCountX = 8, chunkCountZ = 6;

    public Color[] colors;

	private HexCell cellPrefab;
	private Text cellLabelPrefab;
	private HexGridChunk chunkPrefab;
    private World world;
    private float currentCoolDown = 0;
    public HexCell SelectedCell
    {
        get
        {
            return selectedCell;
        }
        set
        {
            if(selectedCell != value)
            {
                if (selectedCell) selectedCell.Color = selectedCellsOrigColor;
                selectedCell = value;
                selectedCellsOrigColor = selectedCell.Color;
                selectedCell.Color = Color.yellow;
                world.UpdateMainMenu(value);
                Debug.Log("Cell " + selectedCell.coordinates.ToString() + " Selected");
            }
        }
    }
    private HexCell selectedCell;
    private Color selectedCellsOrigColor;

	public Texture2D noiseSource;

	HexGridChunk[] chunks;
	HexCell[] cells;

	int cellCountX, cellCountZ;

	void CreateChunks (World.WorldType worldType) {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];
		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);

                if (i <= chunks.Length/3)
                {
                    chunk.Initialize(HexGridChunk.ChunkElevationVariance.HILLY,HexGridChunk.ChunkMaterial.GRASS);
                }
                else if (i >= (chunks.Length/3)*2)
                {
                    chunk.Initialize(HexGridChunk.ChunkElevationVariance.FLAT, HexGridChunk.ChunkMaterial.GRASS);
                }
                else
                {
                    chunk.Initialize(HexGridChunk.ChunkElevationVariance.STEEP, HexGridChunk.ChunkMaterial.GRASS);
                }
            }
		}
	}

	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];
        
		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index =
			coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

    private void Update()
    {
        if (DEBUG) gameObject.SetActive(false);
        if (currentCoolDown > 0) currentCoolDown += Time.deltaTime;
        if (currentCoolDown > .5f) {
            currentCoolDown = 0;
            //Debug.Log("Click cooldown reset");
        }
        if (Input.GetMouseButtonDown(0) && currentCoolDown == 0)
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(inputRay, out hit))
            {
                HexCell currentCell = GetCell(hit.point);
                SelectedCell = currentCell;
            }
            currentCoolDown = Time.deltaTime;
            //Debug.Log("Click cooldown started");
        }
    }

    public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.SetArea(new Area(cell,world));
        cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
		AddCellToChunk(x, z, cell);
	}
    public void RefreshAllChunks()
    {
        for(int count = 0; count < chunks.Length; count++)
        {
            chunks[count].Refresh();
        }
    }
	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

    public HexCell[] FindUncivilizedHexCellsNotUnderwater()
    {
        List<HexCell> cells = new List<HexCell>();
        foreach(HexCell cell in this.cells)
        {
            if (!cell.IsUnderwater && cell.CurrentOccupants == null)
                cells.Add(cell);
        }
        return cells.ToArray();
    }

    public void Save(BinaryWriter writer)
    {
        for (int count = 0; count < cells.Length; count++)
        {
            cells[count].Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        for(int count = 0; count < cells.Length; count++)
        {
            cells[count].Load(reader);
        }
        for (int count = 0; count < chunks.Length; count++)
        {
            chunks[count].Refresh();
        }
    }

    public void SetWorld(World world)
    {
        this.world = world;
    }
}