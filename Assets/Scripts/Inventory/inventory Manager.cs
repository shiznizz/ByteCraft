using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class inventoryManager : MonoBehaviour, IPersistData
{
    public static inventoryManager instance;

    public List<itemSO> inventory = new List<itemSO>();

    public List<weaponStats> weaponList = new List<weaponStats>();

    public int weaponListPos;

    public weaponStats equippedWeapon;

    public GameObject inventorySlot;
    public SlotBoss slotBossScript;

    private weaponStats weapon;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        slotBossScript = inventorySlot.GetComponent<SlotBoss>();
    }

    // adds item to inventory
    public void addItem(itemSO item)
    {
        if (item.itemTypye == itemSO.itemType.Weapon)
        {
            weapon = item.GetWeapon();
            weapon.RefreshAmmo();
        }

        if (weapon.wepType == weaponStats.weaponType.primary && !slotBossScript.primaryWeapon.isFull)
        {
            //Debug.Log("7");
            slotBossScript.equipGear(item);
        }
        else if (weapon.wepType == weaponStats.weaponType.secondary && !slotBossScript.secondaryWeapon.isFull)
        {
            //Debug.Log("8");
            slotBossScript.equipGear(item);
        }
        else if(weapon.wepType == weaponStats.weaponType.special && !slotBossScript.specialWeapon.isFull)
        {
            //Debug.Log("9");
            slotBossScript.equipGear(item);
        }
        else
        {
            //Debug.Log("2");
            inventory.Add(item);
        }
        //Debug.Log("3");
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
            equippedWeapon = weaponList[weaponListPos];
            gameManager.instance.player.GetComponent<playerAttack>().changeGun();
        }
        catch
        {
            equippedWeapon = null;  
        }
        
    }
    // change weapon POS
    public void changeWeaponPOS()
    {
            if (weaponListPos - 1 < 0)
            {
                weaponListPos = 0;
            }
            else
            {
                weaponListPos--;
            }
        currentEquippedWeapon();
    }

    public weaponStats returnCurrentWeapon()
    {
        return weaponList[weaponListPos];
    }

    public void SaveData(ref gameData data)
    {
        data.playerWeapons = this.weaponList;
        data.playerInventory = this.inventory;
        data.weaponPos = this.weaponListPos;
        
    }

    public void LoadData(gameData data)
    {
        this.weaponList = data.playerWeapons;
        this.inventory = data.playerInventory;
        this.weaponListPos = data.weaponPos;
        
        OnLoad();
    }

    public void OnLoad()
    {
        gameManager.instance.updateInventory();
        clearEmptyInventory();

        if (weaponList.Count > 0)
        {

        foreach (weaponStats weapon in  weaponList)
        {
                switch(weapon.wepType)
                {
                    case weaponStats.weaponType.primary:
                        inventorySlot.GetComponent<SlotBoss>().primaryWeapon.onLoad(weapon);
                        inventorySlot.GetComponent<SlotBoss>().primaryWeapon.isFull = true;
                    break;

                    case weaponStats.weaponType.secondary:
                        inventorySlot.GetComponent<SlotBoss>().secondaryWeapon.onLoad(weapon);
                        inventorySlot.GetComponent<SlotBoss>().secondaryWeapon.isFull = true;
                    break;

                    case weaponStats.weaponType.special:
                        inventorySlot.GetComponent<SlotBoss>().specialWeapon.onLoad(weapon);
                        inventorySlot.GetComponent<SlotBoss>().specialWeapon.isFull = true;
                    break;
                }
            }
            playerAttack.instance.changeGun();
        }
    }

    void clearEmptyInventory()
    {
        for (int index = 0; index < weaponList.Count; index++)
        {
            if (weaponList[index] == null)
            {
                weaponList.RemoveAt(index);
            }
        }

        for (int index = 0; index < inventory.Count; index++)
        {
            if (inventory[index] == null)
            {
                inventory.RemoveAt(index);
            }
        }
    }
}

