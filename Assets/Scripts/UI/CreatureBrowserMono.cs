using rak.creatures;
using rak.ecs.ThingComponents;
using rak.ecs.world;
using rak.world;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
namespace rak.UI
{
    public enum CreatureBrowserWindow { Creature_Detail_List }
    public class CreatureBrowserMono : MonoBehaviour,Menu
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

        public static Entity SelectedCreature;

        public TMP_Dropdown creatureDropDown;
        public TMP_Text detailText;
        public TMP_Text clockText;
        public TMP_Text[] memoryText;
        private bool initialized = false;
        private CreatureBrowserWindow currentWindow;
        private static Entity selectedCreature;
        private Entity[] creatureMap;
        private float timeSinceLastUpdate = 0;
        private float updateEvery = .5f;
        public Entity BrowserEntity;
        private EntityManager em;

        public void Initialize(CreatureBrowserWindow startingWindow)
        {
            em = Unity.Entities.World.Active.EntityManager;
            creatureMap = null;
            if(startingWindow == CreatureBrowserWindow.Creature_Detail_List)
            {
                NativeArray<Entity> creatures = em.CreateEntityQuery(typeof(IsCreature)).
                    ToEntityArray(Allocator.TempJob);
                InitializeCreatureList(ref creatures);
                creatures.Dispose();
            }
        }
        private void InitializeCreatureList(ref NativeArray<Entity> creatures)
        {
            if (initialized) Debug.LogWarning("Initialize called on CreatureBrowser when already initialized");
            creatureDropDown.ClearOptions();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            int creatureLength = creatures.Length;
            for (int count = 0; count < creatureLength; count++)
            {
                TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(creatures[count].ToString());
                options.Add(option);
            }
            creatureMap = creatures.ToArray();
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
            em = Unity.Entities.World.Active.EntityManager;
            BrowserEntity = em.CreateEntity();
            em.AddComponentData(BrowserEntity, new CreatureBrowser
            {
                MemoryBuffer = em.AddBuffer<CreatureMemoryBuf>(BrowserEntity)
            });
            Initialize(CreatureBrowserWindow.Creature_Detail_List);
        }
        private void RefreshMemoryText()
        {
            int maxRows = 25;
            int maxColumns = memoryText.Length;
            if(!selectedCreature.Equals(Entity.Null))
            {
                DynamicBuffer<CreatureMemoryBuf> memBuffer = em.GetBuffer<CreatureMemoryBuf>(selectedCreature);
                int memoryLength = memBuffer.Length;
                int count = 0;
                for (int column = 0; column < maxColumns; column++)
                {
                    StringBuilder columnText = new StringBuilder();
                    for (int row = 0; row < maxRows; row++)
                    {
                        if (count == memoryLength)
                            break;
                        if (!memBuffer[count].memory.Subject.Equals(Entity.Null))
                        {
                            if (memBuffer[count].memory.GetInvertVerb())
                                columnText.Append("!");
                            columnText.Append(memBuffer[count].memory.Verb.ToString() + "-" +
                                memBuffer[count].memory.Subject.ToString() + "\n");
                            //+ " " + memBuffers[count].memory.Iterations + "\n");
                        }
                        else
                        {
                            columnText.Append("Empty\n");
                        }
                        if(row+1 == maxRows)
                        {
                            memoryText[column].text = columnText.ToString();
                        }
                        count++;
                    }
                }
            }
        }
        public void RefreshMainText()
        {
            CreatureState state = em.GetComponentData<CreatureState>(selectedCreature);
            CreatureAI ai = em.GetComponentData<CreatureAI>(selectedCreature);
            Target target = em.GetComponentData<Target>(selectedCreature);
            NativeArray<Entity> areaArray = em.CreateEntityQuery(typeof(rak.ecs.area.Area)).ToEntityArray(Allocator.TempJob);
            if(areaArray.Length == 0)
            {
                areaArray.Dispose();
                return;
            }
            ecs.area.Area area = em.GetComponentData<ecs.area.Area>(areaArray[0]);
            Sun sun = em.GetComponentData<Sun>(areaArray[0]);
            ecs.ThingComponents.Needs needs = em.GetComponentData<ecs.ThingComponents.Needs>(selectedCreature);
            string text = DETAILTEXT.Replace("{name}",selectedCreature.ToString());
                text = text.Replace("{state}", state.Value.ToString());
                text = text.Replace("{task}", ai.CurrentTask.ToString());
                text = text.Replace("{taskTarget}", target.targetEntity.ToString());
                text = text.Replace("{currentAction}", ai.CurrentAction.ToString());
                text = text.Replace("{hunger}", needs.Hunger.ToString());
                text = text.Replace("{sleep}", needs.Sleep.ToString());
                detailText.text = text;
            int cc = area.NumberOfCreatures;
            int tc = 0;
            clockText.text = "Creatures-" + cc + " Things-" + tc + " Time-" + sun.AreaLocalTime +
                " Elapsed-" + sun.ElapsedHours + "\n";
            // TODO this is ghetto //
            RefreshMemoryText();
            areaArray.Dispose();
        }

        public void SetFocusObject(Entity focus)
        {
            selectedCreature = focus;
            SelectedCreature = selectedCreature;
            FollowCamera.SetFollowTarget(SelectedCreature);
        }
        public void Deactivate()
        {
            gameObject.SetActive(true);
            initialized = false;
        }
        public void OnDropDownChange()
        {
            if (creatureMap.Length == 0) return;
            SetFocusObject(creatureMap[creatureDropDown.value]);
            RefreshMainText();
        }
        private void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate > updateEvery)
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