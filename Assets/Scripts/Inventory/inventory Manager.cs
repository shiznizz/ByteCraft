using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public GameObject inventorySlot;
    public SlotBoss slotBossScript;

    private weaponStats weapon;

    private void Awake()
    {
        instance = this;
    }
    // adds item to inventory
    public void addItem(itemSO item)
    {
        if (item.itemTypye == itemSO.itemType.Weapon)
        {
            weapon = item.GetWeapon();
        }

        if (weapon.type == weaponStats.weaponType.primary && !inventorySlot.GetComponent<SlotBoss>().primaryWeapon.isFull)
        {
            Debug.Log("7");
            inventorySlot.GetComponent<SlotBoss>().equipGear(item);
        }
        else if (weapon.type == weaponStats.weaponType.secondary && !inventorySlot.GetComponent<SlotBoss>().secondaryWeapon.isFull)
        {
            Debug.Log("8");
            inventorySlot.GetComponent<SlotBoss>().equipGear(item);
        }
        else if(weapon.type == weaponStats.weaponType.special && !inventorySlot.GetComponent<SlotBoss>().specialWeapon.isFull)
        {
            Debug.Log("9");
            inventorySlot.GetComponent<SlotBoss>().equipGear(item);
        }
        else
        {
            Debug.Log("2");
            inventory.Add(item);
        }
        Debug.Log("3");
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

