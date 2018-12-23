using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using System;

public class RAKMotionController : MonoBehaviour
{
    public static float floorPosition = 3.3f;
    public static float headPosition = 3.7112f;
    public static float height = headPosition - floorPosition;
    public static float armLength = .6f;

    public RAKMotionController otherHand;
    public GameObject hmd;
    public bool initialized = false;

    private RAKPlayer player;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device steamVRController;

    private bool vibrate = false;
    private bool gripped = false; // If grip is being held
    private List<VibrationRequest> vibrationQueue;
    private bool triggerHeld;
    private float triggerHeldFor = 0;

    private void Awake()
    {
        
    }

    // Use this for initialization
    public void Initialize()
    {
        player = GetComponentInParent<RAKPlayer>();
        if (RAKPlayer.vrPlayer)
        {
            trackedObject = GetComponent<SteamVR_TrackedObject>();
            transform.localScale = Vector3.one;
            steamVRController = SteamVR_Controller.Input((int)trackedObject.index);
        }
        else
        {
            steamVRController = null;
        }
        vibrationQueue = new List<VibrationRequest>();
        initialized = true;
    }

    private void searchForValidDevice()
    {
        for (int count = 1; count < 12; count++)
        {
            Debug.Log("Other controller index - " + otherHand.getSteamDeviceIndex());
            if (count != otherHand.getSteamDeviceIndex())
            {
                steamVRController = SteamVR_Controller.Input(count);
                if (steamVRController.valid && steamVRController.connected)
                {
                    trackedObject = GetComponent<SteamVR_TrackedObject>();
                    trackedObject.SetDeviceIndex(count);
                    Debug.LogWarning(name + "New device index set to - " + trackedObject.index);
                    Initialize();
                    break;
                }
                else
                {
                    Debug.LogWarning(name + " Failed input on index - " + count);
                    Debug.Log(steamVRController.connected + "-" + steamVRController.valid);
                }
            }
        }
    }
    // Update is called once per frame
    public void update(bool isVrPlayer)
    {
        if (isVrPlayer)
        {
            if (steamVRController != null && steamVRController.valid)
            {
                steamVRController = SteamVR_Controller.Input((int)trackedObject.index);
                // If no action was submitted from combo buttons, proceed to check for regular inputs //
                if (!processCombination())
                {
                    processTrigger();
                    processTouchPad();
                    processMenu();
                    processGrip();
                }
                if (vibrate)
                {
                    vibrationQueue[0].Update(Time.deltaTime);
                    if (vibrationQueue[0].done)
                    {
                        vibrationQueue.Remove(vibrationQueue[0]);
                        if (vibrationQueue.Count == 0)
                        {
                            vibrate = false;
                        }
                    }
                }
            }
            else
            {
                searchForValidDevice();
            }
            if (triggerHeld) triggerHeldFor += Time.deltaTime;
        }
    }
    private Collider getRaycastForward()
    {
        // Raycast forward, if hit, check if it's attachable //
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            return hit.collider;
        }
        return null;
    }


    #region BUTTON PRESSES
    private bool processCombination()
    {
        // Trigger and menu at same time on both controllers //
        if (otherHand.getSteamVRController().GetPress(SteamVR_Controller.ButtonMask.ApplicationMenu) && triggerHeld && otherHand.triggerHeld)
        {
            return true;
        }
        else if (steamVRController.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu) && triggerHeld)
        {
            if (triggerHeldFor > 1.5f)
            {
                
            }
        }
        return false;
    }
    public void processMenuDown()
    {
        
    }
    public void processMenuUp()
    {
    }
    private void processMenu()
    {
        // Menu PRESS DOWN//
        if (steamVRController.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            processMenuDown();
        }
        // Menu PRESS UP//
        else if (steamVRController.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            processMenuUp();
        }
    }
    public void processTouchPadPressDown(Vector2 touchPad)
    {
        /*if (menu != null) // Menu is open
        {
            if (menu.getSelection().isWieldable())
            {
                equipSelected(menu.getSelection().getWieldable());
            }
        }*/
    }
    public void processTouchPadTouchDown(Vector2 touchPad)
    {

    }
    public void processTouchAny(Vector2 touchPad)
    {
        
    }
    public void processTouchPadTouchUp()
    {

    }
    private void processTouchPad()
    {
        Vector2 touchPadLocation = steamVRController.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
        // TOUCHPAD PRESS //
        if (steamVRController.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            processTouchPadPressDown(touchPadLocation);
        }
        // TOUCHPAD TOUCH DOWN //
        else if (steamVRController.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            processTouchPadTouchDown(touchPadLocation);
        }
        // TOUCHPAD TOUCH ANY //
        else if (steamVRController.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            processTouchAny(touchPadLocation);
        }
        // TOUCHPAD TOUCH UP //
        else if (steamVRController.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            processTouchPadTouchUp();
        }
    }
    private void processTrigger()
    {
        if(steamVRController.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            processTriggerDown();
        }
        else if (steamVRController.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            processTriggerUp();
        }
    }
    public void processTriggerDown()
    {
        triggerHeld = true;
    }
    public void processTriggerUp()
    {
        triggerHeld = false;
        triggerHeldFor = 0;
    }
    
    private void processGrip()
    {
        // GRIP //
        if (steamVRController.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            gripped = true;
            // Both controllers gripped? //
            if (otherHand.isGripped())
            {
            }
        }
        else if (steamVRController.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            gripped = false;
        }
    }
    #endregion

    #region GETTERS/SETTERS
    public SteamVR_Controller.Device getSteamVRController()
    {
        return steamVRController;
    }
    public Vector3 getPointBetweenControllers()
    {
        return Vector3.Lerp(transform.position,otherHand.transform.position,.5f);
    }

    public float getDistanceFromHMD()
    {
        return Vector3.Distance(transform.position, hmd.transform.position);
    }
    public bool isGripped()
    {
        return gripped;
    }
    public void setGripped(bool gripped)
    {
        this.gripped = gripped;
    }
    public RAKMotionController getOtherController()
    {
        return otherHand;
    }

    public SteamVR_Controller.Device getDevice()
    {
        return steamVRController;
    }

    public int getSteamDeviceIndex()
    {
        if (player == null || !initialized)
        {
            return -1;
        }
        if (RAKPlayer.vrPlayer)
        {
            if (steamVRController == null)
            {
                return 0;
            }
            return (int)steamVRController.index;
        }
        else
        {
            return 0;
        }
    }
    #endregion
    #region VIBRATION
    public void addVibration(float strength, float duration, bool overrideAllOthers)
    {
        addVibration(strength, duration, overrideAllOthers, null);
    }
    public void addVibration(VibrationRequest vr, bool overrideAllOthers)
    {
        addVibration(0, 0, overrideAllOthers, vr);
    }
    private void addVibration(float strength, float duration, bool overrideAllOthers, VibrationRequest vr)
    {
        if (vr == null) vr = new VibrationRequest(duration, strength, steamVRController, this);
        if (overrideAllOthers || vibrationQueue.Count == 0)
        {
            vibrationQueue = new List<VibrationRequest>();
            vibrationQueue.Add(vr);
        }
        else if (vibrationQueue.Count > 0)
        {
            // If the new requested vibration is harder then current, override //
            if (vibrationQueue[0].vibrateStrength < vr.vibrateStrength)
            {
                // Current request will last longer then new request //
                if (vibrationQueue[0].stopVibratingAt > duration)
                {
                    // Move current request behind new request //
                    vibrationQueue[1] = vibrationQueue[0];
                    vibrationQueue[0] = vr;
                }
            }

        }
        vibrate = true;
    }
    private void clearVibrationQueue()
    {
        vibrate = false;
        vibrationQueue = new List<VibrationRequest>();
    }
    #endregion
    #region VIBRATION SYSTEM
    public class VibrationRequest
    {
        public float beenVibratingFor = 0;
        public float stopVibratingAt = 0;
        public float vibrateStrength = 0;

        public bool done;
        private bool initialized = false;
        public SteamVR_Controller.Device controller;
        public MonoBehaviour parent;

        public VibrationRequest(float duration,float strength,SteamVR_Controller.Device controller,MonoBehaviour parent)
        {
            this.parent = parent;
            this.stopVibratingAt = duration;
            this.vibrateStrength = strength;
            this.controller = controller;
            done = false;
        }
        public void Update(float deltaTime)
        {
            beenVibratingFor += deltaTime;
            if (!initialized)
            {
                Coroutine coroutine = parent.StartCoroutine(vibrationThread(stopVibratingAt, vibrateStrength, controller));
                initialized = true;
            }
            if (beenVibratingFor > stopVibratingAt)
            {
                done = true;
            }
        }
        public float getTimeLeft()
        {
            if(done)
            {
                return 0;
            }
            return stopVibratingAt - beenVibratingFor;
        }
        private IEnumerator vibrationThread(float length, float strength,SteamVR_Controller.Device device)
        {
            for (float i = 0; i < length; i += Time.deltaTime)
            {
                device.TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
                yield return null; //every single frame for the duration of "length" you will vibrate at "strength" amount
            }
        }
    }
    #endregion
    
}
