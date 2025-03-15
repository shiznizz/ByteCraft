using System.Collections;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    private Animator animator; // Reference to the animator
    private bool isMeleeAttacking = false; // To track if we are in the middle of a melee attack
    private float meleeCooldown = 1f; // Time before we can attack again
    private float nextMeleeTime = 0f; // Cooldown tracking

    [SerializeField] private LayerMask damageLayer; // The layer to detect damageable targets
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Collider meleeCollider;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private GameObject weaponModel;

    private playerAttack playerAttackScript; // Reference to the playerAttack script

    void Start()
    {
        animator = GetComponent<Animator>(); // Get the animator component
        playerAttackScript = GetComponent<playerAttack>(); // Get reference to playerAttack script

        //Ensures the melee state is NOT locked at the start
        isMeleeAttacking = false;

        // Automatically find the camera if not set in the Inspector
        //if (cameraTransform == null)
        //{
        //    cameraTransform = Camera.main.transform;
        //}
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

        //Hide the gun when melee starts
        if (weaponModel != null)
        {
            weaponModel.SetActive(false);
        }

        // Trigger the melee attack animation
        animator.SetTrigger("MeleeAttack");

        // Enable melee collider
        turnOnCol();

        // Wait for the animation to finish (adjust the time based on animation length)
        yield return null;

        // Disable melee collider after attack
        turnOffCol();

        isMeleeAttacking = false; // Attack is finished

        // Show the gun again after melee finishes
        if (weaponModel != null)
            weaponModel.SetActive(true);
    }

    // Turn on melee collider (allowing attacks to hit enemies)
    public void turnOnCol()
    {
        meleeCollider.enabled = true;
        //Debug.Log("Melee Collider Enabled");
    }

    // Turn off melee collider (to prevent constant damage)
    public void turnOffCol()
    {
        meleeCollider.enabled = false;
        //Debug.Log("Melee Collider Disabled");
    }

    //Detect the melee hits when an enemy enters teh trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if we are currently attacking
        if (!isMeleeAttacking) return;

        // Check if the object is on the Enemy layer using the damageLayer mask
        if (((1 << other.gameObject.layer) & damageLayer) != 0)
        {
            IDamage damageable = other.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(playerStatManager.instance.attackDamage); // Apply damage
                Debug.Log($"Hit {other.gameObject.name} for {playerStatManager.instance.attackDamage} damage.");
            }
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }
}