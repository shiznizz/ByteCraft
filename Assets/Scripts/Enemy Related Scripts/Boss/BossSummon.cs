using UnityEngine;

public class BossSummon : MonoBehaviour
{
    public GameObject enemyPrefab; // The enemy prefab to spawn
    public GameObject[] spawnPoints; // Array to hold the boss enemy spawn points
    public int maxEnemies = 1; // How many enemies to spawn at each location (set to 1 here, but you can adjust)
    public float summonInterval = 5f; // Time in seconds between summons

    private float summonTimer;

    private void Start()
    {
        summonTimer = summonInterval;
        this.enabled = false; // Start this with the BossSummon script disabled (is triggered in the BossFightMangager Script)
    }

    private void Update()
    {
        summonTimer -= Time.deltaTime;

        if (summonTimer < 0)
        {
            SummonEnemies();
            summonTimer = summonInterval; // Reset the timer
        }
    }

    void SummonEnemies()
    {
        // Loop through the spawn points and spawn enemies at each one
        foreach (GameObject spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Vector3 spawnPosition = spawnPoint.transform.position; // Get the position of the spawn point

                for (int i = 0; i < maxEnemies; i++)
                {
                    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);                    
                }
            }
        }
    }
}
