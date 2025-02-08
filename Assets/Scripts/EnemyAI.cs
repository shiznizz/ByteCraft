using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] int HP;

    Color colorOrig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void takeDamage(int amount)
    {
        HP -= amount; // subtract damage amount from health

        StartCoroutine(flashRed()); // begins the flash of red

        if (HP <= 0) // check if the enemy is dead
        {
            Destroy(gameObject); // get rid of dead enemy

        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red; // sets the color to red
        yield return new WaitForSeconds(0.1f); // flashes for only a second
        model.material.color = colorOrig; // returns to original color
    }
}

