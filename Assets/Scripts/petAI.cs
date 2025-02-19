using UnityEngine;
using UnityEngine.AI;

public class petAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player; // Reference to the player
    [SerializeField] NavMeshAgent agent; // NavMesh agent to move the pet
    [SerializeField] Animator anim; // Optional for animations

    [Header("Movement Settings")]
    [SerializeField] float followDistance = 3f; // How close the pet will follow the player
    [SerializeField] float stopDistance = 1f; // Minimum distance the pet stops at when near the player

    [Header("Idle Behavior")]
    [SerializeField] float idleTime = 2f; // Time the pet waits before performing idle animation
    private float idleTimer;

    private bool isFollowing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform; // If no player is assigned, find the player tag
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Always update the idle timer if pet is following
        if (!isFollowing)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleTime)
            {
                //You can add some idle behavior or animations here
                idleTimer = 0f; // Resets timer after idle time
            }
        }

        FollowPlayer();
    }

    void FollowPlayer()
    {
        //Checks the distance between pet and player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //If too far away, starts to follow the player
        if (distanceToPlayer > followDistance)
        {
            isFollowing = true;
            agent.SetDestination(player.position); // Moves towards player
            if(anim != null)
            {
                anim.SetFloat("Speed", 1f); // Optional, can set animation speed if you have animation
            }
        }
        else if(distanceToPlayer <= followDistance && distanceToPlayer > stopDistance)
        {
            isFollowing = true;
            agent.SetDestination(transform.position); // Stops movement
            if( anim != null )
            {
                anim.SetFloat("Speed", 0f); // Optional, can set idle animation speed
            }
        }
        //If very close, stop and stay idle
        else
        {
            isFollowing = false;
            agent.SetDestination(transform.position); // Stays still near player
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f); // Optional, can set idle animation speed
            }
        }
    }
}
