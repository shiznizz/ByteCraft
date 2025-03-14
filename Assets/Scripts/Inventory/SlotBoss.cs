using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotBoss : MonoBehaviour, IPointerClickHandler
{
    public itemSO item;
    public weaponStats weapon;
    
    public GameObject selectedSlot;

    public itemSO selectedItem;

    public bool isSelected;
    public bool isFull;


    public equipSlot headSlot, chestSlot, legSlot, 
                     gloveSlot, primaryWeapon, secondaryWeapon, specialWeapon;

    public void LateUpdate()
    {
        if (selectedItem != null && Input.GetButtonDown("Delete"))
        {
            deleteItem();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
           if (isSelected)
           {
                gameManager.instance.deselectSlot();
           }
            else if (!isSelected && isFull)
            {
                if (gameManager.instance.selectedInventorySlot != null || gameManager.instance.selectedEquipSlot != null)
                {
                    gameManager.instance.deselectSlot();
                }
                selectItem(eventData);
            }
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isFull)
            {
                
                equipGear(item);
                gameManager.instance.deselectSlot();
            }
        }
    }


    public void equipGear(itemSO gear)
    {
        weapon = gear.GetWeapon();
        //Debug.Log(weapon);

        if (gear.itemTypye == itemSO.itemType.Head && !headSlot.isFull)
        { 
            headSlot.equipGear(gear); 
            headSlot.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Chest && !chestSlot.isFull)
        {
            chestSlot.equipGear(gear);
            chestSlot.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Hands && !gloveSlot.isFull)
        {
            gloveSlot.equipGear(gear);
            gloveSlot.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Legs && !legSlot.isFull)
        {
            legSlot.equipGear(gear);
            legSlot.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Weapon && weapon.wepType == weaponStats.weaponType.primary && !primaryWeapon.isFull) 
        {
            primaryWeapon.equipGear(gear);
            primaryWeapon.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Weapon && weapon.wepType == weaponStats.weaponType.secondary && !secondaryWeapon.isFull)
        {
            secondaryWeapon.equipGear(gear);
            secondaryWeapon.isFull = true;
        }

        else if (gear.itemTypye == itemSO.itemType.Weapon && weapon.wepType == weaponStats.weaponType.special && !specialWeapon.isFull)
        {
            specialWeapon.equipGear(gear);
            specialWeapon.isFull = true;
        }
    }

    public void selectItem(PointerEventData eventData)
    {
        gameManager.instance.selectedInventorySlot = null;
        gameManager.instance.selectedInventorySlot = eventData.pointerClick;

        selectedSlot.SetActive(true);
        isSelected = true;
        gameManager.instance.displaySlot.SetActive(true);

        gameManager.instance.itemDescription.text = item.itemDescription ;
        gameManager.instance.itemName.text = item.itemName;
        gameManager.instance.itemIcon.sprite = item.itemIcon;

        selectedItem = item;

    }

    public void clearSlot()
    {
        inventoryManager.instance.removeItem(item);
    }

    public void deleteItem()
    {
        gameManager.instance.deselectSlot();

        gameManager.instance.selectedInventorySlot = null;

        isSelected = false;
        isFull = false;
        gameManager.instance.displaySlot.SetActive(false);

        gameManager.instance.itemDescription.text = null;
        gameManager.instance.itemName.text = null;
        gameManager.instance.itemIcon.sprite = null;

        

        inventoryManager.instance.inventory.Remove(selectedItem);

        StartCoroutine(deletePopUp());

        selectedItem = null;

        gameManager.instance.updateInventory();

        
    }

    IEnumerator deletePopUp()
    {
        
        gameManager.instance.deleteNotifaction.text = selectedItem.itemName + " Deleted";
        yield return new WaitForSecondsRealtime(1.5f);
        gameManager.instance.deleteNotifaction.text = " ";
    }
}
