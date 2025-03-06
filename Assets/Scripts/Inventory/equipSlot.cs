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

    public weaponStats weapon;
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

        if (item.itemTypye == itemSO.itemType.Weapon)
        {
            weapon = item.GetWeapon();
            inventoryManager.instance.weaponList.Add(weapon);
            indexSlots();
        }
        
        inventoryManager.instance.removeItem(item);
    }

    public void unequipGear(itemSO item)
    {
        
        inventoryManager.instance.addItem(item);

        if (item.itemTypye == itemSO.itemType.Weapon)
        {
            if (gameManager.instance.player.GetComponent<playerController>().weaponListPos == weaponSlotIndex)
            {
                gameManager.instance.player.GetComponent<playerController>().removeWeaponUI();
                Debug.Log("weapon POS Check");
            }

            if (gameManager.instance.player.GetComponent<playerController>().weaponListPos > 0)
                gameManager.instance.player.GetComponent<playerController>().weaponListPos = gameManager.instance.player.GetComponent<playerController>().weaponListPos - 1;
            else
                gameManager.instance.player.GetComponent<playerController>().weaponListPos = 0;

            weapon = item.GetWeapon();
            inventoryManager.instance.weaponList.Remove(weapon);

            indexSlots();
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

    public void indexSlots()
    {
       // for (int i = 0; i < inventoryManager.instance.weaponList.Count ; i++)
       // {
            weaponSlotIndex = inventoryManager.instance.weaponList.FindIndex(weapon.Equals);
       // }
    }
}
