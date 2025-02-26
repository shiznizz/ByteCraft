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


    public bool isFull;
    public bool isSelected;

    private weaponStats gun;

    public void Awake()
    {
        
    }

    public void equipGear(itemSO item)
    {
        this.item = item;
        itemIcon.sprite = item.itemIcon;
        equippedSlot.SetActive(true);

        if (item.itemTypye == itemSO.itemType.Weapon)
        {
           
            gun = item.GetWeapon();

            inventoryManager.instance.weaponList.Add(gun);

        }


        inventoryManager.instance.removeItem(item);

    }

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
    }

    public void selectItem(PointerEventData eventData)
    {
       
        
        gameManager.instance.selectedEquipSlot = null;
        gameManager.instance.selectedEquipSlot = eventData.pointerClick;
        
        selected.SetActive(true);
        isSelected = true;
    }
}
