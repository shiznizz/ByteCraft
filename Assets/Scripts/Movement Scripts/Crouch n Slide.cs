using UnityEngine;

public class CrouchnSlide : MonoBehaviour
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();
        normalCamPos = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        crouch();

        if (pc.isSprinting && pc.isCrouching)
            exitCrouch();
    }

    void crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {// toggles crouch 
            if (pc.isGrounded)
                pc.isCrouching = !pc.isCrouching;
        }

        if (Input.GetButtonDown("Jump") && pc.isCrouching)
            pc.isCrouching = false;
        
        // adjusts controller height and orients controller on ground
        if (pc.isCrouching)
        {
            controller.height = playerStatManager.instance.crouchHeight;
            controller.center = crouchingCenter;
            playerStatManager.instance.playerHeight = playerStatManager.instance.crouchHeight;

            // changes camera position (lerp was breaking this)
            cameraTransform.localPosition = crouchCamPos;

            //cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, crouchCamPos, cameraChangeTime);               
            // if moving faster than walking - slide
            if (playerStatManager.instance.currSpeed > playerStatManager.instance.walkSpeed)
            {
                pc.isSliding = true;
                pc.isSprinting = false;
                // starts slide timer and sets vector to lock player movement
                slideTimer = playerStatManager.instance.maxSlideTime;
                forwardDir = transform.forward;
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
        Debug.Log("exit crouch");
    }

    void slideMovement()
    {    
        // costs jp fuel to slide while not moving
        //if (moveDir == Vector3.zero)
        //    jetpackFuel += -(jetpackFuelMax * .25f);
        
        // slide countdown and force player to move one direction
        slideTimer -= Time.deltaTime;
        controller.Move(forwardDir * playerStatManager.instance.slideSpeed * Time.deltaTime);
        if (slideTimer <= 0)
        {
            exitCrouch();
        }
        
    }
}
