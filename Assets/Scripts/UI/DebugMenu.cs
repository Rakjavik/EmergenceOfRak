using UnityEngine;
using System.Collections;
using rak.UI;
using TMPro;
using rak;

public class DebugMenu : MonoBehaviour, Menu
{
    public TMP_Text MainText;
    private static string _debugText;
    private float updateEvery = 1f;
    private float sinceLastUpdate = 0;
    private static Thing thingInFocus;
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

    public static void AppendDebugLine(string line,Thing subject)
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
        SetFocusObject(null);
        RefreshMainText();
    }

    public void RefreshMainText()
    {
        MainText.text = DebugText;
    }

    public void SetFocusObject(object focus)
    {
        if (MenuController.previousMenu == (int)RootMenu.CreatureBrowser)
            thingInFocus = CreatureBrowser.SelectedCreature;
        else
        {
            thingInFocus = (Thing)focus;
        }
        if(thingInFocus.GetComponent<Camera>() != null)
            setCanvasCamera(thingInFocus.GetComponent<Camera>());
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
