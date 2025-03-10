using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage, lootDrop
{
    enum enemyType { range, melee, stationary }
    enum movementType { random, setPath, seeking}

    #region Variables
    [Header("General Enemy Settings")]
    [SerializeField] enemyType type;
    [SerializeField] movementType movement;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    [Header("Enemy Stats")]
    [SerializeField] int HP;
    [SerializeField] int animTransSpeed;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV; //Field of View

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

    [Header("Loot Drop Settings")]
    [SerializeField] bool dropsLoot;
    [SerializeField] List<LootItem> lootTable;
    [SerializeField] Transform dropPos;

    [Header("Roaming Settings")]
    [SerializeField] int roamPauseTime;
    [SerializeField] List<Transform> pathPositions;
    [SerializeField] int roamDist;
    int currentPathPos;


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
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
        startingPos = transform.position;
        if (type != enemyType.stationary)
        {
            stoppingDistOrig = agent.stoppingDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
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
                if (agent.remainingDistance <= agent.stoppingDistance)
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
    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
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

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            if (dropsLoot)
                dropLoot();
            Destroy(gameObject);
        }
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

    }

    public void dropLoot()
    {
        playerController player = gameManager.instance.playerScript;
        float healthRatio = playerStatManager.instance.playerHP / (float)player.HPOrig;
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
