//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class IntroCharacterMovementController : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float moveSpeed = 3.5f; // Movement Speed
//    [SerializeField] private NavMeshAgent agent; // Reference to NavMeshAgent

//    [Header("Animation Settings")]
//    [SerializeField] private Animator anim; // Reference to Animator component

//    private float currentSpeed = 0f; // Current speed for the blend tree

//    // Start is called before the first frame update
//    void Start()
//    {
//        if (agent == null)
//            agent = GetComponent<NavMeshAgent>();

//        if (anim == null)
//            anim = GetComponent<Animator>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        HandleMovement();
//        HandleAnimation();
//    }

//    // Handle the movement of the character
//    void HandleMovement()
//    {
//        // You can change this logic to suit how you want to move the character (for example, based on player input)
//        // For now, we're using NavMeshAgent to move towards a destination.

//        // Example: Move the character to a random position
//        if (agent.remainingDistance <= agent.stoppingDistance)
//        {
//            Vector3 randomPosition = new Vector3(
//                transform.position.x + Random.Range(-10, 10),
//                transform.position.y,
//                transform.position.z + Random.Range(-10, 10)
//            );
//            agent.SetDestination(randomPosition);
//        }

//        // Update the current speed based on movement
//        currentSpeed = agent.velocity.magnitude;
//    }

//    // Handle the animation transitions (Idle/Walk)
//    void HandleAnimation()
//    {
//        // Pass the speed value to the Animator's 'speed' parameter
//        anim.SetFloat("Speed", currentSpeed);
//    }
//}
