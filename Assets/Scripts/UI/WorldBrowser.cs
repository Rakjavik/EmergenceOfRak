using rak.creatures;
using rak.world;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace rak.UI
{
    public enum WorldBrowserWindow { LoadScreen }
    public class WorldBrowser : MonoBehaviour,Menu
    {
        private Text text;
        private WorldBrowserWindow currentWindow;
        private World world;
        private Text loadButtonText;
        public static HexCell selectedCell { get; private set; }

        public void Initialize(World world)
        {
            text = transform.GetChild(1).GetComponentInChildren<Text>();
            loadButtonText = GetComponentInChildren<Button>().GetComponentInChildren<Text>();
            this.world = world;
            RefreshMainText();
        }
        public void ReplaceCurrentWindow(WorldBrowserWindow replaceWith)
        {
            changeWindow(replaceWith);
        }
        private void changeWindow(WorldBrowserWindow window)
        {
            if (currentWindow == window) Debug.LogWarning("Call to switch to same window");
            if(window == WorldBrowserWindow.LoadScreen)
            {
                deactivateWindow(currentWindow);
            }
            currentWindow = window;
            if(currentWindow == WorldBrowserWindow.LoadScreen)
            {
                text.gameObject.SetActive(true);
                loadButtonText.gameObject.SetActive(true);
            }
        }
        private void deactivateWindow(WorldBrowserWindow window)
        {
            if(window == WorldBrowserWindow.LoadScreen)
            {
                text.gameObject.SetActive(false);
                loadButtonText.gameObject.SetActive(false);
            }
        }
        public void OnClick()
        {
            if (selectedCell)
                LoadCell(selectedCell);
        }
        public void LoadCell(HexCell cell)
        {
            world.LoadArea(cell);
        }
        public void UpdateButtonText(string text)
        {
            loadButtonText.text = text;
        }
        public void RefreshMainText()
        {
            if (selectedCell == null)
            {
                string text = "";
                text += "World - " + world.name + "\n";
                text += "Type - " + world.worldType + "\n";
                text += "----CIVILIZATIONS----\n";
                foreach (Civilization civ in world.GetCivs())
                {
                    text += "Name - " + civ.CivName;
                }
                this.text.text = text;
            }
            else
            {
                string text = "";
                if (selectedCell.CurrentOccupants != null)
                    text += "Current Occupants - " + selectedCell.CurrentOccupants.TribeName + "\n";
                text += "Cell Coordinates \n" + selectedCell.coordinates.ToStringOnSeparateLines() + "\n";
                text += "Terrain type - " + selectedCell.GetChunkMaterial() + "\n";
                text += "Elevation type - " + selectedCell.GetChunkElevationVariance();
                this.text.text = text;
                if (world.IsSaveDataAvailable(selectedCell))
                {
                    loadButtonText.text = "Load";
                }
                else
                {
                    loadButtonText.text = "Generate";
                }
            }
        }
        public void Initialize()
        {
            Initialize(World.GetWorld());
            gameObject.SetActive(true);
        }

        public void SetFocusObject(System.Object focus)
        {
            Debug.Log("Focus set for world browser - " + (HexCell)focus);
            selectedCell = (HexCell)focus;
        }
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void ChangeToPreviousMenu()
        {
            MenuController.ChangeToPreviousMenu();
        }
    }
}
