using Unity.VisualScripting;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] itemSO item;

    public enum LootType {Health, Weapon, armor, Ammo, Fuel, Upgrade}
    public LootType lootType;
    public int amount; // how much value the loot gives to player
    public LootItem lootItem;
    [SerializeField] AudioClip gunPickupSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // check if player touches loot
        {
            IPickup player = other.GetComponent<IPickup>();
            AudioSource playerAudSource = other.GetComponent<AudioSource>();
            if (player != null) 
            {
                switch (lootType)
                {
                    case pickup.LootType.Health:
                        player.heal(amount);
                        break;
                    case pickup.LootType.Weapon:
                        playerAudSource.PlayOneShot(gunPickupSound);
                        player.addInventory(item);
                        break;
                    case pickup.LootType.armor:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Fuel:
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Ammo:
                        amount = lootItem.restoreAmt;
                        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();
                        
                        // if the current gun's ammo reserve is equal to the max reserve ammo, return
                        if (gun.ammoReserve == gun.ammoReserveMax) break;

                        // if adding the ammo amt would be greater than the reserve ammo max, only add enough to hit that max
                        if ((gun.ammoReserve + amount) > gun.ammoReserveMax)
                        {
                            int amtToAdd = gun.ammoReserveMax - gun.ammoReserve;
                            gun.ammoReserve += amtToAdd;

                        }
                        // otherwise, add regularly
                        else if ((gun.ammoReserve + amount) <= gun.ammoReserveMax)
                        {
                            gun.ammoReserve += amount;
                        }
                        gameManager.instance.updateAmmo();
                        break;
                    case pickup.LootType.Upgrade:
                        if (lootItem.upgradeType == upgradeType.Armor)
                        {
                            playerStatManager.instance.shield += lootItem.restoreAmt;
                        } else if (lootItem.upgradeType == upgradeType.Damage)
                        {
                            playerStatManager.instance.attackDamage += lootItem.restoreAmt;
                        } else if (lootItem.upgradeType == upgradeType.Sprint)
                        {
                            playerStatManager.instance.sprintSpeed += lootItem.restoreAmt;
                        }
                        break;
                }
                Destroy(this.gameObject); // remove loot from scene
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
