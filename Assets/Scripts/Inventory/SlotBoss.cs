using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotBoss : MonoBehaviour, IPointerClickHandler
{
    public itemSO item;

    public Image itemIcon;
   
    public TMP_Text itemDescription;
    public TMP_Text itemName;

    public GameObject selectedSlot;
    public GameObject displaySlot;

    public bool isSelected;
    public bool isFull;

    [SerializeField] equipSlot headSlot, chestSlot, legSlot, 
                     gloveSlot, weapon1Slot, weapon2Slot, weapon3Slot;
    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
           if (isSelected)
                gameManager.instance.deselectItem();
            else if (!isSelected && isFull)    
                selectItem();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isFull)
            {
                equipGear();
                gameManager.instance.deselectItem();
            }
        }

    }

    public void equipGear()
    {
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

        else if (item.itemTypye == itemSO.itemType.Weapon && !weapon1Slot.isFull)
        {
            weapon1Slot.equipGear(item);
            weapon1Slot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Weapon && !weapon2Slot.isFull)
        {
            weapon2Slot.equipGear(item);
            weapon2Slot.isFull = true;
        }

        else if (item.itemTypye == itemSO.itemType.Weapon && !weapon3Slot.isFull)
        {
            weapon3Slot.equipGear(item);
            weapon3Slot.isFull = true;
        }
    }

    public void selectItem()
    {
        gameManager.instance.deselectItem();
        selectedSlot.SetActive(true);
        isSelected = true;
        displaySlot.SetActive(true);
        itemDescription.text = item.itemDescription ;
        itemName.text = item.itemName;
        itemIcon.sprite = item.itemIcon;
    }

    public void clearSlot()
    {
        inventoryManager.instance.removeItem(item);
    }

    
}
