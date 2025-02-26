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
            //if (isSelected)
            //    gameManager.instance.deselectItem();
            //else if (!isSelected && isFull)
            
            if (isFull && !isSelected)
            {
                if (gameManager.instance.selectedEquipSlot != null)
                {
                    gameManager.instance.selectedEquipSlot.transform.GetChild(1).gameObject.SetActive(false);
                    gameManager.instance.selectedEquipSlot.GetComponent<equipSlot>().isSelected = false;
                }
                Debug.Log("1");
                selectItem(eventData);
            }
            else if (isSelected)
            {
                selected.SetActive(false);
                isSelected = false;
            }


        }
    }

    public void selectItem(PointerEventData eventData)
    {
        //gameManager.instance.deselectItem();
        Debug.Log("2");
        gameManager.instance.selectedEquipSlot = null;
        gameManager.instance.selectedEquipSlot = eventData.pointerClick;
        
        selected.SetActive(true);
        isSelected = true;
        //displaySlot.SetActive(true);
        //itemDescription.text = item.itemDescription;
        //itemName.text = item.itemName;
        //itemIcon.sprite = item.itemIcon;
    }
}
