using UnityEngine;

public class WallRunning : MonoBehaviour
{

    private playerController pc;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
       if(!pc.isGrounded)
            checkWall();
    }

    private void FixedUpdate()
    {
        
    }

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 forwardDir;

    [Header("Wallrunning")]
    public LayerMask wallLayer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance = 1f;
    private RaycastHit rightWallHit;
    private RaycastHit leftWallHit;
    private bool wallRight;
    private bool wallLeft;
    private Vector3 wallNormal;

    [Header("Exiting")]
    private bool isExitingWall;
    private float exitWallTimer;

    [Header("References")]
    public Transform orientation;

    public float wallRunAcceleration = 10f;
    //public float maxWallRunSpeed = 10f;
    public float wallRunTime = 1.5f;
    public float wallJumpForce = 12f;

   float wallRunTimer;

    private void checkWall()
    {
        RaycastHit hit;

        // checks if player is next to a left or right wall then enters or exits wall running state accordingly
        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);

        if ((wallRight || wallLeft) && !pc.isWallRunning)
            wallRun();
        if ((!wallRight && !wallLeft) && pc.isWallRunning)
            stopWallRun();
    }

    private void wallRun()
    {
        // reset jumps, start wallrun, cancel gravity
        playerStatManager.instance.jumpCount = 0;
        StartWallRun();
        playerVelocity = Vector3.zero;

        // checks wall normal and sets wall normal to left or right wall normal then updates the forwareDir
        wallNormal = wallLeft ? leftWallHit.normal : rightWallHit.normal;
        forwardDir = Vector3.Cross(wallNormal, Vector3.up);

        // if on left wall go backwards
        if (Vector3.Dot(forwardDir, leftWallHit.normal) < 0)
            forwardDir = -forwardDir;
    }

    private void StartWallRun()
    {
        pc.isWallRunning = true;
        wallRunTimer = playerStatManager.instance.maxWallRunTime;
    }

    private void stopWallRun()
    {
        pc.isWallRunning = false;
    }

    private void WallRunMovement()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        // checks angle of normal vector to make sure you're going forward within 90 degree angle
        if (moveDir.z > (forwardDir.z - wallRunAcceleration) && moveDir.z < (forwardDir.z + wallRunAcceleration))
            moveDir += forwardDir;
        else if (moveDir.z < (forwardDir.z - wallRunAcceleration) && moveDir.z > (forwardDir.z + wallRunAcceleration))
        {
            // if not cancel wall run and stop movement vector
            moveDir = Vector3.zero;
            stopWallRun();
        }

        // allows for seamless movement off the wall left or right during wallrunning
        moveDir.x += horizontalInput * wallJumpForce;
        // clamp movement vector to current speed (wall run) 
        moveDir = Vector3.ClampMagnitude(moveDir, playerStatManager.instance.currSpeed);
    }

    private void wallJump()
    {
        // 
        Vector3 jumpDirection = wallNormal + Vector3.up;
        jumpDirection.Normalize(); // Normalize to keep a consistent jump force

        // Apply jump force
        playerVelocity = jumpDirection * wallJumpForce;
        moveDir.x = wallNormal.x * wallJumpForce;

        stopWallRun();
    }
}
