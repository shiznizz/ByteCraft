using UnityEngine;

public class explosion : MonoBehaviour
{
    [Header("Explosion Properties")]
    [SerializeField] GameObject explosiveDevice;
    [SerializeField] Transform explosionCenter;
    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] float explosionRadius;
    [SerializeField] int explosionDmg;
    [SerializeField] float explosionForce;
    [SerializeField] float detonationDelay;
    public bool defaultActiveState;

    [Header("Explosion Effects")]
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] float destroyDelay;
    bool hasExploded;
    
    float detonationTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = explosionRadius;

        explosiveDevice.SetActive(defaultActiveState);
        detonationTimer = detonationDelay;

        hasExploded = false;
    }

    void Update()
    {
        if(!gameManager.instance.isPaused) 
        {
            if(!hasExploded)
            {
                if(explosiveDevice.activeSelf)
                     Explode();
                else
                     detonationDelay -= Time.deltaTime;

                if (detonationDelay <= 0)
                     explosiveDevice.SetActive(true);
            }
            else
                 Destroy(explosiveDevice, destroyDelay);
        }
    }

    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
            else
            {
                //hit.
            }

            IDamage damage = hit.GetComponent<IDamage>();
            damage?.takeDamage(explosionDmg);
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
