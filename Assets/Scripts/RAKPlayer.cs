using rak.UI;
using UnityEngine;

public class RAKPlayer : MonoBehaviour
{
    public static bool vrPlayer = false;

    private void Awake()
    {
        
    }

    private void Update()
    {
        transform.position = CreatureBrowser.SelectedCreature.transform.position;
    }
}