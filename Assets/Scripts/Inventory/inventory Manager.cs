using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class inventoryManager : MonoBehaviour
{
    public static inventoryManager instance;

    public List<itemSO> inventory = new List<itemSO>();

    public List<weaponStats> weaponList = new List<weaponStats>();

    public weaponStats equippedWeapon;

    private void Awake()
    {
        instance = this;
    }
    // adds item to inventory
    public void addItem(itemSO item)
    {
        inventory.Add(item);
        gameManager.instance.updateInventory();
    }
    // removes item from inventory
    public void removeItem(itemSO item)
    {
        inventory.Remove(item);
        gameManager.instance.updateInventory();
    }
    // track the current equipped weapon
    public void currentEquippedWeapon()
    {
        try
        {
            equippedWeapon = weaponList[gameManager.instance.player.GetComponent<playerController>().weaponListPos];
            gameManager.instance.player.GetComponent<playerController>().changeGun();
        }
        catch
        {
            equippedWeapon = null;  
        }
        
    }
    // change weapon POS
    public void changeWeaponPOS()
    {
            if (gameManager.instance.player.GetComponent<playerController>().weaponListPos - 1 < 0)
            {
                gameManager.instance.player.GetComponent<playerController>().weaponListPos = 0;
            }
            else
            {
                gameManager.instance.player.GetComponent<playerController>().weaponListPos--;
            }
        currentEquippedWeapon();
    }
}

