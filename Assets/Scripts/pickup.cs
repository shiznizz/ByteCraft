using Unity.VisualScripting;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] gunStats gun;

    public enum LootType {Health,Gun,Ammo,Fuel}
    public LootType lootType;
    public int amount; // how much value the loot gives to player

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // check if player touches loot
        {
            IPickup player = other.GetComponent<IPickup>();
            //playerController player = other.GetComponent<playerController>();
            if (player != null) 
            {
                switch (lootType)
                {
                    case pickup.LootType.Health:
                        player.heal(amount);
                        break;
                    case pickup.LootType.Gun:
                        player.getGunStats(gun);
                        break;
                    case pickup.LootType.Ammo:
                        break;
                    case pickup.LootType.Fuel:
                        break;
                }
                Destroy(gameObject); // remove loot from scene
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lootType == LootType.Gun)
        {
            gun.ammoCur = gun.ammoMax;
            gun.ammoReserve = gun.ammoReserveMax;
        }
    }
}
