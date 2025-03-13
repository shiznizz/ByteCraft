using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Net;
using System.Security.Cryptography;

public class traps : MonoBehaviour
{
    public enum TrapType { LandMines, StationaryLaser, MovingLaser }

    [Header("Trap Settings")]
    [SerializeField] TrapType trapType;
    [SerializeField] int damageAmount;
    [SerializeField] float triggerRadius;

    [Header("Land Mine Settings")]
    [SerializeField] GameObject explosionEffect;

    [Header("Stationary Laser Settings")]
    [SerializeField] GameObject laserBeam;
    //[SerializeField] public float laserDuration = 3f;
    [SerializeField] public float maxLaserDistance = 100f;

    [Header("Moving Laser Settings")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    public float moveSpeed = 5;

    //[Header("Laser Sound Settings")]
    //[SerializeField] private AudioClip laserHitSound;
    //private AudioSource audioSource;

    private bool isTriggered = false;
    private bool movingToEnd = false;

    private BoxCollider laserCollider; // Collider to detect if the player is hit by laser
    private LineRenderer lineRenderer; // LineRenderer to visualize the laser

    void Start()
    {
        if (trapType == TrapType.MovingLaser)
        {
            // Initialize the moving laser
            MovingLaser();
        }
        else if (trapType == TrapType.StationaryLaser)
        {
            TriggerStationaryLaser();
        }
    }

    void Update()
    {
        if (trapType == TrapType.MovingLaser)
        {
            MovingLaser();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            Debug.Log("Player entered the laser trigger area!");
            isTriggered = true;
            ApplyDamage(other.gameObject);
            TriggerTrapEffect();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is inside the laser trigger area");
            ApplyDamage(other.gameObject);
        }
    }

    private void ApplyDamage(GameObject player)
    {
        playerController playerScript = player.GetComponent<playerController>();
        if (playerScript != null)
        {
            playerScript.takeDamage(damageAmount);
        }

        TriggerTrapEffect();
    }

    public void TriggerTrapEffect()
    {
        switch (trapType)
        {
            case TrapType.LandMines:
                TriggerLandMine();
                break;
            case TrapType.StationaryLaser:
                TriggerStationaryLaser();
                break;
            case TrapType.MovingLaser:
                // Moving laser isn't missing here: its always moving so this isn't required just FYI
                break;
        }
    }

    private void TriggerLandMine()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Stationary laser-specific effects (activating the laser)
    private void TriggerStationaryLaser()
    {
        if (laserBeam != null)
        {
            // Ensure the laser beam is active
            laserBeam.SetActive(true);
            lineRenderer = laserBeam.GetComponent<LineRenderer>();

            laserCollider = laserBeam.GetComponent<BoxCollider>();
            if (laserCollider == null)
            {
                laserCollider = laserBeam.AddComponent<BoxCollider>();
                laserCollider.isTrigger = true;
            }

            // Raycast from the laser's starting point
            RaycastHit hit;
            Vector3 laserDirection = transform.forward; // Direction the laser is facing

            // Perform raycast from laser's position to check where it hits
            float laserLength = maxLaserDistance;
            Vector3 laserEndPoint = transform.position + laserDirection * laserLength;

            if (Physics.Raycast(transform.position, laserDirection, out hit, maxLaserDistance))
            {
                laserEndPoint = hit.point;  // Update the laser's endpoint if we hit something
                laserLength = Vector3.Distance(transform.position, hit.point);  // Update laser length
            }

            // Set the start and end points of the laser visually (LineRenderer)
            lineRenderer.SetPosition(0, transform.position);  // Set start point
            lineRenderer.SetPosition(1, laserEndPoint);       // Set end point (where the laser hits or max distance)

            // Adjust the collider's size to match the laser's length
            laserCollider.size = new Vector3(0.1f, 0.1f, laserLength * 2);  // Small width, length = distance
            laserCollider.center = new Vector3(0f, 0f, laserLength);  // Center the collider along the laser's path

            // Align the collider with the laser's rotation and position
            laserCollider.transform.rotation = transform.rotation;
        }
    }



    // Moving laser movement logic
    private void MovingLaser()
    {
        if (startPoint != null && endPoint != null)
        {
            // Determine which direction the laser should move (towards start or end point)
            Vector3 targetPosition = movingToEnd ? endPoint.position : startPoint.position;

            // Move the laser towards the target position (startPoint -> endPoint, or vice versa)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if the laser has reached the target position
            if (transform.position == targetPosition)
            {
                // Toggle the direction of movement
                movingToEnd = !movingToEnd;
            }

            if (laserBeam != null)
            {
                // Ensure the laser beam is active
                laserBeam.SetActive(true);
                lineRenderer = laserBeam.GetComponent<LineRenderer>();

                laserCollider = laserBeam.GetComponent<BoxCollider>();
                if (laserCollider == null)
                {
                    laserCollider = laserBeam.AddComponent<BoxCollider>();
                    laserCollider.isTrigger = true;
                }

                // Raycast from the laser's starting point
                RaycastHit hit;
                Vector3 laserDirection = transform.forward; // Direction the laser is facing

                // Perform raycast from laser's position to check where it hits
                float laserLength = maxLaserDistance;
                Vector3 laserEndPoint = transform.position + laserDirection * laserLength;

                if (Physics.Raycast(transform.position, laserDirection, out hit, maxLaserDistance))
                {
                    laserEndPoint = hit.point;  // Update the laser's endpoint if we hit something
                    laserLength = Vector3.Distance(transform.position, hit.point);  // Update laser length
                }

                // Set the start and end points of the laser visually (LineRenderer)
                lineRenderer.SetPosition(0, transform.position);  // Set start point
                lineRenderer.SetPosition(1, laserEndPoint);       // Set end point (where the laser hits or max distance)

                // Adjust the collider's size to match the laser's length
                laserCollider.size = new Vector3(0.1f, 0.1f, laserLength * 2);  // Small width, length = distance
                laserCollider.center = new Vector3(0f, 0f, laserLength);  // Center the collider along the laser's path

                // Align the collider with the laser's rotation and position
                laserCollider.transform.rotation = transform.rotation;
            }
        }
    }

    //private void PlayLaserHitSound()
    //{
    //    if (audioSource != null && laserHitSound != null)
    //    {
    //        audioSource.PlayOneShot(laserHitSound);
    //    }
    //}

    void OnDrawGizmos()
    {
        //if (laserCollider != null)
        //{
        //    // Visualize the laser collider
        //    Gizmos.color = Color.red; // Color the collider red for debugging
        //    Gizmos.DrawCube(laserCollider.transform.position + laserCollider.center, laserCollider.size);
        //}

        if (lineRenderer != null)
        {
            Gizmos.color = Color.blue; // Color the line blue
            Gizmos.DrawLine(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1)); // Draw the laser line
        }

        //Draw start and end points for debugging purposes
        if (startPoint != null)
        {
            Gizmos.color = Color.green; // Start point
            Gizmos.DrawSphere(startPoint.position, 0.2f);
        }

        if (endPoint != null)
        {
            Gizmos.color = Color.red; // End point
            Gizmos.DrawSphere(endPoint.position, 0.2f);
        }
    }
}