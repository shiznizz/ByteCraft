using UnityEngine;

public class wallRunning : MonoBehaviour
{ 
    [Header("Wallrunning")]
    public LayerMask wallLayer;
    public LayerMask groundLayer;
    public float wallRunForce;
    public float maxWallRunTime;
    public float wallRunSpeed;
    public float wallRunTimer;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

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
    public float exitWallTime;
    private float exitWallTimer;

    [Header("References")]
    public Transform orientation;
    private playerController pc;

    private float currentWallRunSpeed = 0f;
    public float wallRunAcceleration = 10f;
    public float maxWallRunSpeed = 10f;
    public float wallRunTime = 1.5f;
    public float wallJumpForce = 12f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
          pc = GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckWall();
        StateMachine();
    }

    private void CheckWall()
    {
        RaycastHit hit;
        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight || wallLeft)
            wallNormal = hit.normal;
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && !pc.isGrounded && verticalInput > 0)
        {
            if (!pc.isWallRunning)
                StartWallRun();

            if(wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
                WallRunMovement();
            }

            if (wallRunTimer <= 0 && pc.isWallRunning)
            {
                isExitingWall = true;
                exitWallTimer = exitWallTime;
            }

                if (Input.GetKeyDown(KeyCode.Space))
                wallJump();
        }
        else if (isExitingWall)
        {
            if (pc.isWallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                isExitingWall = false;
        }
        else
        {
            if (pc.isWallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pc.isWallRunning = true;
        wallRunTimer = maxWallRunTime;
    }

    private void StopWallRun()
    {
        pc.isWallRunning = false;
        currentWallRunSpeed = 0f;
    }

    private void WallRunMovement()
    {
        //rb.useGravity = false;
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        //Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        //Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        //if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        //    wallForward = -wallForward;
        //rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        //    rb.AddForce(-wallNormal * 100, ForceMode.Force);


        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;

        // Ensure correct movement direction
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // Smooth acceleration to max speed
        currentWallRunSpeed = Mathf.Lerp(currentWallRunSpeed, maxWallRunSpeed, wallRunAcceleration * Time.deltaTime);

        // Apply movement
        transform.position += wallForward * currentWallRunSpeed * Time.deltaTime;
    }



    private void wallJump()
    {
        isExitingWall = true;
        exitWallTimer = exitWallTime;

        //Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        //Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        Vector3 jumpDirection = wallNormal + Vector3.up;
        //pc.moveDir(jumpDirection * wallJumpForce);
        StopWallRun();
    }
}
