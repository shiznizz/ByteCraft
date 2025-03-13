using UnityEngine;
using UnityEngine.Rendering;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, seeking}

    [Header("General Projectile Settings")]
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    public bool playerProjectile;

    [Header("Seeking Projectile Parameters")]
    [SerializeField] int targetingDistance;
    [SerializeField] float turnSpeed;

    
    private IDamage target;
    private RaycastHit hit;

    float damageTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(type != damageType.stationary)
        {
            if (!playerProjectile)
                rb.linearVelocity = (gameManager.instance.player.transform.position - transform.position).normalized * speed;
            else 
            {
                if (type == damageType.seeking)
                {
                    if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, targetingDistance))      
                        target = hit.collider.GetComponent<IDamage>();
                }
                rb.linearVelocity = Camera.main.transform.forward * speed;
            }

            Destroy(gameObject, destroyTime);
        }
    }

    private void Update()
    {
        damageTimer += Time.deltaTime;
        if (type == damageType.seeking)
            SeekTarget();
    }

    private void SeekTarget()
    {
        if (playerProjectile)
        {
            SeekEnemy();
        }
        else
        {
            SeekPlayer();
        }
    }

    private void SeekPlayer()
    {
        Vector3 playerDir = gameManager.instance.player.transform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
        
        rb.linearVelocity = transform.forward * speed;
    }

    private void SeekEnemy()
    {
        if (target != null)
        {
            Vector3 enemyDir = hit.transform.position - transform.position;
            Quaternion rot = Quaternion.LookRotation(new Vector3(enemyDir.x, 0, enemyDir.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);

            rb.linearVelocity = transform.forward * speed;
        }
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
        if (type != damageType.stationary)
        {
            if (other.isTrigger)
                return;

            IDamage dmg = other.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(damageAmount);
            }

            Destroy(gameObject);
        }
    }
}
