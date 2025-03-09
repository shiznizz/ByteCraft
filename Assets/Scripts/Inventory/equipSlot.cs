
using System.Net;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class equipSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] itemSO item;

    [SerializeField] Image itemIcon;

    [SerializeField] GameObject equippedSlot;

    [SerializeField] GameObject selected;

    public int weaponSlotIndex;
    public bool isFull;
    public bool isSelected;

    public weaponStats currWeapon;
    public weaponStats weapon;
    int test;


    // detects left click
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isFull && !isSelected)
            {
                if (gameManager.instance.selectedEquipSlot != null || gameManager.instance.selectedInventorySlot != null)
                {
                    gameManager.instance.deselectSlot();
                }
                selectItem(eventData);
            }
            else if (isSelected)
            {
                gameManager.instance.deselectSlot();
            }
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isFull)
            {
                Debug.Log("1");
                unequipGear(item);
                gameManager.instance.deselectSlot();
            }
        }
    }

    public void equipGear(itemSO item)
    {
        this.item = item;
        itemIcon.sprite = item.itemIcon;
        equippedSlot.SetActive(true);

        
        weapon = item.GetWeapon();
        inventoryManager.instance.weaponList.Add(weapon);

        gameManager.instance.player.GetComponent<playerController>().getWeaponStats();

        inventoryManager.instance.currentWeapon();
        inventoryManager.instance.removeItem(item);
    }

    public void unequipGear(itemSO item)
    {
        Debug.Log("2");
        // adds item back to the inventory
        inventoryManager.instance.addItem(item);
        // check if item to unequip is a weapon
        if (item.itemTypye == itemSO.itemType.Weapon)
        {    // check if current weapon is the one to remove
            Debug.Log("3");
            //gameManager.instance.player.GetComponent<playerController>().weaponListPos == weaponSlotIndex || inventoryManager.instance.weaponList.Count == 1
            Debug.Log(item.name);
            //Debug.Log(inventoryManager.instance.equippedWeapon.name);
            if (item == inventoryManager.instance.equippedWeapon)
            {
                // remove current weapons UI and Visual
                Debug.Log("if 1");
                gameManager.instance.player.GetComponent<playerController>().removeWeaponUI();
            }
            

            // get weapon
            weapon = item.GetWeapon();
            // remove weapon
            inventoryManager.instance.weaponList.Remove(weapon);
            changeWeaponPOS();
            Debug.Log("4");
            inventoryManager.instance.currentWeapon();
            Debug.Log("5");
            
            // hides UI for guns
            gameManager.instance.hideAmmo();

        }
        isFull = false;
        equippedSlot.SetActive(false);
    }

    public void selectItem(PointerEventData eventData)
    {
        gameManager.instance.selectedEquipSlot = null;
        gameManager.instance.selectedEquipSlot = eventData.pointerClick;

        selected.SetActive(true);
        isSelected = true;

        gameManager.instance.displaySlot.SetActive(true);

        gameManager.instance.itemDescription.text = item.itemDescription;
        gameManager.instance.itemName.text = item.itemName;
        gameManager.instance.itemIcon.sprite = item.itemIcon;
    }

    private void changeWeaponPOS()
    {
        currWeapon = inventoryManager.instance.equippedWeapon;


        if (inventoryManager.instance.weaponList.Count > 0)
        {
            Debug.Log("change if");
            test = inventoryManager.instance.weaponList.IndexOf(currWeapon);

            gameManager.instance.player.GetComponent<playerController>().weaponListPos = test;
            
        }
        else if (inventoryManager.instance.weaponList.Count == 0)
        {
            Debug.Log("change else");
            gameManager.instance.player.GetComponent<playerController>().weaponListPos = 0;
        }
        Debug.Log(inventoryManager.instance.weaponList.IndexOf(inventoryManager.instance.equippedWeapon) + "Change");
    }
}
