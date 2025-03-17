using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private List<WaveData> waveSchedule;
    [SerializeField] private Transform[] spawnPoints;

    private int currentWaveIndex = 0;
    private void Start()
    {
        StartCoroutine(HandleWaveSpawns());
    }

    private IEnumerator HandleWaveSpawns()
    {
        foreach (WaveData wave in waveSchedule)
        {
            foreach (SpawnEvent spawnEvent in wave.spawnEvents)
            {
                yield return new WaitForSeconds(spawnEvent.spawnDelay);

                for (int i = 0; i < spawnEvent.enemyCount; i++)
                {
                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    Debug.Log("Spawning enemy.");
                    Instantiate(spawnEvent.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                    GoalManager.instance.updateGameGoal(-1);
                }
            }

            currentWaveIndex++;
            yield return new WaitForSeconds(5f); // delay between waves
        }
    }
}
