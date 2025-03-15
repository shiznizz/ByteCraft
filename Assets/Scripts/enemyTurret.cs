using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyTurret : MonoBehaviour, IDamage
{
    enum TurretRotationType { set, dynamic}

    [SerializeField] TurretRotationType type;
    [SerializeField] Renderer model;
    [SerializeField] Animator anim;
    [SerializeField] Image hpFillBar;
    [SerializeField] Canvas hpBar;

    [Header("Enemy Stats")]
    [SerializeField] int HP;
    [SerializeField] int animTransSpeed;
    [Range(1,4)][SerializeField] int turnSpeed;
    [SerializeField] int FOV; //Field of View
    private int HPOrginal;

    [Header("Shoot Options")]
    [SerializeField] Transform headPos; //Head position
    [SerializeField] int shootAngle;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;
    [SerializeField] float turnTimeAfterDmg;
    private float shootTimer;
    private float turnTimer;
    private Vector3 playerDir;
    private float angleToPlayer;
    bool playerInRange;

    [Header("Set Rotation settings")]
    [SerializeField] int amountToRotate;
    [SerializeField] float pauseTime;
    private float pauseTimer;
    private Quaternion origRotation;
    private bool turnLeft = true;
    private Quaternion rot;

    [Header("Loot Drop Settings")]
    [SerializeField] bool dropsLoot;
    [SerializeField] List<LootItem> lootTable;
    [SerializeField] Transform dropPos;

    [Header("Death Settings")]
    [SerializeField] private float bodyFadeTime = 5f;
    [SerializeField] private float fadeDuration = 2f;
    private bool isDead = false;
    private Rigidbody rb;
    private Collider enemyCollider;


    private Color colorOrig;
    private bool recentDmg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == TurretRotationType.set)
        {
            origRotation = transform.rotation;
            rot = Quaternion.Euler(new Vector3(0, origRotation.y + amountToRotate, 0));
        }

        HPOrginal = HP;
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        updateEnemyUI();
        shootTimer += Time.deltaTime;
        turnTimer += Time.deltaTime;
        pauseTimer += Time.deltaTime;


        if (type == TurretRotationType.dynamic)
        {
            if (playerInRange && canSeePlayer())
            {

            }
            else if (HP < HPOrginal && turnTimer <= turnTimeAfterDmg)
            {
                Debug.Log("turn");
                faceTarget();
            }
        }
        else if (type == TurretRotationType.set)
        {
            if (rot == transform.rotation && pauseTimer >= pauseTime)
            {
                turnLeft = !turnLeft;
                if (turnLeft)
                {
                    rot = Quaternion.Euler(new Vector3(0,origRotation.y + amountToRotate, 0));
                }
                else
                {
                    rot = Quaternion.Euler(new Vector3(0, origRotation.y - amountToRotate, 0));
                }
            }
            SetRotation(rot);

            if (shootTimer >= shootRate)
                shoot();
        }
    }

    private void SetRotation(Quaternion rot)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        Debug.DrawRay(headPos.position, playerDir, Color.cyan);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit) && angleToPlayer <= FOV)
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                if (shootTimer > shootRate && angleToPlayer <= shootAngle) 
                    shoot();
                if (type == TurretRotationType.dynamic)
                    faceTarget();
                return true;
            }
        }
        return false;
    }
    void faceTarget()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }

    void shoot()
    {

            shootTimer = 0;

            if (anim != null)
                anim.SetTrigger("Shoot");
            else
                createProjectile();
    }

    public void createProjectile()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    public void takeDamage(int amount)
    {
        
        if (HP > 0)
        {
            turnTimer = 0;
            StartCoroutine(enemyShowHpBar());

            HP -= amount;
            StartCoroutine(flashRed());
            if (anim != null)
                anim.SetTrigger("damage");

            faceTarget();


            if (HP <= 0 && !isDead)
            {
                isDead = true;
                gameManager.instance.updateGameGoal(-1);
                if (dropsLoot)
                    dropLoot();

                handleDeath();
            }
        }
    }

    public void dropLoot()
    {
        playerController player = gameManager.instance.playerScript;
        float healthRatio = playerStatManager.instance.HP / (float)player.HPOrig;
        float currAmmo = float.Parse(gameManager.instance.ammoCurText.text);
        float reserveAmmo = float.Parse(gameManager.instance.ammoReserveText.text);
        float maxAmmo = float.Parse(gameManager.instance.ammoMaxText.text);

        float ammoRatio = (currAmmo + reserveAmmo) / maxAmmo;
        foreach (LootItem loot in lootTable)
        {
            float adjustedDropChance = loot.dropChance;

            if (loot.type == itemType.HP && healthRatio < 0.5f)
            {
                adjustedDropChance += Mathf.Lerp(0, 50, 1 - healthRatio); // increase drop chance when health is below 50%
            }
            else if (loot.type == itemType.Ammo && ammoRatio < 0.5f)
            {
                adjustedDropChance += Mathf.Lerp(0, 50, 1 - ammoRatio);
            }


            float roll = Random.Range(0f, 100f);
            if (roll <= adjustedDropChance)
            {
                Instantiate(loot.itemModel, dropPos.position, transform.rotation);
            }
        }
    }

    private void handleDeath()
    {
        hpBar.gameObject.SetActive(false);
        //Disable the collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false; //Disables the collider to avoid further interactions
        }

        //Disables movement and enables physics
        if (rb != null)
        {
            rb.isKinematic = false; // Enable physics
            rb.useGravity = false; // Allow gravity to affect the body

        }

        if (anim != null)
            anim.SetTrigger("Death"); // Trigger death animation

        //Starts fade out process
        StartCoroutine(fadeOutBody());
    }

    private IEnumerator fadeOutBody()
    {
        yield return new WaitForSeconds(bodyFadeTime); // Wait for the specified time before fading

        float elapsedTime = 0f;
        Color originalColor = model.material.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Fade from 1 to 0 alpha
            model.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject); // Destroy the body after fading
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator enemyShowHpBar()
    {
        hpBar.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);

        hpBar.gameObject.SetActive(false);
    }

    void updateEnemyUI()
    {
        hpFillBar.fillAmount = (float)HP / HPOrginal;
        hpBar.transform.LookAt(gameManager.instance.player.transform.position);
    }
}
