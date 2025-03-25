using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyAI : MonoBehaviour, IDamage, lootDrop
{
    enum enemyType { range, melee, stationary, kamikaze }
    enum movementType { random, setPath, seeking, drone }
    enum spawnType { notSpawned, spawned }

    #region Variables
    [Header("General Enemy Settings")]
    [SerializeField] enemyType type;
    [SerializeField] movementType movement;
    [SerializeField] spawnType spawn;
    [SerializeField] public GameObject target;
    [SerializeField] Renderer model;
    [SerializeField] public NavMeshAgent agent;
    [SerializeField] Animator anim;
    //[SerializeField] private bool isDrone = false;
    [SerializeField] GameObject hpBarTarget;
    private GameObject originalTarget;
    private bool isKami;

    [Header("Enemy Stats")]
    [SerializeField] Image hpFillBar;
    [SerializeField] Canvas hpBar;
    [SerializeField] public int HP;
    [SerializeField] int animTransSpeed;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int FOV; //Field of View
    private int HPOrginal;
    [SerializeField] private int armor = 0;
    [SerializeField] private float speed = 0;

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

    [Header("Spawn settings")]
    [SerializeField] private GameObject spawnEffects;
    [SerializeField] private float invulnerableTime;
    [SerializeField] private bool godMode = false;

    public bool isDead = false;
    private Rigidbody rb;
    private Collider enemyCollider;
    private Renderer bodyRenderer;

    [Header("Audio")]
    //[SerializeField] AudioSource enemyAudio;
    [SerializeField] ModulatedSoundBank enemyHurtSounds;
    [SerializeField] ModulatedSoundBank enemyFootsteps;
    [SerializeField] ModulatedSoundBank enemyDeathSounds;
    [SerializeField] ModulatedSoundBank enemyAttackSounds;

    Vector3 startingPos;
    float roamTimer;
    float stoppingDistOrig;
    Vector3 targetDir;
    bool playerInRange;
    float angleToTarget;

    Color colorOrig;
    private bool isAlerted = false;
    private float alertTimer;
    private bool playerInDroneRange = false;
    private float alertCooldown = 5f;

    [SerializeField] private GameObject floatingDamageTextPrefab;
    [SerializeField] float textDestroyTimer;
    private Coroutine damageTextCoroutine;

    public bool isStunned = false;

    #endregion Variables


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrginal = HP;
        colorOrig = model.material.color;
        startingPos = transform.position;

        if (speed != 0) agent.speed = speed;
        
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

        if (movement != movementType.seeking)
        {
            target = gameManager.instance.player;
        }

        originalTarget = target;

        if (spawn == spawnType.spawned)
            onSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        
        updateEnemyUI();
        // if stunned, nav mesh will stop and skip rest of AI's logic
        if (agent.velocity.magnitude > 0.1f && !enemyFootsteps.IsPlaying())
        {
            enemyFootsteps.PlayRandomSound();
        }


        if (type != enemyType.stationary)
        {
            float agentSpeed = agent.velocity.normalized.magnitude; //for agent you are converting a vector 3 to a float by getting the magnitude
            float animatorCurSpeed = anim.GetFloat("Speed");

            anim.SetFloat("Speed", Mathf.MoveTowards(animatorCurSpeed, agentSpeed, Time.deltaTime * animTransSpeed));    // added multiplication by animTransSpeed to give control from editor
            
            if (agent.remainingDistance < 0.01f)
                roamTimer += Time.deltaTime;

            if (isStunned || godMode || isKami || isDead)
            {
                agent.isStopped = true;
                return;
            }
            else
            {
                agent.isStopped = false;
            }
        }

        shootTimer += Time.deltaTime;
        if (isAlerted && !playerInRange && type != enemyType.stationary) // specific to drone bot alerts
        {
            if (alertTimer < alertCooldown)
            {
                alertTimer += Time.deltaTime;
                target = gameManager.instance.player;
                SeekTarget();
            }
            else if (alertTimer >= alertCooldown)
            {
                alertTimer = 0;
                isAlerted = false;
            }
        }

        if (movement == movementType.seeking)
        {
            SeekTarget();
        }

        if (playerInRange && !canSeeTarget())
            checkRoam();
        else if (!playerInRange)
            checkRoam();
    }

    void updateEnemyUI()
    {
        hpFillBar.fillAmount = (float)HP / HPOrginal;
        hpBar.transform.LookAt(target.transform.position);
    }

    void onSpawn()
    {
        spawnEffects.SetActive(true);
        StartCoroutine(OnSpawn());
    }

    IEnumerator OnSpawn()
    {
        godMode = true;
        yield return new WaitForSecondsRealtime(invulnerableTime);
        godMode = false;
    }

    #region EnemyMovement

    void SeekTarget()
    {
        if (!canSeeTarget())
            agent.SetDestination(target.transform.position);
    }

    bool canSeeTarget()
    {
        targetDir = target.transform.position - headPos.position;
        angleToTarget = Vector3.Angle(new Vector3(targetDir.x, 0, targetDir.z), transform.forward);

        Debug.DrawRay(headPos.position, targetDir,Color.cyan);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, targetDir, out hit) && angleToTarget <= FOV)
        {
            if ((hit.collider.CompareTag("Player") || hit.collider.CompareTag("Target")) && angleToTarget <= FOV)
            {
                if (type != enemyType.stationary)
                {
                    agent.SetDestination(target.transform.position);
                }

                //Ranged attack
                if (type != enemyType.melee && type != enemyType.kamikaze && shootTimer >= shootRate && angleToTarget <= shootAngle && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
                {
                    //Debug.Log("Remaing:" + agent.remainingDistance);
                    shoot();
                }
                //Melee attack
                float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
                if (type == enemyType.melee && shootTimer >= shootRate && distanceToPlayer <= meleeDistance)
                //if (shootTimer >= shootRate && type == enemyType.melee && agent.remainingDistance <= meleeDistance) // Ensures attack happens when the shoot timer is ready
                {
                    meleeAttack();
                }
                //Kamaikaze
                if (type == enemyType.kamikaze && shootTimer >= shootRate && distanceToPlayer <= meleeDistance && !hasExploded)
                {
                    
                    kamikazeAttack();
                }
                if (agent.remainingDistance <= agent.stoppingDistance && !isDead)
                {
                    faceTarget();                 
                }

                agent.stoppingDistance = stoppingDistOrig;

                return true;
            }
        }
        //agent.stoppingDistance = 0;
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
                //kamikazeAttack();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            target = originalTarget;
        }

        if (agent != null)
            agent.stoppingDistance = 0;
    }

    void faceTarget()
    {
        targetDir = target.transform.position - headPos.position;
        Quaternion rot = Quaternion.LookRotation(new Vector3(targetDir.x, 0, targetDir.z));
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
        if (isDead || godMode) return;
        StartCoroutine(PlayHurtSound()); //if (isDrone) enemyAudio.PlayOneShot(hurtSound);

        if (movement == movementType.seeking)
        {
            target = gameManager.instance.player;
        }

        if (HP > 0)
        {
            StartCoroutine(enemyShowHpBar());
            int effectiveDamage = Mathf.Max(0, amount - armor);
            HP -= effectiveDamage;

            // debig log to confirm dmg is taken
            Debug.Log("Enemy took: " + amount + " damage");

            // instantiate the floating damage text only once when damage is taken.
            if (floatingDamageTextPrefab != null)
            {
                // set spawn position closer to the enemy 
                Vector3 spawnPos = headPos.transform.position + Vector3.up * 0.5f;
                // parent the floating text to the enemy so it moves with the enemy.
                GameObject dmgText = Instantiate(floatingDamageTextPrefab, spawnPos, Quaternion.identity, transform);
                Destroy(dmgText, textDestroyTimer);

                FloatingDamageText fdt = dmgText.GetComponent<FloatingDamageText>();
                if (fdt != null)
                {
                    fdt.SetText(amount.ToString());
                }
            }

            // start coroutine to repeatedly spawn floating text 
            //if (damageTextCoroutine == null)
            //{
            //    damageTextCoroutine = StartCoroutine(DamageTextLoop(amount));
            //}

            StartCoroutine(flashRed());
            if (anim != null)
                anim.SetTrigger("damage");


            if (type != enemyType.stationary)
            {
                agent.SetDestination(target.transform.position);
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
                GameEventsManager.instance.miscEvents.EnemyKilled();

                if (dropsLoot)
                    dropLoot();

                handleDeath();

                // stop looping dmg text coroutine since enemy is dead
                if (damageTextCoroutine != null)
                {
                    StopCoroutine(damageTextCoroutine);
                    damageTextCoroutine = null;
                }
            }
        }
    }

    //// coroutine that spawns text until enemy dies
    //private IEnumerator DamageTextLoop(int damage)
    //{
    //    // loop until HP reaches 0
    //    while (HP > 0)
    //    {
    //        if (floatingDamageTextPrefab != null)
    //        {
    //            // set spawn location near enemy 
    //            Vector3 spawnPos = transform.position + Vector3.up * 1f;

    //            // instantiate prefab and set parent to enemy so it follows enemy
    //            GameObject dmgText = Instantiate(floatingDamageTextPrefab, spawnPos, Quaternion.identity, transform);

    //            // debug log to confirm instantiation
    //            Debug.Log("Instantiated looping floating text!");

    //            // set dmg amount text
    //            FloatingDamageText fdt = dmgText.GetComponent<FloatingDamageText>();
    //            if (fdt != null)
    //            {
    //                fdt.SetText(damage.ToString());
    //            }
    //        }
    //        // wait for set interval before spawning next text
    //        yield return new WaitForSeconds(1f);
    //    }
    //    damageTextCoroutine = null; // clear the reference
    //}

    private void handleDeath()
    {
        hpBar.gameObject.SetActive(false);
        this.GetComponent<CapsuleCollider>().enabled = false;
        //Debug.Log("Hitting handle death.");
        AlarmDrone droneScript = GetComponent<AlarmDrone>();
        StartCoroutine(PlayDeathSound());
        //Disable the collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false; //Disables the collider to avoid further interactions
        }

        //Disables movement and enables physics
        if (rb != null)
        {
            rb.isKinematic = false; // Enable physics
            rb.useGravity = true; // Allow gravity to affect the body

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
        PlayWeaponSound();
        if (anim != null)
            anim.SetTrigger("Shoot");
        else
            createProjectile();
    }

    public void createProjectile()
    {
        //Creates a projectile at shootPos with the same rotation as the enemy
        GameObject newBullet = Instantiate(bullet, shootPos.position, transform.rotation);
        newBullet.GetComponent<damage>().updateTarget(target);
    }

    void meleeAttack()
    {
        if (isDead) return;
        StartCoroutine(PlayWeaponSound());
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
        isKami = true;
        
        // Ensure the Kamikaze starts moving towards the player
        agent.SetDestination(target.transform.position);

        // Check if within melee range to trigger detonation
        if (Vector3.Distance(transform.position, target.transform.position) <= meleeDistance)
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
        if (Vector3.Distance(transform.position, target.transform.position) <= meleeDistance)
        {
            // scale explosion damage based on difficulty
            int explosionDamage = 25;  // Base explosion damage
            if (DifficultyManager.instance != null)
            {
                explosionDamage = Mathf.RoundToInt(explosionDamage * DifficultyManager.instance.enemyDamageMultiplier);
            }
            gameManager.instance.playerScript.takeDamage(explosionDamage); // Adjust the explosion damage as needed
        }


        // Destroy the Kamikaze enemy after explosion
        Destroy(gameObject);
    }

    public void dropLoot()
    {
        playerController player = gameManager.instance.playerScript;
        float healthRatio = playerStatManager.instance.HP / (float)playerStatManager.instance.HPMax;
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

    #region DroneBotResponse

    public void SetAlerted(bool state)
    {
        isAlerted = state;
        if (isAlerted)
        {
            alertTimer = 0f;
        }
    }

    public void SetPlayerInDroneRange(bool state)
    {
        playerInDroneRange = state;
    }

    #endregion

    #region AOESupport
    public void SetHP(int newHP)
    {
        this.HP = newHP;
    }
    #endregion

    #region Audio
    IEnumerator PlayHurtSound()
    {
        if (enemyHurtSounds != null)
        {
            float clipDuration = enemyHurtSounds.GetClipDuration();
            enemyHurtSounds.PlayCurrentClip();
            yield return new WaitForSeconds(clipDuration);
        }
    }

    IEnumerator PlayMovementSound()
    {
        if (enemyFootsteps != null)
        {
            float clipDuration = enemyDeathSounds.GetClipDuration();
            enemyDeathSounds.PlayCurrentClip();
            yield return new WaitForSeconds(clipDuration);
        }
    }

    IEnumerator PlayDeathSound()
    {
        if (enemyDeathSounds != null)
        {
            float clipDuration = enemyDeathSounds.GetClipDuration();
            enemyDeathSounds.PlayCurrentClip();
            yield return new WaitForSeconds(clipDuration);
        }
    }

    IEnumerator PlayWeaponSound()
    {
        if (enemyAttackSounds != null)
        {
            float clipDuration = enemyAttackSounds.GetClipDuration();
            enemyAttackSounds.PlayCurrentClip();
            yield return new WaitForSeconds(clipDuration);
        }
    }
    #endregion
}
