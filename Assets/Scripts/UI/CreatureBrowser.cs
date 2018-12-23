using rak.creatures;
using rak.world;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace rak.UI
{
    public enum CreatureBrowserWindow { Creature_Detail_List }
    public class CreatureBrowser : MonoBehaviour,Menu
    {
        public const string DETAILTEXT =
            "--Creature Name--\n"+
            "{name}\n" +
            "--Creature State--\n" +
            "{state}\n" +
            "--Current Task--\n" +
            "{task}\n" +
            "--Current Task Target--\n"+
            "{taskTarget}\n" +
            "--Current Action--\n"+
            "{currentAction}\n" +
            "Hunger -- {hungerRelative}-{hunger}\n"+
            "Sleep -- {sleepRelative}-{sleep}\n" +
            "Current Brake amount - {currentBrake}";

        public static Creature SelectedCreature;

        public TMP_Dropdown creatureDropDown;
        public TMP_Text detailText;
        private bool initialized = false;
        private CreatureBrowserWindow currentWindow;
        private Creature selectedCreature;
        private Creature[] creatureMap;
        private float timeSinceLastUpdate = 0;
        private float updateEvery = .5f;

        public void Initialize(CreatureBrowserWindow startingWindow)
        {
            creatureMap = null;
            if(startingWindow == CreatureBrowserWindow.Creature_Detail_List)
            {
                InitializeCreatureList(Area.GetAllThings().ToArray());
            }
        }
        private void InitializeCreatureList(Thing[] allThingsInArea)
        {
            if (initialized) Debug.LogWarning("Initialize called on CreatureBrowser when already initialized");
            creatureDropDown.ClearOptions();
            List<Creature> tempMap = new List<Creature>();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            for (int count = 0; count < allThingsInArea.Length; count++)
            {
                if (allThingsInArea[count].match(Thing.BASE_TYPES.CREATURE))
                {
                    TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(allThingsInArea[count].name);
                    options.Add(option);
                    tempMap.Add((Creature)allThingsInArea[count]);
                }
            }
            creatureMap = tempMap.ToArray();
            creatureDropDown.AddOptions(options);
            currentWindow = CreatureBrowserWindow.Creature_Detail_List;
            OnDropDownChange();
            initialized = true;
        }
        public void ReplaceCurrentWindowWith(CreatureBrowserWindow replaceWith)
        {
            if(currentWindow == replaceWith)
            {
                Debug.LogWarning("Call to replace window with already current - " + replaceWith);
            }
            currentWindow = replaceWith;

        }

        public void Initialize()
        {
            gameObject.SetActive(true);
            Initialize(CreatureBrowserWindow.Creature_Detail_List);
        }

        public void RefreshMainText()
        {
            if(selectedCreature != null && selectedCreature.IsInitialized())
            {
                string text = DETAILTEXT.Replace("{name}",selectedCreature.name);
                text = text.Replace("{state}", selectedCreature.GetCurrentState().ToString());
                text = text.Replace("{task}", selectedCreature.GetCurrentTask().ToString());
                text = text.Replace("{taskTarget}", selectedCreature.GetCurrentTaskTargetName());
                text = text.Replace("{currentAction}", selectedCreature.GetCurrentAction().ToString());
                text = text.Replace("{hunger}", selectedCreature.GetNeedAmount(Needs.NEEDTYPE.HUNGER).ToString());
                text = text.Replace("{hungerRelative}", selectedCreature.
                    GetRelativeNeedAmount(Needs.NEEDTYPE.HUNGER).ToString());
                text = text.Replace("{sleep}", selectedCreature.GetNeedAmount(Needs.NEEDTYPE.SLEEP).ToString());
                text = text.Replace("{sleepRelative}", selectedCreature.
                    GetRelativeNeedAmount(Needs.NEEDTYPE.SLEEP).ToString());
                text = text.Replace("{currentBrake}", selectedCreature.GetCreatureAgent().currentBrakeAmount.ToString());
                detailText.text = text;
            }
        }

        public void SetFocusObject(object focus)
        {
            if (selectedCreature != null)
            {
                selectedCreature.GetComponentInChildren<Camera>().enabled = false;
            }

            selectedCreature = (Creature)focus;
            Camera creatureCam = selectedCreature.GetComponentInChildren<Camera>();
            creatureCam.enabled = true;
            SelectedCreature = selectedCreature;
            setCanvasCamera(creatureCam);
        }
        private void setCanvasCamera(Camera creatureCam)
        {
            Canvas canvas = detailText.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = creatureCam;
            canvas.planeDistance = .5f;
        }
        public void Deactivate()
        {
            gameObject.SetActive(false);
            initialized = false;
        }
        public void OnDropDownChange()
        {
            SetFocusObject(creatureMap[creatureDropDown.value]);
            RefreshMainText();
        }
        private void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if(timeSinceLastUpdate > updateEvery)
            {
                timeSinceLastUpdate = 0;
                RefreshMainText();
            }
        }
        public void ChangeToDebugMenu()
        {
            MenuController.ChangeMenu(RootMenu.DebugMenu);
        }
        public void ChangeToPreviousMenu()
        {
            MenuController.ChangeToPreviousMenu();
        }
    }
}