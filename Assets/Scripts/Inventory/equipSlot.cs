using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class equipSlot : MonoBehaviour
{

    [SerializeField] itemSO item;

    [SerializeField] Image itemIcon;

    [SerializeField] GameObject equippedSlot;

    public bool isFull;

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
}
