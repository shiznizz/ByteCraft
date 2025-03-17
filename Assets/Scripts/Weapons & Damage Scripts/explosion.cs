using UnityEngine;

public class explosion : MonoBehaviour
{
    [Header("Explosion Properties")]
    [SerializeField] Transform explosionCenter;
    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] float explosionRadius;
    [SerializeField] float explosionDmg;
    [SerializeField] float explosionForce;
    [SerializeField] float detonationDelay;

    [Header("Explosion Effects")]
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] float destroyDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = explosionRadius;

        Explode();
    }

    public void Explode()
    {

    }
}
