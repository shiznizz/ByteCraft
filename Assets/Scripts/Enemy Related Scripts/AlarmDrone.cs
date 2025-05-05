using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public class AlarmDrone : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public Transform[] waypoints;
    [SerializeField] public float speed;
    [SerializeField] public float detectionRange;

    [Header("Audio")]
    [SerializeField] AudioSource alertAudioSource;
    [SerializeField] AudioSource flyingAudioSource;
    [SerializeField] AudioSource hoverAudioSource;
    [SerializeField] AudioClip alertClip;
    [SerializeField] AudioClip deathClip;

    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] List<enemyAI> nearbyEnemies;
    [SerializeField] private Renderer model;
    [SerializeField] int roamPauseTime; // Pause time at each waypoint
    private float roamTimer;

    private int currentWaypoint = 0;
    public bool isPaused = false;
    public bool isAlerted;
    public bool isDead = false;
    private Vector3 fallTarget;
    private bool isFalling = false;
    private enemyAI droneScript;

    private void Start()
    {
        alertAudioSource.volume = 0.3f;
        //alertAudioSource.enabled = false;
        isAlerted = false;
        droneScript = GetComponent<enemyAI>();
    }

    void Update()
    {
        if (droneScript != null)
        {
            if (droneScript.HP <= 0 && !isFalling)
            {
                handleDeath();
            }
        }
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

        if (isFalling)
        {
            transform.position = Vector3.MoveTowards(transform.position, fallTarget, Time.deltaTime * 2f);

            transform.Rotate(Vector3.forward * 100f * Time.deltaTime);

            if (Vector3.Distance(transform.position, fallTarget) < 0.1f)
            {
                isFalling = false;
                Destroy(gameObject, 3f);
            }
        }
    }

    void Patrol()
    {
        // do nothing if there are no waypoints set up
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];

        // rotate towards the target waypoint
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(-direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // move towards the target waypoint
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // start movement sound if not already playing
        if (!flyingAudioSource.isPlaying)
        {
            flyingAudioSource.loop = true;
            flyingAudioSource.Play();
        }

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            isPaused = true;    // Pause when reaching a waypoint
            roamTimer = 0f;     // Reset the pause timer
            flyingAudioSource.Stop();
        }
    }

    void PauseAtWaypoint()
    {
        if (!hoverAudioSource.isPlaying) 
        { 
            hoverAudioSource.Play();
        }
        roamTimer += Time.deltaTime;

        if (roamTimer >= roamPauseTime)
        {
            isPaused = false;
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length; // Move to the next waypoint
            hoverAudioSource.Stop();
        }
    }

    void DetectPlayer()
    {
        AlertNearbyEnemies();
    }

    void AlertNearbyEnemies()
    {
        int excludedLayers = groundLayer | playerLayer;
        int includedLayers = ~excludedLayers;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, includedLayers);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                enemyAI enemyScript = hitCollider.GetComponent<enemyAI>();
                if (enemyScript != null)
                {
                    nearbyEnemies.Add(enemyScript);
                    enemyScript.SetAlerted(true);
                    enemyScript.SetPlayerInDroneRange(true);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isAlerted = true;
            //coll.radius = detectionRange + 2;
            DetectPlayer();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //coll.radius = detectionRange;
            isAlerted = false;
            alertAudioSource.enabled = false;
            foreach (enemyAI enemy in nearbyEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetPlayerInDroneRange(false);
                }
            }
        }
    }

    void PlayAlarmAudio()
    {
        alertAudioSource.enabled = true;
        alertAudioSource.PlayOneShot(alertClip);
    }

    public void handleDeath()
    {
        if (isDead) return;
        if (!isDead) isDead = true;
        isAlerted = false;

        flyingAudioSource.Stop();
        hoverAudioSource.Stop();
        alertAudioSource.Stop();

        if (alertAudioSource != null)
        {
            alertAudioSource.PlayOneShot(deathClip);
        }

        isPaused = true;
        speed = 0;

        fallTarget = new Vector3(transform.position.x, transform.position.y - 1.3f, transform.position.z);
        isFalling = true;
    }
}