using UnityEngine;

public class SlowMovingOrb : MonoBehaviour
{
    public float speed = 5f; // Orbs speed
    public float explosionRadius = 5f; // Explosion area radius
    public int explosionDamage = 50; // Damage of the explosion
    public GameObject explosionEffect; // Visual effect for explosion
    public LayerMask damageLayer; // Layer mask to check what takes damage

    private void Update()
    {
        //Moves the orb forward at a slow speed
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Triggers explosion when the orb collides with anything
        Explode();
    }

    void Explode()
    {
        //Creates explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        //Damage enemies in the explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayer);
        foreach (Collider collider in hitColliders)
        {
            IDamage damageable = collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(explosionDamage);  // Apply damage
            }
        }

        //Destroy the orb after explosion
        Destroy(gameObject);
    }
}
