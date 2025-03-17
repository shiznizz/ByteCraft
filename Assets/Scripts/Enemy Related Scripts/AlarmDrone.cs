using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class AlarmDrone : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public Transform[] waypoints;
    [SerializeField] public float speed;
    [SerializeField] public float detectionRange;
    [SerializeField] GameObject[] nearbyEnemies;
    [SerializeField] SphereCollider coll;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip alertClip;

    [SerializeField] int roamPauseTime; // Pause time at each waypoint
    private float roamTimer;

    private int currentWaypoint = 0;
    public bool isPaused = false;
    public bool isAlerted;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.3f;
        audioSource.enabled = false;
        coll.radius = detectionRange;
        isAlerted = false;
    }

    void Update()
    {
        if (!isAlerted)
        {
            if (!isPaused)
            {
                Patrol();
            }
            else
            {
                PauseAtWaypoint();
            }
        } 
        else 
        {
            PlayAlarmAudio();
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            isPaused = true;    // Pause when reaching a waypoint
            roamTimer = 0f;     // Reset the pause timer
        }
    }

    void PauseAtWaypoint()
    {
        roamTimer += Time.deltaTime;

        if (roamTimer >= roamPauseTime)
        {
            isPaused = false;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length; // Move to the next waypoint
        }
    }

    void DetectPlayer()
    {
        AlertNearbyEnemies();
    }

    void AlertNearbyEnemies()
    {
        foreach(GameObject enemy in nearbyEnemies)
        {
            enemyAI enemyScript = enemy.GetComponent<enemyAI>();
            if (enemyScript != null)
            {
                enemyScript.SetAlerted(true);
                enemyScript.SetPlayerInDroneRange(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isAlerted = true;
            coll.radius = detectionRange + 2;
            DetectPlayer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            coll.radius = detectionRange;
            isAlerted = false;
            audioSource.enabled = false;
            foreach (GameObject enemy in nearbyEnemies)
            {
                enemyAI enemyScript = enemy.GetComponent<enemyAI>();
                if (enemyScript != null)
                {
                    enemyScript.SetPlayerInDroneRange(false);
                }
            }
        }
    }

    void PlayAlarmAudio()
    {
        audioSource.enabled = true;
        audioSource.PlayOneShot(alertClip);
    }
}