using UnityEngine;
using UnityEngine.XR;

public class WallRunning : MonoBehaviour
{
    public Transform orientation;
    private playerController pc;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Vector3 forwardDir;

    [Header("Wallrunning")]
    public LayerMask wallLayer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    private RaycastHit rightWallHit;
    private RaycastHit leftWallHit;
    private bool wallRight;
    private bool wallLeft;
    private Vector3 wallNormal;
    private Vector3 prevWallNormal;
    private bool hasRunOnWall;
    private bool startingWR;

    [Header("Exiting")]
    private bool isExitingWall = false;
    private float exitWallTimer;

    float wallRunTimer;
    float wallRerunTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pc.isGrounded)
            checkWall();

        wallRunTimers();
    }

    private void FixedUpdate()
    {
        if (pc.isWallRunning && verticalInput > 0)
            WallRunMovement();
        else
            stopWallRun();
    }

    private void checkWall()
    {
        // checks if player is next to a left or right wall then enters or exits wall running state accordingly
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, playerStatManager.instance.wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, playerStatManager.instance.wallCheckDistance, wallLayer);

        wallRunHandler();
    }

    void wallRunHandler()
    {
        if (Input.GetButtonDown("Jump") && pc.isWallRunning)
            wallJump();

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if ((wallRight || wallLeft) && !pc.isWallRunning && verticalInput > 0)
            testWall();
        if ((!wallRight && !wallLeft) && pc.isWallRunning || verticalInput < 0)
            stopWallRun();
    }

    private void WallRunMovement()
    {
        // reset jumps, start wallrun
        playerStatManager.instance.jumpCount = 0;

        // cancel any y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // determines the forward direction of the wall
        forwardDir = Vector3.Cross(wallNormal, Vector3.up);

        // check which direction is closer to where the player is facing. (forwards or backwards)
        if ((orientation.forward - forwardDir).magnitude > (orientation.forward - -forwardDir).magnitude)
            forwardDir = -forwardDir;

        // add a one time push to help player get up to wall running speed
        if (startingWR)
        {
            startingWR = false;
            rb.AddForce(forwardDir * playerStatManager.instance.wallRunSpeed * 0.9f, ForceMode.Impulse);
        }
        // apply force to player to move them along the wall
        rb.AddForce(forwardDir * playerStatManager.instance.wallRunSpeed, ForceMode.Force);

        // first if allows for gently moving away from the wall (based on wall normal and a or d press)
        // else if forces player to the wall if they are pressing either no key or the key that is opposite of the wall normal
        if ((wallLeft && horizontalInput > 0) || (wallRight && horizontalInput < 0))
            rb.AddForce(wallNormal * playerStatManager.instance.currSpeed * 2, ForceMode.Force);
        else if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * playerStatManager.instance.wallAdhesiveForce, ForceMode.Force);
    }

    void testWall()
    {
        // checks wall normal and sets wall normal to left or right wall normal
        getWallNorm();

        // code to prevent running on the same wall twice.
        // commented out because it is taking too long to get it to work the way I want it to.

        //if (hasRunOnWall)
        //{
        //    //Debug.Log("Wall norm: " + wallNormal + " Prev norm: " +  prevWallNormal);
        //    float wallAngle = Vector3.Angle(wallNormal, prevWallNormal);

        //    Debug.Log("Wall Angle: " + wallAngle);
        //    if (wallAngle > playerStatManager.instance.minimumWallAngleDifference)
        //    {
        //        Debug.Log("you wall ran, and should run again");
        //        StartWallRun();
        //    }
        //}
        //else
        //{
              StartWallRun();
        //    hasRunOnWall = true;
        //}
    }


    private void wallJump()
    {
        getWallNorm();

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        Vector3 jumpUp = transform.up * playerStatManager.instance.wallJumpUpForce;
        Vector3 jumpSide = wallNormal * playerStatManager.instance.wallJumpSideForce;
        Vector3 jumpDirection = transform.up * playerStatManager.instance.wallJumpUpForce + wallNormal * playerStatManager.instance.wallJumpSideForce;
        jumpUp.Normalize(); // Normalize to keep a consistent jump force

        //rb.AddForce(jumpUp, ForceMode.Impulse);
        //rb.AddForce(jumpSide, ForceMode.Impulse);
        rb.AddForce(jumpDirection, ForceMode.Impulse);

        stopWallRun();
    }

    void getWallNorm()
    {
        wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        //Debug.Log("wallNorm: " + wallNormal);
    }

    private void StartWallRun()
    {
        if (pc.isJetpacking)
            pc.isJetpacking = false;

        pc.isWallRunning = true;
        startingWR = true;

        wallRunTimer = playerStatManager.instance.maxWallRunTime;
    }

    private void stopWallRun()
    {
        pc.isWallRunning = false;
        prevWallNormal = wallNormal;

        // prevent wall running for a time after wall run
        if (!isExitingWall)
        {
            isExitingWall = true;
            exitWallTimer = playerStatManager.instance.exitWallTime;
        }
    }

    private void wallRunTimers()
    {
        if (wallRunTimer > 0)
            wallRunTimer -= Time.deltaTime;
        if (wallRunTimer <= 0)
            stopWallRun();
        //Debug.Log("Wall time: " + wallRunTimer);

        if (isExitingWall)
            exitWallTimer -= Time.deltaTime;
        if (exitWallTimer <= 0)
            isExitingWall = false;

        //if (!pc.isWallRunning)
        //    wallRerunTimer = playerStatManager.instance.wallRerunTime;
        //if (!pc.isWallRunning && wallRerunTimer > 0)
        //    wallRerunTimer -= Time.deltaTime;
        //if (wallRerunTimer < 0 || pc.isGrounded)
        //    hasRunOnWall = false;

    }
}
