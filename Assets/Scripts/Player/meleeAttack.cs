using System.Collections;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    private Animator animator; // Reference to the animator
    private bool isMeleeAttacking = false; // To track if we are in the middle of a melee attack
    private float meleeCooldown = 1f; // Time before we can attack again
    private float nextMeleeTime = 0f; // Cooldown tracking

    [SerializeField] private LayerMask damageLayer; // The layer to detect damageable targets
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform orientation;

    private playerAttack playerAttackScript; // Reference to the playerAttack script
    private Camera playerCamera;

    void Start()
    {
        animator = GetComponent<Animator>(); // Get the animator component
        playerAttackScript = GetComponent<playerAttack>(); // Get reference to playerAttack script
        playerCamera = Camera.main;
    }

    void Update()
    {
        HandleMeleeInput(); // Handle melee input on each frame
    }

    // Handles the input for melee attack
    void HandleMeleeInput()
    {
        if (Time.time >= nextMeleeTime && !isMeleeAttacking)
        {
            if (Input.GetKeyDown(KeyCode.F)) // Check for F key input
            {
                nextMeleeTime = Time.time + meleeCooldown; // Set next attack time
                StartCoroutine(MeleeAttackRoutine()); // Start melee attack
            }
        }
    }

    IEnumerator MeleeAttackRoutine()
    {
        isMeleeAttacking = true; // Set attacking to true

        // Disable guns during melee attack
        playerAttackScript.DisableWeapons();

        // Trigger the melee attack animation
        animator.SetTrigger("MeleeAttack");

        // Detect melee hit
        PerformMeleeHitDetection();

        // Wait for the animation to finish (adjust the time based on animation length)
        yield return new WaitForSeconds(0.5f);

        isMeleeAttacking = false; // Attack is finished

        // Re-enable guns after melee attack
        playerAttackScript.EnableWeapons();
    }

    // Perform melee hit detection
    void PerformMeleeHitDetection()
    {
        RaycastHit hit;

        Vector3 attackOrigin = attackPoint != null ? attackPoint.position : transform.position;
        Vector3 attackDirection = orientation.forward;

        if (Physics.Raycast(attackOrigin, attackDirection, out hit, 2f, damageLayer)) // Adjust range if needed
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(playerStatManager.instance.attackDamage); // Apply damage
            }
        }
    }
}