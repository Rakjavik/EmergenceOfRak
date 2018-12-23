using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class RAKCarouselMenu : MonoBehaviour
{
    public interface MenuItem
    {
        bool isAvailableToDisplay();
        GameObject getMini();
    }
    public MenuItem[] menuItems;
    public float multiplyScaleBy = 1f;
    public float speed = 300f;
    public float highlightedScale = 2f;

    private GameObject[] minis;
    private bool busy = false;
    private bool next;
    private int currentIndex = 0;
    private float step;
    private bool initialized = false;
    private GameObject container;
    private float moved = 0;

    // Use this for initialization
    public void Initialize(MenuItem[] menuItemsAll)
    {
        List<MenuItem> availableItems = new List<MenuItem>();
        foreach(MenuItem item in menuItemsAll)
        {
            if(item.isAvailableToDisplay())
            {
                availableItems.Add(item);
            }
        }
        this.menuItems = availableItems.ToArray();
        container = new GameObject("CarouselMenu");
        container.transform.SetParent(transform);
        container.transform.position = transform.position;
        container.transform.rotation = transform.rotation;
        minis = new GameObject[menuItems.Length];
        step = 360 / minis.Length;
        for (int count = 0; count < minis.Length; count++)
        {
            minis[count] = GameObject.Instantiate(menuItems[count].getMini());
            minis[count].transform.localScale = minis[count].transform.localScale * multiplyScaleBy;
            minis[count].transform.SetParent(container.transform);
            minis[count].transform.position = container.transform.position;
            minis[count].transform.rotation = container.transform.rotation;
            minis[count].transform.localPosition += -minis[count].transform.forward * .15f;
            minis[count].transform.RotateAround(container.transform.position, container.transform.up, step * count);
        }
        highlightSelection(true);
        initialized = true;
    }
    private void Update()
    {
        if (!initialized) { return; }
        if (busy)
        {
            if (moved > step)
            {
                highlightSelection(true);
                busy = false;
                moved = 0;
            }
            for(int count = 0; count < minis.Length; count++)
            {
                if(next)
                {
                    minis[count].transform.RotateAround(transform.position, minis[count].transform.up, Time.deltaTime*speed);
                }
                else
                {
                    minis[count].transform.RotateAround(transform.position, -minis[count].transform.up, Time.deltaTime*speed);
                }
            }
            moved += Time.deltaTime * speed;
        }
        else
        {
            minis[currentIndex].transform.Rotate(container.transform.up, Time.deltaTime * speed/5);
        }
    }
    public void moveTo(bool next)
    {
        if(busy) { return; }

        busy = true;
        this.next = next;
        highlightSelection(false);
        if(next)
        {
            currentIndex--;
        }
        else
        {
            currentIndex++;
        }
        if (currentIndex > minis.Length-1)
        {
            currentIndex = 0;
        }
        else if (currentIndex < 0)
        {
            currentIndex = minis.Length-1;
        }
    }
    private void highlightSelection(bool highlight)
    {
        if(highlightedScale == 1) { return; }
        if(highlight)
        {
            minis[currentIndex].transform.localScale = minis[currentIndex].transform.localScale * 2.0f;
        }
        else
        {
            minis[currentIndex].transform.localScale = minis[currentIndex].transform.localScale / 2.0f;
        }
    }
    public void close()
    {
        if (initialized)
        {
            foreach (GameObject go in minis)
            {
                Destroy(go);
            }
        }
        Destroy(container);
        Destroy(this);
    }
    public MenuItem getSelection()
    {
        return menuItems[currentIndex];
    }
}
