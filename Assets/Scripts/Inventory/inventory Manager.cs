using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class inventoryManager : MonoBehaviour
{
    public static inventoryManager instance;

    public List<itemSO> inventory = new List<itemSO>();

    public List<weaponStats> weaponList = new List<weaponStats>();

    public weaponStats equippedWeapon;

    //private int weaponIndex;

    

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

    public void currentWeapon()
    {
        Debug.Log("CW");
        try
        {
            Debug.Log("CW if " + gameManager.instance.player.GetComponent<playerController>().weaponListPos);
            //weaponIndex = gameManager.instance.player.GetComponent<playerController>().weaponListPos;
            equippedWeapon = weaponList[gameManager.instance.player.GetComponent<playerController>().weaponListPos];
        }
        catch
        {
            Debug.Log("CW else");
            equippedWeapon = null;
        }
            
    }
}

