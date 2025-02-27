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
        for(int i = 0; i < numItems; i++)
        {
            Instantiate(lootTable[i].itemModel, dropPositions[i].position, dropPositions[i].rotation);
        }

        Destroy(gameObject);
    }
}
