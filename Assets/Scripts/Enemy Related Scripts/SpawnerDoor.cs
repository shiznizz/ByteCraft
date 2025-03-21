using UnityEngine;

public class SpawnerDoor : MonoBehaviour, IDamage
{
    [Header("Config")]
    [SerializeField] GameObject[] objectsToSpawn;
    [SerializeField] public int timeBetweenSpawns;
    [SerializeField] Transform spawnPos;
    [SerializeField] int HP;

    float spawnTimer;
    int spawnCount;
    bool startSpawning;
    int timeBetweenSpawnsOrig;

    private void Start()
    {
        timeBetweenSpawnsOrig = timeBetweenSpawns;
        timeBetweenSpawns = 3;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (startSpawning)
        {
            if (spawnCount >= 1) timeBetweenSpawns = timeBetweenSpawnsOrig;
            if (spawnTimer >= timeBetweenSpawns)
            {
                spawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }

    void spawn()
    {
        Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)], spawnPos.position, spawnPos.rotation);
        GoalManager.instance.updateGameGoal(-1);
        spawnCount++;
        spawnTimer = 0;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }
}
