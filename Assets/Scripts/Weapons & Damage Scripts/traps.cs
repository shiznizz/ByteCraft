using UnityEngine;
using System.Collections;

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
    public float laserDuration = 3f;

    [Header("Moving Laser Settings")]
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    public float moveSpeed = 5;

    private bool isTriggered = false;
    private bool movingToEnd = false;

    void Start()
    {
        if (laserBeam != null && trapType == TrapType.StationaryLaser) 
        {
            laserBeam.SetActive(false); // Starts with laser off
        }
    }

    void Update()
    {
        if (trapType == TrapType.MovingLaser)
        {
            MoveMovingLaser();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;
            ApplyDamage(other.gameObject);
            TriggerTrapEffect();
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
                //TriggerStationaryLaser();
                break;
            case TrapType.MovingLaser:
                // Moving laser is constantly moving so this is not required here
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
            laserBeam.SetActive(true);
            StartCoroutine(DeactivateLaser());
        }

        // Optional: Play laser activation sound or effects
    }

    // Moving laser movement logic
    private void MoveMovingLaser()
    {
        if (startPoint != null && endPoint != null)
        {
            Vector3 targetPosition = movingToEnd ? endPoint.position : startPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                movingToEnd = !movingToEnd;
            }
        }
    }

    private IEnumerator DeactivateLaser()
    {
        yield return new WaitForSeconds(laserDuration);
        if (laserBeam != null)
        {
            laserBeam.SetActive(false);
        }

        // Optional: Deactivate or destroy the laser trap if needed
        Destroy(gameObject); // Optional if you want to remove the trap after the laser is deactivated
    }
}