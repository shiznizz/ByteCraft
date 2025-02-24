using UnityEngine;
using System.Collections.Generic;

public class lootChest : MonoBehaviour, lootDrop
{
    [SerializeField] int numItems;
    [SerializeField] List<LootItem> lootTable;
    [SerializeField] List<Transform> dropPositions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void dropLoot()
    {
        Debug.Log("Running the dropLoot function...");

        Debug.Log("Inside of drop loot.");
        for(int i = 0; i < numItems; i++)
        {
            Debug.Log("Inside of the for statement");
            LootItem item = lootTable[i];
            Transform pos = dropPositions[i];
            Instantiate(item.itemModel, pos.position, pos.rotation);
            Debug.Log("Instantiated item");
        }
/*        foreach (LootItem loot in lootTable) 
        {
            Debug.Log("Inside of the foreach statement");
            Instantiate(loot.itemModel, dropPositions[posIdx].position, dropPositions[posIdx].rotation);

        }*/

        Destroy(gameObject);
        Debug.Log("Chest destroyed");
    }


}
