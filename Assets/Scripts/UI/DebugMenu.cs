﻿using UnityEngine;
using System.Collections;
using rak.UI;
using TMPro;
using rak;
using Unity.Entities;

public class DebugMenu : MonoBehaviour, Menu
{
    public TMP_Text MainText;
    private static string _debugText;
    private float updateEvery = 1f;
    private float sinceLastUpdate = 0;
    private static Entity thingInFocus;
    private const int maxStringLength = 1000;
    private const int removeThisManyWhenMaxed = 100;
    public static string DebugText { get
        {
            return _debugText;
        }
        private set
        {
            if(!_debugText.Equals(value))
            {
                _debugText = value;
            }
        }
    }
    public static void AppendLine(string line)
    {
        appendDebugLine(line);
    }

    private static void appendDebugLine(string line)
    {
        _debugText += "\n" + line;
        //Debug.Log(line);
        if (_debugText.Length > maxStringLength)
        {
            _debugText = _debugText.Substring(removeThisManyWhenMaxed);
        }
    }

    public static void AppendDebugLine(string line,Entity subject)
    {
        if(thingInFocus == subject)
        {
            AppendLine(line);
        }
    }

    public void ChangeToPreviousMenu()
    {
        MenuController.ChangeToPreviousMenu();
    }

    public void OnClick()
    {
        RefreshMainText();
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Initialize()
    {
        gameObject.SetActive(true);
        _debugText = "";
        SetFocusObject(Entity.Null);
        RefreshMainText();
    }

    public void RefreshMainText()
    {
        MainText.text = DebugText;
    }

    public void SetFocusObject(Entity focus)
    {
        if (MenuController.previousMenu == (int)RootMenu.CreatureBrowser)
            thingInFocus = CreatureBrowserMono.SelectedCreature;
        else
        {
            thingInFocus = focus;
        }
    }

    private void setCanvasCamera(Camera creatureCam)
    {
        Canvas canvas = MainText.GetComponentInParent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = creatureCam;
        canvas.planeDistance = .5f;
    }

    private void Update()
    {
        sinceLastUpdate += Time.deltaTime;
        if(sinceLastUpdate > updateEvery)
        {
            sinceLastUpdate = 0;
            if (!MainText.text.Equals(DebugText))
            {
                RefreshMainText();
            }
        }
    }
}
