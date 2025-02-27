using UnityEngine;

public interface IPickup
{
    void PickupLoot(pickup.LootType type, int amount);

    void heal(int amount);

    void getArmor(int amount);
    void getAmmo(int amount);
    void refillFuel(int amount);

   // void getWeaponStats(weaponStats weapon);

    void addInventory(itemSO item);
}
