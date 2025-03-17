using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossFightManager : MonoBehaviour
{
    [Header("References")]
    public BossSummon bossSummon; // Reference to the BossSummon script
    public PhaseTimer phaseTimer; // Reference to the PhaseTimer Script
    public BossInvulnerability bossInvulnerability; // Reference to the BossInvulnerability script
    public AudioSource bossAudioSource;  // Reference to the AudioSource component for the boss
    public Transform player; // Reference to the player transform
    public NavMeshAgent bossAgent; // NavMeshAgent component for movement
    public Animator anim; // Boss animator component

    [Header("Audio Clips")]
    public AudioClip[] bossVoiceLines;  // Array of audio clips for the boss' voice lines
    public AudioClip[] phaseTwoMechanicDialogue; // Array for phase two dialogue
    public AudioClip bossDeathDialogue; // Dialogue when the boss dies

    [Header("Settings")]
    public float bossTalkDuration = 3f; // Duration of the boss talking (in seconds)
    public float chaseSpeed = 3.5f; // Movement Speed
    public float attackRange = 3f; // Range to trigger AOE attack
    [SerializeField] private int bossHP = 200; // Boss HP (currently set to 200hp for phase two)
    [SerializeField] private float phaseTwoTimer = 30f; // Time between random mechanics is 30 seconds

    [Header("Animation Settings")]
    public float animTransSpeed = 5f; // Speed of animation transition

    private bool fightStarted = false;
    private bool isInPhaseOne = false; // Tracks if boss is in phase one
    private bool hasPlayedPhaseOneVoiceLine = false; // Tracks if the phase one voice line as been played
    private bool isShieldActive = false;
    private bool isInPhaseTwo = false;

    private void OnTriggerEnter(Collider other)
    {
        // Automatically start the boss fight
        if (!fightStarted)
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

        // Log Phase One end (for debugging)
        Debug.Log("Phase One ended, transitioning to Phase Two.");
    }

    void StartPhaseTwo()
    {
        isInPhaseTwo = true; // Marked as true for phase two
        bossHP = 200; // Resets boss HP for phase two
        phaseTwoTimer = 30f; // Resets phase mechanic timer

        if (bossAgent != null)
        {
            bossAgent.isStopped = false;
            bossAgent.speed = chaseSpeed;
        }

        StartCoroutine(PhaseTwoMechanicsCycle());
    }

    private void Update()
    {
        if (isInPhaseTwo && bossHP > 0 && player)
        {
            bossAgent.SetDestination(player.position);
            UpdateMovementAnimation();
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
                    StartCoroutine(SummonEnemies()); // Summon enemy wave
                    PlayMechanicDialogue(0); // Dialogue for Summon Enemies
                    break;
                case 1:
                    ActivateEnergyShield(); // Energy shield
                    PlayMechanicDialogue(1); // Dialogue for Energy Shield
                    break;
                case 2:
                    StartCoroutine(HeavyAOEAttack()); // AOE attack
                    PlayMechanicDialogue(2); // Dialogue for AOE Attack
                    break;
                case 3:
                    StartCoroutine(LaunchTrackingProjectiles()); // Tracking projectiles
                    PlayMechanicDialogue(3); // Dialogue for Tracking Projectiles
                    break;
            }

            if (bossAgent && player)
                bossAgent.SetDestination(player.position);

            // Reset the timer for the next mechanic
            phaseTwoTimer = 30f;
        }

        if (bossHP <= 0)
        {
            EndBossFight(); // Boss is defeated
        }
    }

    IEnumerator SummonEnemies()
    {
        Debug.Log("Boss summons enemy wave.");
        if (bossSummon != null)
        {
            bossSummon.enabled = true; // Enable enemy summoning
            yield return new WaitForSeconds(10f); // Allow enemies to spawn for 10 seconds
            bossSummon.enabled = false; // Disable enemy summoning after wave
        }
    }

    void ActivateEnergyShield()
    {
        Debug.Log("Boss activates energy shield.");
        if (!isShieldActive)
        {
            isShieldActive = true;
            bossInvulnerability.isInvulnerable = true; // Make the boss invulnerable due to shield
            StartCoroutine(ShieldDuration());
        }
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(10f); // Shield stays active for 10 seconds
        isShieldActive = false;
        bossInvulnerability.isInvulnerable = false; // Deactivate shield after time
        Debug.Log("Boss shield deactivated.");
    }

    IEnumerator HeavyAOEAttack()
    {
        Debug.Log("Boss performs heavy AOE attack.");
        // Implement the AOE attack effect here

        yield return new WaitForSeconds(1f); // Delay before next mechanic
    }

    IEnumerator LaunchTrackingProjectiles()
    {
        Debug.Log("Boss fires tracking projectiles.");
        // Create and launch tracking projectiles toward the player

        yield return new WaitForSeconds(1f); // Delay before next mechanic
    }

    public void TakeDamage(int damage)
    {
        if (!isInPhaseTwo || bossHP <= 0 || bossInvulnerability.isInvulnerable) return;

        bossHP -= damage;
        Debug.Log($"Boss took {damage} damage, remaining HP: {bossHP}");

        if (bossHP <= 0 && isInPhaseTwo)
        {
            EndBossFight();
        }
    }

    void EndBossFight()
    {
        Debug.Log("Boss has been defeated.");
        // Implement boss death behavior here (e.g., play death animation, reward the player)

        PlayVoiceLine(bossVoiceLines.Length - 1); // Play the final dialogue in array for death
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
}
