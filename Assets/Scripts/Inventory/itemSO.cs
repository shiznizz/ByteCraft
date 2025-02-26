using UnityEngine;

// making the class abstract allows us to add multple types of items into a single container
public abstract class itemSO : ScriptableObject
{
    [Header("Item")] // data shared across all items
    public string itemName;
    [TextArea]
    public string itemDescription;

    public Sprite itemIcon;

    public itemType itemTypye;

    public enum itemType
    {
        Weapon,
        Head,
        Chest,
        Hands,
        Legs
    }

    public abstract itemSO GetItem();
    public abstract ArmorSO GetArmor();
    public abstract weaponStats GetWeapon();
    
}
