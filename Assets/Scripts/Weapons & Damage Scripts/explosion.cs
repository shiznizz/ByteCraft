using System.Collections.Generic;
using UnityEngine;

public class explosion : MonoBehaviour
{
    [Header("Explosion Properties")]
    [SerializeField] GameObject explosiveDevice;
    [SerializeField] GameObject explosiveContainer;
    [SerializeField] Transform explosionCenter;
    [SerializeField] float explosionRadius;
    [SerializeField] int explosionDmg;
    [SerializeField] float explosionForce;
    [SerializeField] float explosionUpForce;
    [SerializeField] float detonationDelay;
    public bool defaultActiveState;
    public bool doesBombDestroy = true;

    [Header("Explosion Effects")]
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] float destroyDelay;
    bool hasExploded;
    
    float detonationTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        explosiveDevice.SetActive(defaultActiveState);
        detonationTimer = 0;

        hasExploded = false;
    }

    void Update()
    {
        if(!gameManager.instance.isPaused) 
        {
            if(!hasExploded)
            {
                if(explosiveDevice.activeSelf && detonationTimer >= detonationDelay)
                     Explode();
                else
                     detonationTimer += Time.deltaTime;

                if (detonationTimer <= 0)
                     explosiveDevice.SetActive(true);
            }
            else 
            {
                if (doesBombDestroy)
                {
                    if (explosiveContainer != null)
                    Destroy(explosiveContainer, destroyDelay);
                    Destroy(explosiveDevice, destroyDelay);
                }
                else
                {
                    hasExploded = false;
                    explosiveDevice.SetActive(false);
                }
            }
           
        }
    }

    public void Explode()
    {
        if (hasExploded) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        HashSet<GameObject> affectedObjects = new HashSet<GameObject>();

        foreach (Collider hit in colliders)
        {
            if (!affectedObjects.Contains(hit.gameObject)) // Ensure unique objects
            {
                affectedObjects.Add(hit.gameObject);

                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddForce(transform.up * explosionUpForce, ForceMode.Impulse);
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
                else
                {
                    // hit
                }

                IDamage damage = hit.GetComponent<IDamage>();
                damage?.takeDamage(explosionDmg);
            }
        }

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        hasExploded = true;
    }
}
