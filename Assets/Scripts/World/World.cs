using rak.creatures;
using rak.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace rak.world
{
    public class World : MonoBehaviour
    {
        public static bool ISDEBUGSCENE { get; private set; }

        public FollowCamera followCamera;
        public static FollowCamera FollowCamera;
        public enum WorldType { CLASSM }
        public const int NUMBEROFSTARTINGCIVS = 30;
        public const int THING_PROCESS_BATCH_SIZE_DIVIDER = 15;
        public static string WORLD_DATAPATH;
        private static World world;

        public static HexCell currentCell { get; private set; }
        public static World GetWorld() { return world; }
        public static Area CurrentArea { get; private set; }

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

        private bool editing = false;
        private bool _initialized;
        private float worldUpdatesEvery = 1; //Seconds
        private float sinceLastUpdate = 0;

        private void InitializeDebugWorld()
        {
            civilizations = new List<Civilization>();
            civilizations.Add(new Civilization(BASE_SPECIES.Gnat, "DaGnats", true, 5, 15));
            Tribe debugTribe = new Tribe(5,BASE_SPECIES.Gnat);
            
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
            CurrentArea = currentCell.MakeArea(this,debugTribe);
            MenuController menuController = new MenuController(creatureBrowserPrefab, worldBrowserPrefab, debugMenuPrefab);
            FollowCamera = followCamera;
            menuController.Initialize(RootMenu.CreatureBrowser);
            _initialized = true;
        }
        private void Awake()
        {
            Initialize();
        }
        private void Initialize()
        {
            ISDEBUGSCENE = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower().Contains("debug");
            if (ISDEBUGSCENE && !_initialized)
            {
                InitializeDebugWorld();
                return;
            }
            world = this;
            FollowCamera = this.followCamera;
            WORLD_DATAPATH = Application.persistentDataPath + "/Worlds/";
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
            _initialized = true;
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
            mainMenu.SetCurrentMenuFocus(cellInfo);
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

        private void Update()
        {
            sinceLastUpdate += Time.deltaTime;
            if(sinceLastUpdate > worldUpdatesEvery)
            {
                
                sinceLastUpdate = 0;
            }
            CurrentArea.update(Time.deltaTime);
        }

    }
}
