using rak.creatures;
using rak.UI;
using rak.world;
using System.Collections.Generic;
using UnityEngine;

public class RAKPlayer : MonoBehaviour
{
    public static bool vrPlayer = false;

    private static RakInput leftHand;
    private static RakInput rightHand;
    public static float MoveSpeed { get; private set; }
    private Creature rideTarget;
    private bool riding = false;

    private void Awake()
    {
        MoveSpeed = 250;
    }

    private void Update()
    {
        if (riding)
            transform.position = rideTarget.transform.position;
    }

    public void RideClosestCreature()
    {
        if (riding)
            riding = false;
        else
        {
            List<Creature> creatures = Area.GetAllCreatures();
            int size = creatures.Count;
            Creature closestCreature = null;
            float closestDistance = float.MaxValue;
            for(int count = 0; count < size; count++)
            {
                float distance = Vector3.Distance(transform.position, creatures[count].transform.position);
                if (distance < closestDistance)
                {
                    closestCreature = creatures[count];
                    closestDistance = distance;
                }
            }
            if (closestCreature != null)
            {
                rideTarget = closestCreature;
                riding = true;
            }
        }
    }
}
