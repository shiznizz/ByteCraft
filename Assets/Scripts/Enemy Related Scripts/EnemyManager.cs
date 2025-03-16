using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Configuration")]
    private int startingEnemies = 0;

    public int currentEnemies { get; private set; }

    private void Awake()
    {
        currentEnemies = startingEnemies;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.enemyEvents.onEnemyGained += EnemyGained;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.enemyEvents.onEnemyGained -= EnemyGained;
    }

    private void Start()
    {
        GameEventsManager.instance.enemyEvents.EnemyChange(currentEnemies);
    }

    private void EnemyGained(int key)
    {
        currentEnemies++;
        GameEventsManager.instance.enemyEvents.EnemyChange(currentEnemies);
    }
}
