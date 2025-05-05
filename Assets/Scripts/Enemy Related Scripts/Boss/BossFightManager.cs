using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BossFightManager : MonoBehaviour, IDamage
{
    [Header("References")]
    public BossSummon bossSummon; // Reference to the BossSummon script
    public PhaseTimer phaseTimer; // Reference to the PhaseTimer Script
    public BossInvulnerability bossInvulnerability; // Reference to the BossInvulnerability script
    public AudioSource bossAudioSource;  // Reference to the AudioSource component for the boss
    public Transform player; // Reference to the player transform
    public NavMeshAgent bossAgent; // NavMeshAgent component for movement
    public Animator anim; // Boss animator component
    public Renderer bossRenderer; // For flashing effect when hit

    [SerializeField] Image hpFillBar;
    [SerializeField] Image invulnerableHpFillBar;
    [SerializeField] Image hpBar;

    [Header("Audio Clips")]
    public AudioClip[] bossVoiceLines;  // Array of audio clips for the boss' voice lines
    public AudioClip[] phaseTwoMechanicDialogue; // Array for phase two dialogue
    public AudioClip bossDeathDialogue; // Dialogue when the boss dies

    [Header("Settings")]
    [SerializeField] private int bossHP = 200; // Boss HP (currently set to 200hp for phase two)
    public float hitFlashDuration = 0.2f; // Time the boss flashes when hit
    public float bossTalkDuration = 3f; // Duration of the boss talking (in seconds)
    public float chaseSpeed = 3.5f; // Movement Speed
    public float attackRange = 3f; // Range to trigger AOE attack
    [SerializeField] private float phaseTwoTimer; // Time between random mechanics is 30 seconds

    [Header("Animation Settings")]
    public float animTransSpeed = 5f; // Speed of animation transition
    public GameObject shieldEffect; // Assign shield effect here

    //[Header("Heavy AOE Attack Settings")]
    //public GameObject aoeWarningEffect; // The warning effect that shows where the attack will hit
    //public GameObject aoeExplosionEffect; // The explosion effect
    //public float aoeRadius = 5f; // Radius of the AOE attack
    //public int aoeDamage = 30; // Damage the attack deals
    //public float aoeWarningTime = 2f; // Time before explosion
    //public float aoeAttackCooldown = 10f; // Cooldown before the boss can use the attack again

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;  // The bullet prefab to instantiate
    public Transform shootPos;       // Position where bullet comes from
    public float bulletSpeed = 10f;  // Speed of the bullet    
    private float shootTimer = 0f;   // Timer for shooting cooldown
    public float shootRate = 1f;     // Rate at which the boos shoots


    [Header("Basic Attack Settings")]
    public float basicAttackRange = 5f;
    public int basicAttackDamage = 5;
    public float attackCooldown = 2f;
    private bool isAttacking = false;

    [Header("Testing Flags")]
    public bool testSummonEnemies = true;  // Toggle to test Summon Enemies
    public bool testActivateEnergyShield = true;  // Toggle to test Energy Shield
    public bool testHeavyAOEAttack = true;  // Toggle to test Heavy AOE Attack
    public bool testLaunchTrackingProjectiles = true;  // Toggle to test Tracking Projectiles

    private bool fightStarted = false;
    private bool isInPhaseOne = false; // Tracks if boss is in phase one
    private bool hasPlayedPhaseOneVoiceLine = false; // Tracks if the phase one voice line as been played
    private bool isShieldActive = false;
    private bool isInPhaseTwo = false;

    [Header("UI References")]
    [SerializeField] TextMeshProUGUI subtitleText;
    [SerializeField] private GameObject contentParent;
    private string[] subtitles = { "Well, well, well. What do we have here? Another foolish hero thinking they can take me down? How utterly predictable.", "You think you can defeat me with ease? Let me show you the power of my minions!", "Enough! The time for petty distractions is over. Prepare yourself for the real challenge!", "You may have won this round, but I'll return stronger!" };

    private void OnTriggerEnter(Collider other)
    {
        // Automatically start the boss fight
        if (!fightStarted && other.CompareTag("Player")) // Make sure the player has the correct Tag!
        {
            StartCoroutine(StartBossFight());
        }
    }

    IEnumerator StartBossFight()
    {
        fightStarted = true;

        //Trigger the boss talk animation/sequence here

        // Start the boss dialogue (you can adjust when it starts depending on your desired flow)
        PlayVoiceLine(0);  // Play first boss voice line when the fight starts

        //Wait for the boss to finish talking
        yield return new WaitForSeconds(bossTalkDuration);

        //Starts the first phase of the fight (summoning enemies)
        StartPhaseOne();
    }

    void StartPhaseOne()
    {
        // Ensure health bar is visible from the start of Phase One
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);  // Make sure the health bar is visible at all times
        }

        // Set the health bar to full when the phase starts
        if (invulnerableHpFillBar != null)
        {
            invulnerableHpFillBar.fillAmount = 1f;
        }

        // Prevent phase one voice line from playing multiple times
        if (!hasPlayedPhaseOneVoiceLine)
        {
            // Play voice line when the boss enters Phase 1
            PlayVoiceLine(1);  // Play a different voice line for Phase 1 start
            hasPlayedPhaseOneVoiceLine = true; // Mark as played

            //Start summoning enemeies set time
            // Enable the boss summoning and phase timer
            if (bossSummon != null) bossSummon.enabled = true;
            if (phaseTimer != null) phaseTimer.enabled = true;
            if (bossInvulnerability != null) bossInvulnerability.isInvulnerable = true; // Make the boss invulnerable during Phase One
        }

        // Start the shield effect for Phase One
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);  // Turn on shield effect for Phase One
        }

        //Starts the EndPhaseOne coroutine to disable the summoning after 3 minutes
        StartCoroutine(EndPhaseOneAfterTime());
    }

    IEnumerator EndPhaseOneAfterTime()
    {
        // Wait for the set time and then stop summoning enemies
        yield return new WaitForSeconds(phaseTimer.phaseDuration);

        // Check if the voice line for Phase One end is already played
        if (!isInPhaseOne)
        {
            // Play voice line for Phase 1 end
            PlayVoiceLine(2);  // Play voice line for Phase 1 end
            isInPhaseOne = true;
        }

        // End Phase One and transition to the next phase
        EndPhaseOne();
        StartPhaseTwo();
    }

    void EndPhaseOne()
    {
        // Disable the summoning script after Phase One ends
        if (bossSummon) bossSummon.enabled = false;

        // Disable invulnerability and prepare for Phase Two
        if (bossInvulnerability) bossInvulnerability.EndInvulnerability();

        // Remove the shield effect at the end of Phase One
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);  // Turn off the shield effect after Phase One ends
        }

        // Log Phase One end (for debugging)
        //Debug.Log("Phase One ended, transitioning to Phase Two.");
    }

    void StartPhaseTwo()
    {
        isInPhaseTwo = true; // Marked as true for phase two
        bossHP = 200; // Resets boss HP for phase two
        phaseTwoTimer = 20f; // Resets phase mechanic timer

        if (bossAgent != null)
        {
            bossAgent.isStopped = false;
            bossAgent.speed = chaseSpeed;
        }

        // Force boss to be vulnerable at start of phase two
        if (bossInvulnerability != null)
        {
            bossInvulnerability.isInvulnerable = false;
            //Debug.Log("Phase Two started: Boss is now vulnerable!");
        }

        StartCoroutine(PhaseTwoMechanicsCycle());
    }

    void Update()
    {
        // Prevent boss actions if dead
        if (bossHP <= 0)
        {
            if (!isInPhaseTwo)  // Make sure Phase Two is not still active after death
            {
                // Additional logic for boss death can go here if needed
                return;
            }
        }

        if (isInPhaseTwo && player)
        {
            bossAgent.SetDestination(player.position);
            UpdateMovementAnimation();
            shootTimer += Time.deltaTime;  // Increment shoot timer

            // Update health bar in each frame (optional)
            updateEnemyUI();

            // Handle attack mechanics
            if (Vector3.Distance(player.position, transform.position) <= attackRange && shootTimer >= shootRate && !isAttacking)
            {
                shoot();  // Call the shoot method
                shootTimer = 0f;  // Reset the timer after shooting
            }
        }
    }

    void updateEnemyUI()
    {
        //if (hpFillBar != null)
        //{
        //    hpFillBar.fillAmount = (float)bossHP / 200f;
        //}

        // Update health bar fill based on current boss HP (for the regular health bar)
        if (hpFillBar != null && !isShieldActive)
        {
            // Normal health bar
            hpFillBar.fillAmount = (float)bossHP / 200f;
        }

        // Update invulnerable health bar when shield is active
        if (invulnerableHpFillBar != null)
        {
            // Show invulnerable health bar when shield is active
            if (isShieldActive)  // If shield is active
            {
                invulnerableHpFillBar.gameObject.SetActive(true);  // Ensure the invulnerable bar is visible
                invulnerableHpFillBar.fillAmount = (float)bossHP / 200f;  // Sync the blue bar with current health
                invulnerableHpFillBar.color = Color.blue;  // Make it blue to indicate invulnerability
            }
            else
            {
                invulnerableHpFillBar.gameObject.SetActive(false);  // Hide when not invulnerable
            }
        }
    }

    private void UpdateMovementAnimation()
    {
        if (anim != null && bossAgent != null)
        {
            float agentSpeed = bossAgent.velocity.magnitude; // Convert velocity to a float speed
            float animatorCurSpeed = anim.GetFloat("Speed");

            // Smoothly transition animation speed
            anim.SetFloat("Speed", Mathf.MoveTowards(animatorCurSpeed, agentSpeed, Time.deltaTime * animTransSpeed));
        }
    }

    IEnumerator PhaseTwoMechanicsCycle()
    {
        while (isInPhaseTwo && bossHP > 0)
        {
            yield return new WaitForSeconds(phaseTwoTimer);

            // Randomly choose one of the four mechanics to activate
            int randomMechanic = Random.Range(0, 4);
            switch (randomMechanic)
            {
                case 0:
                    if (testSummonEnemies) // Check if Summon Enemies is enabled
                    {
                        StartCoroutine(SummonEnemies()); // Summon enemy wave
                        PlayMechanicDialogue(0); // Dialogue for Summon Enemies
                    }
                    break;

                case 1:
                    if (testActivateEnergyShield) // Check if Energy Shield is enabled
                    {
                        ActivateEnergyShield(); // Energy shield
                        PlayMechanicDialogue(1); // Dialogue for Energy Shield
                    }
                    break;

                case 2:
                    if (testHeavyAOEAttack) // Check if Heavy AOE Attack is enabled
                    {
                        StartCoroutine(HeavyAOEAttack()); // AOE attack
                        PlayMechanicDialogue(2); // Dialogue for AOE Attack
                    }
                    break;

                case 3:
                    if (testLaunchTrackingProjectiles) // Check if Tracking Projectiles is enabled
                    {
                        StartCoroutine(LaunchTrackingProjectiles()); // Tracking projectiles
                        PlayMechanicDialogue(3); // Dialogue for Tracking Projectiles
                    }
                    break;
            }

            if (bossAgent && player)
                bossAgent.SetDestination(player.position);

            // Reset the timer for the next mechanic
            phaseTwoTimer = 10f;
        }

        if (bossHP <= 0)
        {
            EndBossFight(); // Boss is defeated
        }
    }

    void shoot()
    {
        //Debug.Log("Attempting to shoot...");
        shootTimer = 0;

        if (anim != null)
            anim.SetTrigger("Shoot");
        else
            createProjectile();
    }

    public void createProjectile()
    {
        if (bulletPrefab != null && shootPos != null)
        {
            GameObject newBullet = Instantiate(bulletPrefab, shootPos.position, transform.rotation);
            newBullet.GetComponent<damage>().updateTarget(player.gameObject);
        }
    }

    IEnumerator SummonEnemies()
    {
        //Debug.Log("Boss summons enemy wave.");
        if (bossSummon != null)
        {
            bossSummon.enabled = true; // Enable enemy summoning
            yield return new WaitForSeconds(10f); // Allow enemies to spawn for 10 seconds
            bossSummon.enabled = false; // Disable enemy summoning after wave
        }
    }

    void ActivateEnergyShield()
    {
        //Debug.Log("Boss activates energy shield.");
        if (!isShieldActive)  // Check if shield is not already active
        {
            isShieldActive = true;  // Mark shield as active
            bossInvulnerability.isInvulnerable = true;  // Make boss invulnerable

            // Update the invulnerable health bar to show remaining health (in blue)
            if (invulnerableHpFillBar != null)
            {
                invulnerableHpFillBar.gameObject.SetActive(true);  // Ensure the invulnerable bar is visible
                invulnerableHpFillBar.fillAmount = (float)bossHP / 200f;  // Sync the health with the regular health bar
                invulnerableHpFillBar.color = Color.blue;  // Set color to blue
            }

            if (shieldEffect != null)
            {
                shieldEffect.SetActive(true);  // Turn on shield effect
            }

            // Start the shield duration timer
            StartCoroutine(ShieldDuration());
        }
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(10f); // Shield stays active for 10 seconds
        isShieldActive = false;
        bossInvulnerability.isInvulnerable = false; // Deactivate shield after time

        if (shieldEffect != null)
            shieldEffect.SetActive(false); // Disable shield effect

        // After shield is removed, hide the invulnerable HP bar and switch back to regular health bar
        if (invulnerableHpFillBar != null)
        {
            invulnerableHpFillBar.gameObject.SetActive(false);  // Hide the invulnerable health bar when shield is no longer active
        }

        //Debug.Log("Boss shield deactivated.");
    }

    IEnumerator HeavyAOEAttack()
    {
        //Ensure that the BossSummon script is enabled
        if (bossSummon != null)
        {
            bossSummon.enabled = true;  // Enable the BossSummon script to allow bomb summoning

            //Summon bombs at random locations after a brief delay (or immediately)
            yield return new WaitForSeconds(1f);  // Delay before summoning bombs (you can adjust this)

            //Call the method to summon bombs at random spawn points
            bossSummon.SummonBombs();

            //Wait for a brief duration before ending the attack
            yield return new WaitForSeconds(1f); // Bombs are summoned, so give time for visual effects

            //Disable the BossSummon script after the bomb summoning is complete
            bossSummon.enabled = false;
        }
        else
        {
            Debug.LogError("BossSummon script is not assigned or missing!");
        }
    }

    IEnumerator LaunchTrackingProjectiles()
    {
        //Debug.Log("Boss fires tracking projectiles.");
        // Create and launch tracking projectiles toward the player

        yield return new WaitForSeconds(1f); // Delay before next mechanic
    }

    IEnumerator bossShowHpBar()
    {
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);  // Show the health bar

            // Optional: If you want to hide the health bar when the boss dies, you can check the boss's HP here
            if (bossHP <= 0)
            {
                hpBar.gameObject.SetActive(false);  // Hide if the boss dies
            }
        }

        // Wait for a moment to keep the bar visible long enough
        yield return new WaitForSecondsRealtime(5f);
    }

    public void takeDamage(int damage)
    {
        //Debug.Log("Boss received damage: " + damage);
        TakeDamage(damage);  // Calls existing TakeDamage method
    }

    void TakeDamage(int damage)
    {
        // Ensure boss only takes damage in Phase Two and when not dead
        if (!isInPhaseTwo || bossHP <= 0) return;

        // Reduce HP
        bossHP -= damage;
        //Debug.Log($"Boss took {damage} damage. Remaining HP: {bossHP}");

        StartCoroutine(bossShowHpBar());

        // Flash effect to show hit
        StartCoroutine(FlashOnHit());

        if (anim != null)
            anim.SetTrigger("damage");

        // Check if boss is dead
        if (bossHP <= 0)
        {
            StartCoroutine(HandleDeath());
            //EndBossFight();
        }
    }

    private IEnumerator FlashOnHit()
    {
        if (bossRenderer == null)
        {
            //Debug.LogError("Boss Renderer is missing! Assign the Renderer in the Inspector.");
            yield break; // Stop function if no Renderer exists
        }

        //Debug.Log("Boss hit! Flashing red.");
        Color originalColor = bossRenderer.material.color;
        bossRenderer.material.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);
        bossRenderer.material.color = originalColor;
    }

    private IEnumerator HandleDeath()
    {
        //Debug.Log("Boss has been defeated. Initiating death sequence...");

        isInPhaseTwo = false; // Stop Phase Two
        bossAgent.isStopped = true; // Stop movement
        if (bossInvulnerability != null) bossInvulnerability.isInvulnerable = true; // Make sure no extra hits register

        if (anim != null)
        {
            anim.SetTrigger("Death"); // Trigger death animation
        }

        // Destroy the health bar when the boss dies
        if (hpBar != null)
        {
            Destroy(hpBar.gameObject); // Destroy the Canvas containing the health bar
        }

        PlayVoiceLine(bossVoiceLines.Length - 1); // Play death dialogue

        //yield return new WaitForSeconds(3f); // Adjust based on death animation length
        
        // Wait for the boss dialogue to finish
        yield return new WaitForSeconds(bossVoiceLines[bossVoiceLines.Length - 1].length);

        // After the dialogue finishes, play the end scene
        StartCoroutine(PlayEndScene());

        // Stop any further boss actions after death
        //EndBossFight();
        //gameManager.instance.youWin();
        //StartCoroutine(FadeOutAndDestroy());
    }

    //This will handle the transition to the end scene
    private IEnumerator PlayEndScene()
    {
        //Assuming EndSceneController is attached to an object in the scene
        EndSceneController endSceneController = Object.FindFirstObjectByType<EndSceneController>();

        //Play the timeline (EndSceneController will handle the timeline logic)
        if (endSceneController != null)
        {
            endSceneController.timelineDirector.Play();

            //Wait for the timeline to finish, depending on the desired behavior
            yield return new WaitUntil(() => endSceneController.timelineDirector.time >= endSceneController.timelineDirector.duration);
        }

        //Load the credits scene
        SceneManager.LoadScene("9 Game Ending");
    }

    void EndBossFight()
    {
        //Debug.Log("Boss has been defeated.");
        // Implement boss death behavior here (e.g., play death animation, reward the player)

        //PlayVoiceLine(bossVoiceLines.Length - 1); // Play the final dialogue in array for death
        isInPhaseTwo = false;
        bossAgent.isStopped = true; // Stop boss movement
    }

    // Function to play a specific voice line based on index
    void PlayVoiceLine(int lineIndex)
    {
        if (lineIndex >= 0 && lineIndex < bossVoiceLines.Length)
        {
            // Play the specific voice line if it exists
            bossAudioSource.clip = bossVoiceLines[lineIndex];
            bossAudioSource.Play();

            StartCoroutine(DisplaySubtitles(bossVoiceLines[lineIndex].length, lineIndex));
        }
    }

    // Play the appropriate dialogue for the mechanic in Phase Two
    void PlayMechanicDialogue(int mechanicIndex)
    {
        if (mechanicIndex >= 0 && mechanicIndex < phaseTwoMechanicDialogue.Length)
        {
            bossAudioSource.clip = phaseTwoMechanicDialogue[mechanicIndex];
            bossAudioSource.Play();
        }
    }

    public int getBossHP()
    {
        return bossHP;
    }

    IEnumerator DisplaySubtitles(float voiceLength, int lineIdx)
    {
        contentParent.SetActive(true);
        string dialogueLine = subtitles[lineIdx];
        if (dialogueLine != null)
        {
            subtitleText.text = dialogueLine;
            yield return new WaitForSeconds(voiceLength);
            subtitleText.text = string.Empty;
        }
        contentParent.SetActive(false);
    }
}
