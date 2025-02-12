using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
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
    [SerializeField] float shootRate;

    Color colorOrig;

    float shootTimer;
    float angleToPlayer;

    Vector3 playerDir;

    bool playerInRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        gameManager.instance.updateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        float agentSpeed = agent.velocity.normalized.magnitude;
        float animCurSpeed = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.MoveTowards(animCurSpeed, agentSpeed, Time.deltaTime * animTransSpeed));

        shootTimer += Time.deltaTime;

        if(playerInRange && canSeePlayer())
        {

        }
        agent.SetDestination(gameManager.instance.player.transform.position);

        if (shootTimer >= shootRate)
        {
            shoot();
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            faceTarget();
        }
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;
        if(Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if(hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                agent.SetDestination(gameManager.instance.player.transform.position);

                if (shootTimer >= shootRate)
                    shoot();
                if (agent.remainingDistance <= agent.stoppingDistance)
                    faceTarget();

                return true;
            }
        }
        return false;
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

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
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

        Instantiate(bullet, shootPos.position, transform.rotation);
    }
}
