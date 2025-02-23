using Unity.VisualScripting;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] weaponStats weapon;

    public enum LootType {Health, Weapon}
    public LootType lootType;
    public int amount; // how much value the loot gives to player

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
                        player.getWeaponStats(weapon);
                        break;
                }
                Destroy(gameObject); // remove loot from scene
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lootType == LootType.Weapon && weapon.type == weaponStats.weaponType.Gun)
        {
            weapon.gun.ammoCur = weapon.gun.ammoMax;
            weapon.gun.ammoReserve = weapon.gun.ammoReserveMax;
        }
    }
}
