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
    private static void AppendDebugLine(string line)
    {
        _debugText += "\n" + line;
    }
    public static void AppendDebugLine(string line,Thing subject)
    {
        if(thingInFocus == subject)
        {
            AppendDebugLine(line);
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
