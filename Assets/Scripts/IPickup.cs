using UnityEngine;

public interface IPickup
{
    void PickupLoot(pickup.LootType type, int amount);

    void heal(int amount);

    void getGunStats(gunStats gun);
}
