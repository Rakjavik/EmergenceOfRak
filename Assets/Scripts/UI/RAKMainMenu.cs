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
    public class RAKMainMenu : MonoBehaviour
    {
        private Text text;
        private World world;
        private Text loadButtonText;
        private HexCell selectedCell;
        public void Initialize(World world)
        {
            text = transform.GetChild(1).GetComponentInChildren<Text>();
            loadButtonText = GetComponentInChildren<Button>().GetComponentInChildren<Text>();
            this.world = world;
            UpdateText();
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
        public void UpdateText()
        {
            string text = "";
            text += "World - " + world.name + "\n";
            text += "Type - " + world.worldType + "\n";
            text += "----CIVILIZATIONS----\n";
            foreach (Civilization civ in world.GetCivs())
            {
                text += "Name - " + civ.CivName + "\n Population - " + civ.getPopulation() + "\n";
            }
            this.text.text = text;
        }
        public void UpdateText(HexCell cellSelected)
        {
            this.selectedCell = cellSelected;
            string text = "";
            text += "Cell Coordinates \n" + cellSelected.coordinates.ToStringOnSeparateLines() + "\n";
            text += "Terrain type - " + cellSelected.GetChunkMaterial() + "\n";
            text += "Elevation type - " + cellSelected.GetChunkElevationVariance();
            this.text.text = text;
            Debug.Log("Menu text updated to - " + text);

            if(world.IsSaveDataAvailable(cellSelected))
            {
                loadButtonText.text = "Load";
            }
            else
            {
                loadButtonText.text = "Generate";
            }
        }
    }
}
