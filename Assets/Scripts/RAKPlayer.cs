using rak.UI;
using UnityEngine;

public class RAKPlayer : MonoBehaviour
{
    public static bool vrPlayer = false;

    private static RakInput leftHand;
    private static RakInput rightHand;
    public static float MoveSpeed { get; private set; }

    private void Awake()
    {
        MoveSpeed = 250;
    }

    private void Update()
    {
        
    }

    
}
