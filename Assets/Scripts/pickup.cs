using Unity.VisualScripting;
using UnityEngine;

public class pickup: MonoBehaviour
{
    public enum LootType {Health}
    public LootType lootType;
    public int amount; // how much value the loot gives to player

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // check if player touches loot
        {
            playerController player = other.GetComponent<playerController>();
            if (player != null) 
            {
                player.PickupLoot(this.lootType, amount);   
                Destroy(gameObject); // remove loot from scene
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
