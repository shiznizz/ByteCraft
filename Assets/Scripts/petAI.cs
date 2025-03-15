//using UnityEngine;
//using UnityEngine.AI;
//using TMPro;  // If you're using TextMeshPro

//public class petAI : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] Transform player; // Reference to the player
//    [SerializeField] NavMeshAgent agent; // NavMesh agent to move the pet
//    [SerializeField] Animator anim; // Optional for animations
//    [SerializeField] TextMeshProUGUI interactionText; // Reference to the UI TextMeshPro (showing "Press P to Pet")

//    [Header("Movement Settings")]
//    [SerializeField] float followDistance = 3f; // How close the pet will follow the player (Adjustable in Inspector)
//    [SerializeField] float stopDistance = 1f; // Minimum distance the pet stops at when near the player

//    [Header("Idle Behavior")]
//    [SerializeField] float idleTime = 2f; // Time the pet waits before performing idle animation
//    private float idleTimer;

//    private bool isFollowing = false;
//    private bool isInRange = false; // Check if the player is in range

//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        if (player == null)
//        {
//            player = GameObject.FindWithTag("Player").transform; // If no player is assigned, find the player tag
//        }

//        // Initially hide the text
//        if (interactionText != null)
//        {
//            interactionText.gameObject.SetActive(false); // Hide the interaction text at the start
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        FollowPlayer();

//        // Check for "P" key press and if pet is within range and ready for interaction
//        if (isInRange && Input.GetKeyDown(KeyCode.P))
//        {
//            PerformSpin();
//        }
//    }
//        void FollowPlayer()
//    {
//        // Checks the distance between pet and player
//        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

//        // If too far away, starts to follow the player
//        if (distanceToPlayer > followDistance)
//        {
//            isFollowing = true;
//            agent.SetDestination(player.position); // Moves towards player
//            if (anim != null)
//            {
//                anim.SetFloat("Speed", 1f); // Optional, can set animation speed if you have animation
//            }
//        }
//        else if (distanceToPlayer <= followDistance && distanceToPlayer > stopDistance)
//        {
//            isFollowing = true;
//            agent.SetDestination(transform.position); // Stops movement
//            if (anim != null)
//            {
//                anim.SetFloat("Speed", 0f); // Optional, can set idle animation speed
//            }
//        }
//        // If very close, stop and stay idle
//        else
//        {
//            isFollowing = false;
//            agent.SetDestination(transform.position); // Stays still near player
//            if (anim != null)
//            {
//                anim.SetFloat("Speed", 0f); // Optional, can set idle animation speed
//            }
//        }
//    }

//    // Method to perform the spin animation when "P" key is pressed
//    void PerformSpin()
//    {
//        if (anim != null)
//        {
//            anim.SetTrigger("SpinTrigger"); // Trigger the spin animation
//        }
//    }

//    // Triggered when the player enters the trigger collider area
//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))  // Check if the object entering the trigger is the player
//        {
//            isInRange = true; // Player is in range for interaction
//            if (interactionText != null)
//            {
//                interactionText.gameObject.SetActive(true); // Show the interaction text
//            }
//        }
//    }

//    // Triggered when the player exits the trigger collider area
//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))  // Check if the object exiting the trigger is the player
//        {
//            isInRange = false; // Player is out of range for interaction
//            if (interactionText != null)
//            {
//                interactionText.gameObject.SetActive(false); // Hide the interaction text
//            }
//        }
//    }
//}
