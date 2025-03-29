using Unity.VisualScripting;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] itemSO item;

    public enum LootType {Health, Weapon, armor, Ammo, Fuel, Upgrade, Shield}
    public LootType lootType;
    public int amount; // how much value the loot gives to player
    public LootItem lootItem;
    [SerializeField] AudioClip gunPickupSound;
    [SerializeField] AudioClip healthPickupSound;
    [SerializeField] AudioClip ammoPickupSound;
    [SerializeField] AudioClip shieldPickupSound;
    [SerializeField] AudioClip upgradePickupSound;
    [SerializeField] AudioClip fuelPickupSound;

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
                        playerAudSource.PlayOneShot(healthPickupSound);
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
                        playerAudSource.PlayOneShot(fuelPickupSound);
                        player.addInventory(item);
                        break;
                    case pickup.LootType.Shield:
                        playerAudSource.PlayOneShot(shieldPickupSound);
                        AddShield();
                        break;
                    case pickup.LootType.Ammo:
                        playerAudSource.PlayOneShot(ammoPickupSound);
                        AddAmmo();
                        break;
                    case pickup.LootType.Upgrade:
                        playerAudSource.PlayOneShot(upgradePickupSound);
                        HandleUpgrade();
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

    void AddAmmo()
    {
        if (inventoryManager.instance.weaponList.Count == 0)
            return;

        amount = lootItem.restoreAmt;
        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();

        // if the current gun's ammo reserve is equal to the max reserve ammo, return
        if (gun.ammoReserve == gun.ammoReserveMax) return;

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
    }

    void AddShield()
    {
        amount = lootItem.restoreAmt;
        if(playerStatManager.instance.shield <= playerStatManager.instance.shieldOverChargeMax)
            playerStatManager.instance.shield = playerStatManager.instance.shield + amount;
        //Debug.Log($"Current shield amt: {playerStatManager.instance.shield}, incoming shield amt: {amount}, shield max: {playerStatManager.instance.shieldMax}");
        
        // if the current shield amt is equal to the max shield amt, return
/*        if (playerStatManager.instance.shield == playerStatManager.instance.shieldMax) return;

        // if adding the shield amt would be greater than the max shield amt, only add enough to hit that max
        if ((playerStatManager.instance.shield + amount) > playerStatManager.instance.shieldMax)
        {
            float amtToAdd = playerStatManager.instance.shieldMax - playerStatManager.instance.shield;
            playerStatManager.instance.shield += amtToAdd;
        }
        // otherwise, add regularly
        else if ((playerStatManager.instance.shield + amount) <= playerStatManager.instance.shieldMax)
        {
            playerStatManager.instance.shield += amount;
        }*/

    }

    void HandleUpgrade()
    {
        if (lootItem.upgradeType == upgradeType.Armor)
        {
            playerStatManager.instance.shield += lootItem.restoreAmt;
        }
        else if (lootItem.upgradeType == upgradeType.Damage)
        {
            playerStatManager.instance.attackDamage += lootItem.restoreAmt;
        }
        else if (lootItem.upgradeType == upgradeType.Sprint)
        {
            playerStatManager.instance.sprintSpeed += lootItem.restoreAmt;
        }
    }
}
