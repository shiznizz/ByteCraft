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
    

    public bool isSelected;
    public bool isFull;

    [SerializeField] equipSlot headSlot, chestSlot, legSlot, 
                     gloveSlot, primaryWeapon, secondaryWeapon, specialWeapon;
    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
           if (isSelected)
           {
                gameManager.instance.deselectSlot();
            }
               // gameManager.instance.deselectItem();
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
                equipGear();
                gameManager.instance.deselectSlot();
            }
        }
    }


    public void equipGear()
    {
        weapon = item.GetWeapon();

        if (item.itemTypye == itemSO.itemType.Head && !headSlot.isFull)
        { 
            headSlot.equipGear(item); 
            headSlot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Chest && !chestSlot.isFull)
        {
            chestSlot.equipGear(item);
            chestSlot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Hands && !gloveSlot.isFull)
        {
            gloveSlot.equipGear(item);
            gloveSlot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Legs && !legSlot.isFull)
        {
            legSlot.equipGear(item);
            legSlot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Weapon && weapon.type == weaponStats.weaponType.primary && !primaryWeapon.isFull) 
        {
            primaryWeapon.equipGear(item);
            primaryWeapon.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Weapon && weapon.type == weaponStats.weaponType.secondary && !secondaryWeapon.isFull)
        {
            secondaryWeapon.equipGear(item);
            secondaryWeapon.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Weapon && weapon.type == weaponStats.weaponType.special && !specialWeapon.isFull)
        {
            specialWeapon.equipGear(item);
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
    }

    public void clearSlot()
    {
        inventoryManager.instance.removeItem(item);
    }
}
