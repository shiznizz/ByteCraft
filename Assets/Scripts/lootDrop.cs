using UnityEngine;
using UnityEngine.UIElements;

public class lootDrop : MonoBehaviour
{

    [SerializeField] Transform dropPos;
    [SerializeField] GameObject lootItem;

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
        Instantiate(lootItem, dropPos);
    }
}
