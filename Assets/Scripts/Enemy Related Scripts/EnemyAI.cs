using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyAI : MonoBehaviour, IDamage, lootDrop
{
    enum enemyType { range, melee, stationary, kamikaze }
    enum movementType { random, setPath, seeking}

    #region Variables
    [Header("General Enemy Settings")]
    [SerializeField] enemyType type;
    [SerializeField] movementType movement;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    [Header("Enemy Stats")]
    [SerializeField] Image hpFillBar;
    [SerializeField] Canvas hpBar;
    [SerializeField] int HP;
    [SerializeField] int animTransSpeed;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV; //Field of View
    private int HPOrginal;

    [Header("Ranged Enemy Options")]
    [SerializeField] Transform headPos; //Head position
    [SerializeField] int shootAngle;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;
    float shootTimer;

    [Header("Melee Enemy Options")]
    [SerializeField] Collider meleeCol;
    [SerializeField] float meleeDistance;
    private bool hasExploded = false;

    [Header("Loot Drop Settings")]
    [SerializeField] bool dropsLoot;
    [SerializeField] List<LootItem> lootTable;
    [SerializeField] Transform dropPos;

    [Header("Roaming Settings")]
    [SerializeField] int roamPauseTime;
    [SerializeField] List<Transform> pathPositions;
    [SerializeField] int roamDist;
    int currentPathPos;

    [Header("Death Settings")]
    [SerializeField] private float bodyFadeTime = 5f;
    [SerializeField] private float fadeDuration = 2f;
    private bool isDead = false;
    private Rigidbody rb;
    private Collider enemyCollider;
    private Renderer bodyRenderer;


    Vector3 startingPos;
    float roamTimer;
    float stoppingDistOrig;
    Vector3 playerDir;
    bool playerInRange;
    float angleToPlayer;

    Color colorOrig;

    #endregion Variables


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrginal = HP;
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
        startingPos = transform.position;
        if (type != enemyType.stationary)
        {
            stoppingDistOrig = agent.stoppingDistance;
        }

        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        bodyRenderer = model;

        // scale enemy stats based on current difficulty
        if (DifficultyManager.instance != null)
        {
            HP = Mathf.RoundToInt(HP * DifficultyManager.instance.enemyHealthMultiplier);
        }
    }

    // Update is called once per frame
    void Update()
    {
       updateEnemyUI();
        if (type != enemyType.stationary)
        {
            float agentSpeed = agent.velocity.normalized.magnitude; //for agent you are converting a vector 3 to a float by getting the magnitude
            float animatorCurSpeed = anim.GetFloat("Speed");

            anim.SetFloat("Speed", Mathf.MoveTowards(animatorCurSpeed, agentSpeed, Time.deltaTime * animTransSpeed));    // added multiplication by animTransSpeed to give control from editor
            
            if (agent.remainingDistance < 0.01f)
                roamTimer += Time.deltaTime;
        }

        shootTimer += Time.deltaTime;

        if (playerInRange && !canSeePlayer())
            checkRoam();
        else if (!playerInRange)
            checkRoam();
    }

    void updateEnemyUI()
    {
        hpFillBar.fillAmount = (float)HP / HPOrginal;
        hpBar.transform.LookAt(gameManager.instance.player.transform.position);
    }

    #region EnemyMovement

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, 0, playerDir.z), transform.forward);

        Debug.DrawRay(headPos.position, playerDir,Color.cyan);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit) && angleToPlayer <= FOV)
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                if (type != enemyType.stationary)
                {
                    agent.SetDestination(gameManager.instance.player.transform.position);
                }

                //Ranged attack
                if (type != enemyType.melee && shootTimer >= shootRate && angleToPlayer <= shootAngle)
                {
                    shoot();
                }
                //Melee attack
                float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);
                if (type == enemyType.melee && shootTimer >= shootRate && distanceToPlayer <= meleeDistance)
                //if (shootTimer >= shootRate && type == enemyType.melee && agent.remainingDistance <= meleeDistance) // Ensures attack happens when the shoot timer is ready
                {
                    meleeAttack();
                }               
                if (agent.remainingDistance <= agent.stoppingDistance && !isDead)
                {
                    faceTarget();                 
                }

                agent.stoppingDistance = stoppingDistOrig;

                return true;
            }
        }
        agent.stoppingDistance = 0;
        return false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (type == enemyType.kamikaze)
            {
                //Start the kamikaze attack
                kamikazeAttack();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
        agent.stoppingDistance = 0;
    }

    void faceTarget()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    void checkRoam()
    {
        if (type == enemyType.stationary)
            return;

        else if (roamTimer > roamPauseTime && agent.remainingDistance < 0.01f)
        {
            if (movement == movementType.random)
                randomRoam();
            else if (movement == movementType.setPath)
                setPathRoam();
        }
    }

    void randomRoam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 randPos = Random.insideUnitSphere * roamDist;
        randPos += startingPos;

        NavMeshHit hit;

        NavMesh.SamplePosition(randPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
    }

    void setPathRoam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        agent.SetDestination(pathPositions[currentPathPos].position);

        if (currentPathPos < pathPositions.Count - 1)
            currentPathPos++;
        else
            currentPathPos = 0;
    }

    #endregion EnemyMovement

    #region EnemyDamage
    IEnumerator enemyShowHpBar()
    {
        hpBar.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(5f);

        hpBar.gameObject.SetActive(false);
    }
    public void takeDamage(int amount)
    {
        if (HP > 0)
        {
            StartCoroutine(enemyShowHpBar());

            HP -= amount;
            StartCoroutine(flashRed());
            if (anim != null)
                anim.SetTrigger("damage");


            if (type != enemyType.stationary)
            {
                agent.SetDestination(gameManager.instance.player.transform.position);
            }
            else
            {
                faceTarget();
            }

            if (meleeCol != null)
                turnOffCol();

            if (HP <= 0 && !isDead)
            {
                isDead = true;
                gameManager.instance.updateGameGoal(-1);
                if (dropsLoot)
                    dropLoot();

                handleDeath();
                //Destroy(gameObject);
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
        if (agent != null)
        {
            agent.isStopped = true; // Stops movement
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
        Color originalColor = bodyRenderer.material.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Fade from 1 to 0 alpha
            bodyRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
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

    #endregion EnemyDamage

    #region EnemyAttack
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
        //Creates a projectile at shootPos with the same rotation as the enemy
        Instantiate(bullet, shootPos.position, transform.rotation);

    }

    void meleeAttack()
    {
        shootTimer = 0;
        anim.SetTrigger("Melee Attack");
        //shootTimer = 0; // Reset the shoot timer for the cooldown between melee attacks

        turnOnCol();


        if (type == enemyType.kamikaze)
        {
            kamikazeAttack();
        }
    }

    void kamikazeAttack()
    {
        if (hasExploded) return;

        hasExploded = true;

        if (type == enemyType.kamikaze)
        {
            agent.SetDestination(gameManager.instance.player.transform.position);
        }

        // Check if within melee range to trigger detonation
        if (Vector3.Distance(transform.position, gameManager.instance.player.transform.position) <= meleeDistance)
        {
            // Trigger detonate animation (similar to melee attack)
            anim.SetTrigger("Detonate");

            StartCoroutine(explosionAfterDelay());
        }
    }

    private IEnumerator explosionAfterDelay()
    {
        // Wait for the detonate animation to finish (adjust the delay as needed)
        yield return new WaitForSeconds(3.0f);

        // Trigger explosion animation
        anim.SetTrigger("Explode");

        // Apply explosion damage if the player is close enough
        if (Vector3.Distance(transform.position, gameManager.instance.player.transform.position) <= meleeDistance)
        {
            // scale explosion damage based on difficulty
            int baseDamage = 25;
            int explosionDamage = (DifficultyManager.instance != null)
                ? Mathf.RoundToInt(baseDamage * DifficultyManager.instance.enemyDamageMultiplier)
                : baseDamage;
            gameManager.instance.playerScript.takeDamage(25); // Adjust explosion damage as needed
        }

        // Destroy the Kamikaze enemy after explosion
        Destroy(gameObject);
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

    public void turnOnCol()
    {
        meleeCol.enabled = true;
    }

    public void turnOffCol()
    {
        meleeCol.enabled = false;
    }

    #endregion EnemyAttack
}
