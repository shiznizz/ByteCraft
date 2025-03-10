
using System.Net;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class equipSlot : MonoBehaviour, IPointerClickHandler
{
    public itemSO item;

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

        
        inventoryManager.instance.removeItem(item);
    }

    public void unequipGear(itemSO item)
    {
        // adds item back to the inventory
        inventoryManager.instance.addItem(item);
        // check if item to unequip is a weapon
        if (item.itemTypye == itemSO.itemType.Weapon)
        {   
            weapon = item.GetWeapon();
           
            // checks if weapon to remove is the current weapon equipped
            if (item == inventoryManager.instance.equippedWeapon)
            {
                // remove current weapons UI and Visual
                gameManager.instance.player.GetComponent<playerController>().removeWeaponUI();
            }
            // remove weapon then change weapon POS to make sure we dont go out of bounds
            inventoryManager.instance.weaponList.Remove(weapon);
            inventoryManager.instance.changeWeaponPOS();
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

}
