using UnityEngine;

public class wallRunning : MonoBehaviour
{ 
    //[Header("Wallrunning")]
    //public LayerMask whatIsWall;
    //public LayerMask whatIsGround;
    //public float wallRunForce;
    //public float maxWallRunTime;
    //public float wallRunSpeed;
    //public float wallRunTimer;
    //public float wallJumpUpForce;
    //public float wallJumpSideForce;

    //[Header("Input")]
    //private float horizontalInput;
    //private float verticalInput;

    //[Header("Detection")]
    //public float wallCheckDistance;
    //private RaycastHit rightWallHit;
    //private RaycastHit leftWallHit;
    //private bool wallRight;
    //private bool wallLeft;

    //[Header("Exiting")]
    //private bool isExitingWall;
    //public float exitWallTime;
    //private float exitWallTimer;

    //[Header("References")]
    //public Transform orientation;
    //private playerController pc;
    //private Rigidbody rb;

    //// Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    rb = GetComponent<Rigidbody>();
    //    pc = GetComponent<playerController>();
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    CheckWall();
    //    StateMachine();
    //}

    //private void CheckWall()
    //{
    //    wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
    //    wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    //}

    //private void StateMachine()
    //{
    //    // Getting Inputs
    //    horizontalInput = Input.GetAxisRaw("Horizontal");
    //    verticalInput = Input.GetAxisRaw("Vertical");

    //    // State 1 - Wallrunning
    //    if ((wallLeft || wallRight) && !pc.isGrounded && verticalInput > 0)
    //    {
    //        if (!pc.isWallRunning)
    //            StartWallRun();

    //        if(wallRunTimer > 0)
    //            wallRunTimer -= Time.deltaTime;

    //        if (wallRunTimer <= 0 && pc.isWallRunning)
    //        {
    //            isExitingWall = true;
    //            exitWallTimer = exitWallTime;
    //        }

    //            if (Input.GetKeyDown(KeyCode.Space))
    //            wallJump();
    //    }
    //    else if (isExitingWall)
    //    {
    //        if (pc.isWallRunning)
    //            StopWallRun();

    //        if (exitWallTimer > 0)
    //            exitWallTimer -= Time.deltaTime;

    //        if (exitWallTimer <= 0)
    //            isExitingWall = false;
    //    }
    //    else
    //    {
    //        if (pc.isWallRunning)
    //            StopWallRun();
    //    }
    //}

    //private void StartWallRun()
    //{
    //    pc.isWallRunning = true;
    //    wallRunTimer = maxWallRunTime;
    //}

    //private void StopWallRun()
    //{
    //    pc.isWallRunning = false;
    //}

    //private void WallRunMovement()
    //{
    //    rb.useGravity = false;
    //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

    //    Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

    //    Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

    //    if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
    //        wallForward = -wallForward;
    //    rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

    //    if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
    //        rb.AddForce(-wallNormal * 100, ForceMode.Force);
    //}

    //private void StopWallRunMovement()
    //{
    //    rb.useGravity = true;
    //}

    //private void wallJump()
    //{
    //    isExitingWall = true;
    //    exitWallTimer = exitWallTime;

    //    Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
    //    Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        
    //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    //    rb.AddForce(forceToApply, ForceMode.Impulse);
    //}
}
