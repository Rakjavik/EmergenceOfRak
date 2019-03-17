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
            "--Creature Name--\n" +
            "{name}\n" +
            "--Creature State--\n" +
            "{state}\n" +
            "--Current Task--\n" +
            "{task}\n" +
            "--Current Action--\n" +
            "{currentAction}\n" +
            "--Current Task Target--\n" +
            "{taskTarget}\n" +
            "Hunger -- {hungerRelative}-{hunger}\n" +
            "Sleep -- {sleepRelative}-{sleep}\n";

        public static Creature SelectedCreature;

        public TMP_Dropdown creatureDropDown;
        public TMP_Text detailText;
        public TMP_Text clockText;
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
                if (allThingsInArea[count].match(Thing.Base_Types.CREATURE))
                {
                    TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(allThingsInArea[count].thingName);
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
                string text = DETAILTEXT.Replace("{name}",selectedCreature.thingName);
                text = text.Replace("{state}", selectedCreature.GetCurrentState().ToString());
                text = text.Replace("{task}", selectedCreature.GetCurrentTask().ToString());
                text = text.Replace("{taskTarget}", selectedCreature.GetCurrentTaskTargetName());
                text = text.Replace("{currentAction}", selectedCreature.GetCurrentAction().ToString());
                text = text.Replace("{hunger}", selectedCreature.GetNeedAmount(Needs.NEEDTYPE.HUNGER).ToString());
                text = text.Replace("{hungerRelative}", selectedCreature.
                    GetRelativeNeedAmount(Needs.NEEDTYPE.HUNGER).ToString());
                text = text.Replace("{sleep}", ((int)selectedCreature.GetNeedAmount(Needs.NEEDTYPE.SLEEP)).ToString());
                text = text.Replace("{sleepRelative}", ((int)selectedCreature.
                    GetRelativeNeedAmount(Needs.NEEDTYPE.SLEEP)).ToString());
                text = text.Replace("{currentBrake}", ((int)selectedCreature.GetCreatureAgent().
                    CurrentBrakeAmountRequest.magnitude).ToString());
                detailText.text = text;
            }
            int cc = World.CurrentArea.ActiveCreatureCount;
            int tc = World.CurrentArea.ActiveThingCount;
            clockText.text = "Creatures-" + cc + " Things-" + tc + " Time-" + Area.GetFriendlyLocalTime()+"\n";
            clockText.text += "Deaths Flight-" + World.CurrentArea.DeathsByFlight + 
                " Hunger-" + World.CurrentArea.DeathsByHunger;
        }

        public void SetFocusObject(object focus)
        {
            selectedCreature = (Creature)focus;
            SelectedCreature = selectedCreature;
            FollowCamera.SetFollowTarget(SelectedCreature.transform);
        }
        public void Deactivate()
        {
            gameObject.SetActive(false);
            initialized = false;
        }
        public void OnDropDownChange()
        {
            SetFocusObject(creatureMap[creatureDropDown.value]);
            List<Creature> livingCreatures = new List<Creature>();
            for(int count = 0; count < creatureMap.Length; count++)
            {
                if (creatureMap[count].GetCurrentState() != Creature.CREATURE_STATE.DEAD)
                    livingCreatures.Add(creatureMap[count]);
                    
            }
            creatureMap = livingCreatures.ToArray();
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