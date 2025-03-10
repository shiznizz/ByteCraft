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
            inventoryManager.instance.weaponList.Insert(weaponSlotIndex, weapon);
            gameManager.instance.player.GetComponent<playerController>().getWeaponStats();
        }

        inventoryManager.instance.removeItem(item);
    }

    public void unequipGear(itemSO item)
    {
        // adds item back to the inventory
        inventoryManager.instance.addItem(item);
        // check if item to unequip is a weapon
        if (item.itemTypye == itemSO.itemType.Weapon)
        {    // check if current weapon is the one to remove

            if (gameManager.instance.player.GetComponent<playerController>().weaponListPos == weaponSlotIndex || inventoryManager.instance.weaponList.Count == 1)
            {
                // remove current weapons UI and Visual

                gameManager.instance.player.GetComponent<playerController>().removeWeaponUI();

                Debug.Log("if 1");

            }
            else
            {
                // moves current weapon POS to where the current weapon is after the removal of the weapon
                Debug.Log("else 1");
                gameManager.instance.player.GetComponent<playerController>().weaponListPos = gameManager.instance.player.GetComponent<playerController>().weaponListPos - 1;
            }

            // get weapon
            weapon = item.GetWeapon();
            // remove weapon
            inventoryManager.instance.weaponList.Remove(weapon);

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
}
