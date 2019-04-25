using System.Collections.Generic;
using UnityEngine;

namespace rak.UI
{
    public enum RootMenu { CreatureBrowser, WorldBrowser,DebugMenu,StartMenu }
    public interface Menu
    {
        void Initialize();
        void RefreshMainText();
        void SetFocusObject(System.Object focus);
        void Deactivate();
        void ChangeToPreviousMenu();
    }

    public class MenuController
    {
        public static RootMenu currentMenu { get; private set; }
        public static int previousMenu { get; private set; }
        private static Dictionary<RootMenu,Menu> availableMenus;

        public MenuController(GameObject creatureBrowserPrefab,GameObject worldBrowserPrefab,
            GameObject debugMenuPrefab)
        {
            availableMenus = new Dictionary<RootMenu, Menu>();
            availableMenus.Add(RootMenu.CreatureBrowser,
                GameObject.Instantiate(creatureBrowserPrefab).GetComponent<CreatureBrowserMono>());
            availableMenus.Add(RootMenu.WorldBrowser,
                GameObject.Instantiate(worldBrowserPrefab).GetComponent<WorldBrowser>());
            availableMenus.Add(RootMenu.DebugMenu,
                GameObject.Instantiate(debugMenuPrefab).GetComponent<DebugMenu>());
            availableMenus[RootMenu.CreatureBrowser].Deactivate();
            availableMenus[RootMenu.WorldBrowser].Deactivate();
            availableMenus[RootMenu.DebugMenu].Deactivate();
            Debug.LogWarning("Menu initialized with size - " + availableMenus.Keys.Count);
        }
        public void Initialize(RootMenu startMenu)
        {
            currentMenu = startMenu;
            availableMenus[currentMenu].Initialize();
        }
        public void RefreshCurrentMenuMainText()
        {
            availableMenus[currentMenu].RefreshMainText();
        }
        public void SetCurrentMenuFocus(System.Object focus)
        {
            availableMenus[currentMenu].SetFocusObject(focus);
        }
        
        public static void ChangeMenu(RootMenu changeTo)
        {
            availableMenus[currentMenu].Deactivate();
            previousMenu = (int)currentMenu;
            currentMenu = changeTo;
            availableMenus[currentMenu].Initialize();
        }
        public static void ChangeToPreviousMenu()
        {
            ChangeMenu((RootMenu)previousMenu);
        }
    }
}
