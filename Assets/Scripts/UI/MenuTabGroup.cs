using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTabGroup : MonoBehaviour
{
    public List<MenuTabButton> tabButtons = new List<MenuTabButton>();
    public List<GameObject> tabObjects = new List<GameObject>();
    public Sprite buttonDefault;
    public Sprite buttonSelected;
    public Sprite buttonHover;

    private MenuTabButton selectedButton;

    void Start()
    {
        selectedButton = tabButtons[0];
    }

    public void OnTabEnter(MenuTabButton button)
    {
        ResetTabs();
        if (button != selectedButton) 
            button.background.sprite = buttonHover;
    }

    public void OnTabExit(MenuTabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelect(MenuTabButton button)
    {
        selectedButton = button;
        ResetTabs();
        button.background.sprite = buttonSelected;

        int selectedIndex = button.transform.GetSiblingIndex();
        for (int index = 0; index < tabObjects.Count; index++)
        {
            if (index == selectedIndex)
                tabObjects[index].SetActive(true);
            else
                tabObjects[index].SetActive(false);
        }
    }

    public void ResetTabs()
    {
        foreach (MenuTabButton button in tabButtons)
        {
            if (button != selectedButton)
                button.background.sprite = buttonDefault;
        }
    }
}
