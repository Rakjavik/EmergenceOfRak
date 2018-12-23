using rak.creatures;
using rak.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace rak.world
{
    /*public class World : MonoBehaviour
    {
        public enum WorldType { CLASSM }
        public const int NUMBEROFSTARTINGCIVS = 30;
        public static string WORLD_DATAPATH;
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
        private List<Civilization> civilizations;
        public List<Civilization> GetCivs() { return civilizations; }
        public RAKTerrainMaster currentTerrain;
        private HexMapEditor editor;
        private RAKMainMenu mainMenu;
        public HexMapCamera mapCam;

        private bool editing = false;

        private void Awake()
        {
            WORLD_DATAPATH = Application.persistentDataPath + "/Worlds/";
            worldType = WorldType.CLASSM;
            worldName = "AlphaWorld";
            civilizations = new List<Civilization>();
            HexMetrics.Initialize(worldType);
            hexGrid = HexGrid.generate(this);
            mapCam.grid = hexGrid;
            currentTerrain = GetComponent<RAKTerrainMaster>();
            if (editing)
                editor = Instantiate(RAKUtilities.getWorldPrefab("HexMapEditor")).GetComponent<HexMapEditor>();

            for (int count = 0; count < NUMBEROFSTARTINGCIVS; count++)
            {
                Civilization civ = new Civilization(
                    Species.BASE_SPECIES.GNAT,
                    Civilization.GenerateCivName(Species.BASE_SPECIES.GNAT), false, 1, 1200);
                civ.FindHome(this, true);
                addCiv(civ);
            }
            hexGrid.RefreshAllChunks();
            mainMenu = Instantiate(RAKUtilities.getUIPrefab("MainMenu").GetComponent<RAKMainMenu>());
            mainMenu.Initialize(this);
            if(AutoLoadArea)
            {
                HexCell autoLoadCell = hexGrid.GetCell(new Vector3(0,0,0));
                mainMenu.LoadCell(autoLoadCell);

            }
        }

        public void LoadArea(HexCell cell)
        {
            currentTerrain.Initialize(this, cell);
            
            hexGrid.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(false);
        }

        public HexCell[] FindUncivilizedHexCellsNotUnderwater()
        {
            return hexGrid.FindUncivilizedHexCellsNotUnderwater();
        }
        public void addCiv(Civilization civ)
        {
            if (!civilizations.Contains(civ))
                civilizations.Add(civ);
        }
        public void removeCiv(Civilization civ)
        {
            if (civilizations.Contains(civ))
                civilizations.Remove(civ);
        }
        public void UpdateMainMenu()
        {
            mainMenu.UpdateText();
        }
        public void UpdateMainMenu(HexCell cellInfo)
        {
            mainMenu.UpdateText(cellInfo);
        }
        public bool IsSaveDataAvailable(HexCell cell)
        {
            return currentTerrain.IsSaveDataAvailable(cell,WorldName);
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
    }

    /*public class Area
    {
        private List<Thing> allThings;
        private World world;
        private HexCell cell;

        public Area(HexCell cell,World world)
        {
            this.cell = cell;
            this.world = world;
        }
        public void Initialize()
        {
            allThings = new List<Thing>();
            /*addCreatureToWorld("Gnat");
            addCreatureToWorld("Gnat", new Vector3(50, 32, 50));
            for (int i = 0; i < 5; i++)
            {
                addThingToWorld("fruit");
            }
            
        }
        
        public void Update(long delta)
        {
            foreach(Thing thing in allThings)
            {
                thing.Update();
            }
        }

        public Thing[] findConsumeable(Species.CONSUMPTION_TYPE consumptionType)
        {
            List<Thing> things = new List<Thing>();
            foreach(Thing thing in allThings)
            {
                if(thing.match(consumptionType))
                {
                    things.Add(thing);
                }
            }
            return things.ToArray();
        }
        public void addCreatureToWorld(string nameOfPrefab) { addCreatureToWorld(nameOfPrefab, Vector3.zero); }
        public void addCreatureToWorld(string nameOfPrefab,Vector3 position)
        {
            GameObject thingObject = RAKUtilities.getCreaturePrefab(nameOfPrefab);
            GameObject newThing = UnityEngine.Object.Instantiate(thingObject);
            newThing.GetComponent<Creature>().initialize(nameOfPrefab,this);
            if(position != Vector3.zero) { newThing.transform.position = position; }
            allThings.Add(newThing.GetComponent<Thing>());
        }
        public void addThingToWorld(string nameOfPrefab)
        {
            addThingToWorld(nameOfPrefab, Vector3.zero);
        }
        public void addThingToWorld(string nameOfPrefab,Vector3 position)
        {
            GameObject thingObject = RAKUtilities.getThingPrefab(nameOfPrefab);
            GameObject newThing = UnityEngine.Object.Instantiate(thingObject);
            newThing.GetComponent<Thing>().initialize("fruit");
            if (position != Vector3.zero) newThing.transform.position = position;
            allThings.Add(newThing.GetComponent<Thing>());
        }
        public bool removeThingFromWorld(Thing thing)
        {
            return allThings.Remove(thing);
        }
    }*/
}
