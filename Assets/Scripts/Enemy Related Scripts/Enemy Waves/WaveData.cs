using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveData", menuName = "Waves/Wave Data")]
public class WaveData : ScriptableObject
{
    public string waveName;
    [SerializeField] public List<SpawnEvent> spawnEvents;
}

[System.Serializable]
public class SpawnEvent
{
    public GameObject enemyPrefab;
    public int enemyCount;
    public float spawnDelay; // delay before this event triggers
    [SerializeField] public List<Transform> spawnPoints;
}