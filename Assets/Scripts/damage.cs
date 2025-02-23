using UnityEngine;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary}
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    float damageTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(type == damageType.moving)
        {
            rb.linearVelocity = (gameManager.instance.player.transform.position - transform.position).normalized * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void Update()
    {
        damageTimer += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        if (type == damageType.stationary && damageTimer > speed)
        {
            damageTimer = 0;

            IDamage dmg = other.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (type == damageType.moving)
        {
            if (other.isTrigger)
                return;

            IDamage dmg = other.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);
            }

            if (type == damageType.moving)
            {
                Destroy(gameObject);
            }
        }
    }
}
