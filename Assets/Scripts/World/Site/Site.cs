using UnityEngine;
using System.Collections;
using rak;
using rak.creatures;
using System.Collections.Generic;
using rak.world;

public class Site
{
    public string SiteName { get; private set; }
    public Thing[] OwnedItems { get; private set; }
    public Creature[] CurrentResidents { get; private set; }
    public Dictionary<ResourceType,int> CurrentResources { get; private set; }

    public Site(string siteName)
    {
        this.SiteName = siteName;
        CurrentResources = new Dictionary<ResourceType, int>();
    }

    public void IncrementResource(ResourceType resource, int amount)
    {
        if(amount > 0)
            CurrentResources[resource] += amount;
    }
    public bool HasAtLeast(ResourceType resource,int amount)
    {
        return CurrentResources[resource] >= amount;
    }
    public void DecrementResource(ResourceType resource, int amount)
    {
        if (amount > 0)
        {
            if (CurrentResources[resource] - amount > 0)
                CurrentResources[resource] -= amount;
            else
                Debug.LogError("Subracting this amount brings below 0 - " + amount + "-" + resource);
        }
    }

    public void SetOwnedItems(Thing[] ownedItems)
    {
        OwnedItems = ownedItems;
    }
    public void SetCurrentResidents(Creature[] currentResidents)
    {
        this.CurrentResidents = currentResidents;
    }
}
