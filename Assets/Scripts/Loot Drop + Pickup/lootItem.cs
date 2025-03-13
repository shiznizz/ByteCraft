using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu]

public class LootItem : ScriptableObject
{
    public string itemName;
    public GameObject itemModel;
    [Range(0f, 100f)] public float dropChance;
    public itemType type;
    public int restoreAmt; // specifically for armor, health, ammo, fuel, anything we may want to have
                           // various tiers of, like a health potion type that restores a little and another
                           // that restores a lot. If not applicable, set to 0.
}

public enum itemType
{
    HP,
    Armor,
    Gun,
    Melee,
    Pet,
    Ammo,
    Fuel,
    Key
}