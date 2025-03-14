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

    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] damageHitSounds;
    //[SerializeField] private float damageHitVolume; // Volume of the sound
    private AudioSource audioSource;
    
    [Header("Seeking Projectile Parameters")]
    [SerializeField] int targetingDistance;
    [SerializeField] float turnSpeed;
    [SerializeField] LayerMask ignoreLayer;

    
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
                    if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, targetingDistance,~ignoreLayer))      
                        target = hit.collider.GetComponent<IDamage>();
                }

                if (target != null)
                {
                    SeekEnemy();
                }
                else
                {
                    rb.linearVelocity = Camera.main.transform.forward * speed;
                }
            }

            Destroy(gameObject, destroyTime);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not found on " + gameObject.name);
        }

        // Ensure damageHitSounds has at least one sound
        if (damageHitSounds.Length == 0 || damageHitSounds[0] == null)
        {
            Debug.LogError("No AudioClips assigned to damageHitSounds on " + gameObject.name);
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
                PlayDamageSound(); // Play sound when damage is applied
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
                PlayDamageSound(); // Play sound when damage is applied
            }

            Destroy(gameObject);
        }
    }

    // Method to play the damage sound
    private void PlayDamageSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource is missing on " + gameObject.name);
            return;
        }

        if (damageHitSounds.Length == 0 || damageHitSounds[0] == null)
        {
            Debug.LogWarning("No AudioClips assigned to damageHitSounds on " + gameObject.name);
            return;
        }

        // Play a random sound from the damageHitSounds array
        AudioClip soundToPlay = damageHitSounds[Random.Range(0, damageHitSounds.Length)];
        Debug.Log("Playing sound: " + soundToPlay.name);
        audioSource.PlayOneShot(soundToPlay);
    }
}
