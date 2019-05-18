using rak.creatures;
using rak.ecs.ThingComponents;
using rak.ecs.world;
using rak.UI;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace rak.world
{
    public class World : MonoBehaviour
    {
        public enum Time_Of_Day { SunRise, Midday, SunSet, Night }

        public static bool ISDEBUGSCENE { get; private set; }

        public FollowCamera followCamera;
        public static FollowCamera FollowCamera;
        public enum WorldType { CLASSM }
        public const int NUMBEROFSTARTINGCIVS = 30;
        public static string WORLD_DATAPATH;
        private static World world;

        public static HexCell currentCell { get; private set; }
        public static World GetWorld() { return world; }
        public static Area CurrentArea { get; private set; }
        public static Tribe ActiveTribe { get; private set; }

        public bool AutoLoadArea;

        public WorldType worldType;
        public string WorldName
        {
            get
            {
                return worldName;
            }
            set
            {
                if (worldName != value) WorldName = value;
            }
        }
        private string worldName;

        private HexGrid hexGrid; // Grid that stores the world
        private Dictionary<HexCell, Tribe> tribes;
        private List<Civilization> civilizations;
        public RAKTerrainMaster masterTerrain;
        public GameObject creatureBrowserPrefab;
        public GameObject worldBrowserPrefab;
        public GameObject debugMenuPrefab;
        private HexMapEditor editor;
        private MenuController mainMenu;
        public HexMapCamera mapCam;
        public AudioClip AmbientSound;

        private bool editing = false;
        public static bool Initialized = false;
        private float worldUpdatesEvery = 1; //Seconds
        private float sinceLastUpdate = 0;

        private void InitializeDebugWorld()
        {
            civilizations = new List<Civilization>();
            civilizations.Add(new Civilization(BASE_SPECIES.Gnat, "DaGnats", true, 5, 15));
            Tribe debugTribe = new Tribe(5,BASE_SPECIES.Gnat);
            ActiveTribe = debugTribe;
            //civilizations[0].AddTribe(debugTribe);
            Debug.LogWarning("DEBUG MODE ENABLED");
            world = this;
            worldType = WorldType.CLASSM;
            worldName = "DebugWorld";
            tribes = new Dictionary<HexCell, Tribe>();
            HexMetrics.Initialize(worldType);
            hexGrid = HexGrid.generate(this);
            currentCell = debugTribe.FindHome(this, true);
            masterTerrain.Initialize(this, currentCell);
        }

        // ENTRY METHOD //
        private void Awake()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Area.WorldDisabled();
        }

        private void FixedUpdate()
        {
            if(Initialized)
                CurrentArea.FixedUpdate(Time.deltaTime);
        }

        private void Initialize()
        {
            ISDEBUGSCENE = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower().Contains("debug");
            WORLD_DATAPATH = Application.persistentDataPath + "/Worlds/";
            if (ISDEBUGSCENE && !Initialized)
            {
                InitializeDebugWorld();
                return;
            }
            world = this;
            FollowCamera = this.followCamera;
            if (!Directory.Exists(WORLD_DATAPATH))
            {
                Directory.CreateDirectory(WORLD_DATAPATH);
            }
            worldType = WorldType.CLASSM;
            worldName = "AlphaWorld";
            tribes = new Dictionary<HexCell, Tribe>();
            civilizations = new List<Civilization>();
            civilizations.Add(new Civilization(BASE_SPECIES.Gnat, "DaGnats",true,5,15));
            HexMetrics.Initialize(worldType);
            hexGrid = HexGrid.generate(this);
            mapCam.grid = hexGrid;
            //masterTerrain = GetComponent<RAKTerrainMaster>();
            if (editing)
                editor = Instantiate(RAKUtilities.getWorldPrefab("HexMapEditor")).GetComponent<HexMapEditor>();

            for (int count = 0; count < NUMBEROFSTARTINGCIVS; count++)
            {
                Tribe tribe = new Tribe(1200, BASE_SPECIES.Gnat);
                HexCell cell = tribe.FindHome(this, true);
                //civilizations[0].AddTribe(tribe);
                addTribe(tribe,cell);
            }
            hexGrid.RefreshAllChunks();
            mainMenu = new MenuController(creatureBrowserPrefab, worldBrowserPrefab,debugMenuPrefab);
            if(AutoLoadArea)
            {
                HexCell autoLoadCell = hexGrid.GetCell(new Vector3(0,0,0));
                LoadArea(autoLoadCell);
                mainMenu.Initialize(RootMenu.CreatureBrowser);
            }
            else
                mainMenu.Initialize(RootMenu.WorldBrowser);
            
        }
        private void completeInitialize()
        {
            em = Unity.Entities.World.Active.EntityManager;
            CurrentArea = currentCell.MakeArea(this, ActiveTribe);
            MenuController menuController = new MenuController(creatureBrowserPrefab, worldBrowserPrefab, debugMenuPrefab);
            FollowCamera = followCamera;
            menuController.Initialize(RootMenu.CreatureBrowser);

            Initialized = true;
        }

        public void LoadArea(HexCell cell)
        {
            masterTerrain.Initialize(this, cell);
            hexGrid.gameObject.SetActive(false);
            Tribe tribe = tribes[cell];
            if (tribe != null)
            {
                CurrentArea = cell.MakeArea(this, tribe);
                tribe.Initialize();
            }
            else
            {
                Debug.LogError("Civ is null during area load");
            }
            MenuController.ChangeMenu(RootMenu.CreatureBrowser);
        }

        public HexCell[] FindUncivilizedHexCellsNotUnderwater()
        {
            return hexGrid.FindUncivilizedHexCellsNotUnderwater();
        }
        public void addTribe(Tribe tribe,HexCell cell)
        {
            tribes.Add(cell,tribe);
        }
        public void removeCiv(HexCell cell)
        {
            tribes.Remove(cell);
        }
        public List<Civilization> GetCivs() { return civilizations; }
        public void UpdateMainMenu()
        {
            mainMenu.RefreshCurrentMenuMainText();
        }
        public void UpdateMainMenu(HexCell cellInfo)
        {
            //mainMenu.SetCurrentMenuFocus(cellInfo);
            mainMenu.RefreshCurrentMenuMainText();
        }
        
        public bool IsSaveDataAvailable(HexCell cell)
        {
            return masterTerrain.IsSaveDataAvailable(cell,WorldName);
        }
        private void SetEditing(bool editing)
        {
            if(editing != this.editing)
            {
                if(editing)
                {
                    editor.gameObject.SetActive(true);
                    editor.hexGrid = this.hexGrid;
                }
                else
                {
                    editor.gameObject.SetActive(false);
                }
                this.editing = editing;
            }
        }

        private EntityManager em;
        private void Update()
        {
            if (!Initialized)
            {
                if (RAKTerrainMaster.Initialized)
                    completeInitialize();
                return;
            }
            sinceLastUpdate += Time.deltaTime;
            if(sinceLastUpdate > worldUpdatesEvery)
            {
                sinceLastUpdate = 0;
                EntityQuery query = em.CreateEntityQuery(new ComponentType[] { typeof(Produces),typeof(Position) });
                NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
                for(int count = 0; count < entities.Length; count++)
                {
                    Produces producer = em.GetComponentData<Produces>(entities[count]);
                    if(producer.ProductionAvailable == 1)
                    {
                        Position position = em.GetComponentData<Position>(entities[count]);
                        string prefab;
                        if (producer.thingToProduce == Thing.Thing_Types.Fruit)
                            prefab = "fruit";
                        else
                            prefab = "None";
                        float3 spawnPosition = position.Value;
                        spawnPosition.y += 10;
                        CurrentArea.addThingToWorld(prefab,spawnPosition,false);
                        producer.ProductionAvailable = 0;
                        producer.timeSinceLastSpawn = 0;
                        em.SetComponentData(entities[count], producer);
                    }
                }
                entities.Dispose();
            }
            CurrentArea.update(Time.deltaTime);
        }

    }

    public struct Coord2
    {
        public int x, y;

        public Coord2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public struct Coord3
    {
        public int x, y, z;

        public Coord3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
