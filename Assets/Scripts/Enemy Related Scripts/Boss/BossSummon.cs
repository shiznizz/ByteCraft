using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossSummon : MonoBehaviour
{
    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs; // The enemy prefab to spawn
    public GameObject[] spawnPoints; // Array to hold the boss enemy spawn points
    public int maxEnemies = 1; // How many enemies to spawn at each location (set to 1 here, but you can adjust)
    public float summonInterval = 5f; // Time in seconds between summons

    private float summonTimer;

    [Header("Bomb Settings")]
    public GameObject[] bombPrefabs; // Array for different bomb prefabs
    public GameObject[] bombSpawnPoints; // Array for different spawn points
    public int bombsToSpawn = 3; // Number of bombs that are spawned
    public float bombSpawnDelay = 0.5f; // Delay between bomb spawns
    public float summonIntervals = 5f; // Interval for periodic summoning of bombs

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

        //Trigger bomb spawning when the timer runs out
        if (summonTimer <= 0)
        {
            SummonBombs();
            summonTimer = summonInterval;  // Reset the summon timer
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
                    // Select a random enemy prefab from the array
                    if (enemyPrefabs.Length > 0)
                    {
                        int randomIndex = Random.Range(0, enemyPrefabs.Length);
                        GameObject selectedEnemyPrefab = enemyPrefabs[randomIndex];

                        // Instantiate the selected enemy at the spawn position
                        Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
                    }
                    else
                    {
                        //Debug.LogError("No enemy prefabs assigned in BossSummon script!");
                    }
                }
            }
        }
    }

    //Method to spawn bombs at random locations
    public void SummonBombs()
    {
        //Get a list of available spawn points from the bombSpawnPoints array
        List<GameObject> availableSpawnPoints = new List<GameObject>(bombSpawnPoints);
        List<GameObject> selectedSpawnPoints = new List<GameObject>();

        //Select 3 random spawn points for the bombs
        for (int i = 0; i < 3; i++)  // Always select 3 random points
        {
            if (availableSpawnPoints.Count > 0)
            {
                int randomIndex = Random.Range(0, availableSpawnPoints.Count); // Get a random index from available spawn points
                selectedSpawnPoints.Add(availableSpawnPoints[randomIndex]);  // Add the selected spawn point to the list
                availableSpawnPoints.RemoveAt(randomIndex);  // Remove the selected spawn point to avoid repetition
            }
        }

        //Instantiate bombs at selected locations
        foreach (GameObject spawnPoint in selectedSpawnPoints)
        {
            if (bombPrefabs.Length > 0)  // Ensure bomb prefabs are assigned
            {
                int randomBombIndex = Random.Range(0, bombPrefabs.Length);  // Pick a random bomb prefab
                GameObject bombPrefab = bombPrefabs[randomBombIndex];  // Get the selected bomb prefab
                Instantiate(bombPrefab, spawnPoint.transform.position, Quaternion.identity);  // Instantiate the bomb at the spawn point
            }
            else
            {
                //Debug.LogError("No bomb prefabs assigned in BossSummon script!");
            }
        }
    }
}
