using Unity.VisualScripting;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] itemSO item;

    public enum LootType {Health, Weapon, armor, Ammo, Fuel, Upgrade}
    public LootType lootType;
    public int amount; // how much value the loot gives to player
    public LootItem lootItem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // check if player touches loot
        {
            IPickup player = other.GetComponent<IPickup>();
            if (player != null) 
            {
                switch (lootType)
                {
                    case pickup.LootType.Health:
                        player.heal(amount);
                        break;
                    case pickup.LootType.Weapon:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.armor:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Fuel:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Ammo:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Upgrade:
                        Debug.Log($"Picked up upgrade item: {lootItem.name} - {lootItem.upgradeType}");
                        break;
                }
                Destroy(gameObject); // remove loot from scene
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (lootType == LootType.Weapon && weapon.type == weaponStats.weaponType.Gun)
        //{
        //    weapon.gun.ammoCur = weapon.gun.ammoMax;
        //    weapon.gun.ammoReserve = weapon.gun.ammoReserveMax;
        //}
    }
}
