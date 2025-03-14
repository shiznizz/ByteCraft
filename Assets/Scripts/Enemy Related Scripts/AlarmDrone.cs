using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class AlarmDrone : MonoBehaviour
{
    [SerializeField] public Transform[] waypoints;
    [SerializeField] public float speed;
    [SerializeField] public float detectionRange;
    public LayerMask playerLayer;
    public LayerMask enemyLayer;

    [SerializeField] int roamPauseTime; // Pause time at each waypoint
    private float roamTimer;

    private int currentWaypoint = 0;
    private bool isPaused = false;
    [SerializeField] GameObject[] nearbyEnemies;

    void Update()
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
            DetectPlayer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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
}