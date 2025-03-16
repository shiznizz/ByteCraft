using UnityEngine;

public class PhaseTimer : MonoBehaviour
{
    public float phaseDuration = 60f; // Currently set to 3 minutes
    private float remainingTime;

    public BossInvulnerability bossInvulnerabilityScript;
    public BossFightManager bossFightManager; // Reference to the BossFightManager

    private void Start()
    {
        remainingTime = phaseDuration;

        // Ensure we have the necessary references
        if (bossInvulnerabilityScript == null)
            bossInvulnerabilityScript = GetComponent<BossInvulnerability>();  // Get the invulnerability script attached to the boss

        if (bossFightManager == null)
            bossFightManager = GetComponent<BossFightManager>();  // Get the boss fight manager script (should be on the same object)
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;

        if ( remainingTime <= 0 )
        {
            EndPhase();
        }
    }

    // Handle end of phase
    void EndPhase()
    {
        // Ensure we are actually in Phase 1 before ending it
        if (bossInvulnerabilityScript.isInvulnerable)
        {
            // End the invulnerability (i.e., phase 1 ends)
            bossInvulnerabilityScript.EndInvulnerability();

            // Log that phase 1 has ended (for debugging)
            Debug.Log("Phase 1 ended, boss is now vulnerable.");

            // You could trigger phase 2 logic here
        }
    }
}
