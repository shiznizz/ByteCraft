using UnityEngine;

public class CrouchnSlide : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] CharacterController controller;

    private playerController pc;
    private Rigidbody rb;

    Vector3 crouchingCenter = new Vector3(0, -0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    private Vector3 forwardDir;

    float slideTimer;

    [Header("Camera Options")]
    [SerializeField] Transform cameraTransform;
    Vector3 normalCamPos;
    Vector3 crouchCamPos;

    //horizontalInput = Input.GetAxisRaw("Horizontal");
    //    verticalInput = Input.GetAxisRaw("Vertical");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();
        normalCamPos = cameraTransform.localPosition;
        playerStatManager.instance.playerHeight = playerStatManager.instance.standingHeight;
    }

    // Update is called once per frame
    void Update()
    {
        crouch();

        if (pc.isSprinting && pc.isCrouching)
            exitCrouch();
        //if (pc.isSliding && pc.isGrounded)
        //    slideCountdown();
    }

    private void FixedUpdate()
    {
        // because gravity wasn't working when crouching or sliding
        if ((!pc.isWallRunning || !pc.isGrounded) && (pc.isCrouching || pc.isSliding)) 
            pc.applyGravity();
        if (pc.isSliding && pc.isGrounded)
            slideMovement();
    }

    void crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {// toggles crouch 
            if (pc.isGrounded)
                pc.isCrouching = !pc.isCrouching;
        }

        if ((Input.GetButtonDown("Jump") || Input.GetButtonDown("Sprint")) && pc.isCrouching)
            pc.isCrouching = false;
     
        
        // adjusts controller height and orients controller on ground
        if (pc.isCrouching)
        {
            controller.height = playerStatManager.instance.crouchHeight;
            controller.center = crouchingCenter;
            playerStatManager.instance.playerHeight = playerStatManager.instance.crouchHeight;

            // changes camera position (lerp was breaking this)
            cameraTransform.localPosition = crouchCamPos;
              
            // if moving faster than walking - slide
            if (playerStatManager.instance.currSpeed > playerStatManager.instance.walkSpeed && !pc.isSliding)
            {
                pc.isSliding = true;
                pc.isSprinting = false;
                playerStatManager.instance.slideSpeed = playerStatManager.instance.slideSpeedMax;
                // starts slide timer and sets vector to lock player movement
                slideTimer = playerStatManager.instance.maxSlideTime;
                forwardDir = orientation.forward;
            }
        }
        else
        {
            exitCrouch();
        }
    }

    public void exitCrouch()
    {
        // readjusts controller and camera height
        controller.height = playerStatManager.instance.standingHeight;
        controller.center = standingCenter;
        playerStatManager.instance.playerHeight = playerStatManager.instance.standingHeight;
        pc.isCrouching = false;
        pc.isSliding = false;
        cameraTransform.localPosition = normalCamPos;
    }

    void slideMovement()
    {
        slideTimer -= Time.deltaTime;
        rb.AddForce(forwardDir.normalized * playerStatManager.instance.currSpeed * 10f, ForceMode.Force);
        playerStatManager.instance.slideSpeed -= Time.deltaTime * playerStatManager.instance.slideFriction;
        if (slideTimer <= 0)
            pc.isSliding = false;
            //exitCrouch();
    }

    void slideCountdown()
    {
        //Debug.Log("slide timer = " + slideTimer);
        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0)
            exitCrouch();
    }
}
