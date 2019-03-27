using rak.world;
using UnityEngine;
using Valve.VR;

public class RakInput : MonoBehaviour
{
    public SteamVR_Input_Sources handType; // 1
    public SteamVR_Action_Boolean teleportAction; // 2
    public SteamVR_Action_Boolean grabAction; // 3

    private RAKPlayer player;

    private float ignoreInputs = 0;

    private bool getTeleportDown()
    {
        return teleportAction.GetStateDown(handType);
    }
    private bool getGrab()
    {
        return grabAction.GetState(handType);
    }

    private void Awake()
    {
        player = GetComponentInParent<RAKPlayer>();
    }

    private void Update()
    {
        if (ignoreInputs > 0)
            ignoreInputs -= Time.deltaTime;
        else
        {
            if (getGrab())
            {
                Area.SetDestinationForAllCreatureAgents(transform.position);
                ignoreInputs = 1;
            }
            if (getTeleportDown())
            {
                player.transform.position += transform.forward*RAKPlayer.MoveSpeed * Time.deltaTime;
            }
        }
    }
}
