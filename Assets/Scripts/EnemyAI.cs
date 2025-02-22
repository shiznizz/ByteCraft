using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage, lootDrop
{
    enum enemyType { range, melee }

    [SerializeField] enemyType type;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;

    [SerializeField] Transform headPos; //Head position
    [SerializeField] int HP;
    [SerializeField] int animTransSpeed;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] int FOV; //Field of View

    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] Collider attackCol;
    [SerializeField] float shootRate;
    [SerializeField] float meleeDistance;
    [SerializeField] bool dropsLoot;

    [SerializeField] Collider weaponCol;

    Color colorOrig;

    float shootTimer;
    float angleToPlayer;
    float stoppingDistOrig;

    Vector3 playerDir;

    bool playerInRange;

    // loot drop mechanic variables
    [SerializeField] GameObject lootItem;
    [SerializeField] Transform dropPos;

    // enemy roaming variables
    [SerializeField] int roamPauseTime;
    [SerializeField] int roamDist;
    Vector3 startingPos;
    float roamTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        //gameManager.instance.updateGameGoal(1);
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        float agentSpeed = agent.velocity.normalized.magnitude; //for agent you are converting a vector 3 to a float by getting the magnitude
        float animatorCurSpeed = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.MoveTowards(animatorCurSpeed, agentSpeed, Time.deltaTime * animTransSpeed));    // added multiplication by animTransSpeed to give control from editor

        shootTimer += Time.deltaTime;

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;


        if (playerInRange && !canSeePlayer())
            checkRoam();
        else if (!playerInRange)
            checkRoam();
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, transform.position.y, playerDir.z), transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit) && angleToPlayer <= FOV)
        {
            if(hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                agent.SetDestination(gameManager.instance.player.transform.position);

                if (shootTimer >= shootRate && type == enemyType.range)
                {
                    shoot();
                }
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                return true;
            }
        }
        agent.stoppingDistance = 0;
        return false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (shootTimer >= shootRate && type == enemyType.melee && agent.remainingDistance <= agent.stoppingDistance)
        {
            meleeAttack();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = false;
        }
        agent.stoppingDistance = 0;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(flashRed());
        agent.SetDestination(gameManager.instance.player.transform.position);

        //weaponColOff();

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

    void shoot()
    {
        shootTimer = 0;

        anim.SetTrigger("Shoot");
    }

    public void createBullet()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    void meleeAttack()
    {
        shootTimer = 0;
        anim.Play("Melee Attack");
    }

    public void dropLoot()
    {
        Instantiate(lootItem, dropPos.position, transform.rotation);
    }

    void checkRoam()
    {
        if ((roamTimer > roamPauseTime && agent.remainingDistance < 0.01f) || gameManager.instance.playerScript.HP <= 0)
            roam();
    }

    void roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 randPos = Random.insideUnitSphere * roamDist;
        randPos += startingPos;

        NavMeshHit hit;

        NavMesh.SamplePosition(randPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
    }

    public void turnOnCol()
    {
        attackCol.enabled = true;
    }

    public void turnOffCol()
    {
        attackCol.enabled = false;
    }
}
