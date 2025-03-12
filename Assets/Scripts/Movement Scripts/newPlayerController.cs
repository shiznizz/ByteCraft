using UnityEngine;

public class newPlayerController : MonoBehaviour
{
    Rigidbody rb;
    private Vector3 moveDir;
    [SerializeField] Transform orientation;

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;
    public bool isJetpacking;

    private float horizontalInput;
    private float verticalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerStatManager.instance.playerHeight = playerStatManager.instance.standingHeight;
    }

    // Update is called once per frame
    void Update()
    {
        playerInput();
        SpeedControl();
        checkGround();
        jump();
    }
    private void FixedUpdate()
    {
        movePlayer();
    }

    void playerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        Debug.Log("Horizontal: " + horizontalInput + " Vertical: " + verticalInput);
    }

    void movePlayer()
    {
        moveDir = (horizontalInput * orientation.right) + (verticalInput * orientation.forward);
        rb.AddForce(moveDir.normalized * playerStatManager.instance.walkSpeed * 10f, ForceMode.Force);
        Debug.Log("MoveDir: " + moveDir);
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVelocity.magnitude > playerStatManager.instance.currSpeed)
        {
            Vector3 limitVelocity = flatVelocity.normalized * playerStatManager.instance.currSpeed;
            rb.linearVelocity = new Vector3(limitVelocity.x, rb.linearVelocity.y, limitVelocity.z);
        }
    }
    void checkGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerStatManager.instance.playerHeight * 0.5f + 0.1f/*,~ignoreLayer*/);

        if (isGrounded)
        {
            playerStatManager.instance.jumpCount = 0;
            rb.linearDamping = playerStatManager.instance.groundDrag;
        }
        else
            rb.linearDamping = 0;
    }

    private void jump()
    {
        if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax)
        {
            playerStatManager.instance.jumpCount++;
           
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(transform.up * playerStatManager.instance.jumpForce, ForceMode.Impulse);
        }
    }
}
