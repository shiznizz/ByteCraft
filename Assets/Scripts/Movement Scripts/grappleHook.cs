using UnityEngine;
using static playerController;

public class grappleHook : MonoBehaviour
{
    [SerializeField] LayerMask ignoreLayer;

    private playerController pc;
    private Rigidbody rb;

    private movementState grappleState;
    public enum movementState
    {
        grappleNormal, // did not shoot grapple
        grappleMoving, // grapple succesful now moving player
    }

    float grappleCooldownTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

    private void Awake()
    {
        // sets state of the grapple
        grappleState = movementState.grappleNormal;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused)
        {
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * playerStatManager.instance.attackDistance, Color.red);
            //switches states of grapple
            switch (grappleState)
            {
                // not grappling 
                case movementState.grappleNormal:
                    if (!gameManager.instance.isPaused)
                        oldMovement();

                    break;
                // is grappling
                case movementState.grappleMoving:
                    grappleMovement();
                    break;
            }
        }
    }

    void oldMovement()
    {
        // apply momentum
        playerVelocity += playerMomentum;


        // grapple code
        if (playerMomentum.magnitude >= 0f)
        {
            playerMomentum -= playerMomentum * playerStatManager.instance.drag * Time.deltaTime;
            if (playerMomentum.magnitude <= .0f)
            {
                playerMomentum = Vector3.zero;
            }
        }

        // checks if clicking mouse 2 (right click)
        if (testGrappleKeyPressed())
            shootGrapple();

        // replacing this code in speed control
        moveDir = Vector3.ClampMagnitude(moveDir, playerStatManager.instance.currSpeed);

        playerStatManager.instance.attackTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;
    }

    void applyGravity()
    {
        // adds a continous downwards force to the rigidbody
        rb.AddForce(Vector3.down * playerStatManager.instance.gravity);
    }

    // handles where the grapple is hitting
    void shootGrapple()
    {
        // resets cooldown
        grappleCooldownTimer = 0;
        // chcks if the grapple hits a collider or not
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, playerStatManager.instance.grappleDistance, ~ignoreLayer))
        {

            Debug.Log(hit.collider.name);

            pc.isGrappling = true;
            grapplePostion = hit.point;

            playerStatManager.instance.grappleRope.enabled = true;
            playerStatManager.instance.grappleRope.SetPosition(1, grapplePostion);

            grappleState = movementState.grappleMoving;
        }
    }
    private void LateUpdate()
    {
        if (pc.isGrappling)
            playerStatManager.instance.grappleRope.SetPosition(0, playerStatManager.instance.grappleShootPos.position);
    }
    // handles the grapple moving the character
    void grappleMovement()
    {
        // sets min and max speed for grapple movement
        float grappleSpeed = Mathf.Clamp(Vector3.Distance(transform.position, grapplePostion), playerStatManager.instance.grappleSpeedMin, playerStatManager.instance.grappleSpeedMax);
        // direction the player will move
        Vector3 grappleDir = (grapplePostion - transform.position).normalized;
        // moving the player // commented out to avoid errors
        //controller.Move(grappleSpeed * grappleSpeedMultiplier * Time.deltaTime * grappleDir);

        // checks if reached end of grapple
        float grapleDistanceMove = 1f;
        if (Vector3.Distance(transform.position, grapplePostion) < grapleDistanceMove)
        {
            grappleState = movementState.grappleNormal;
            playerVelocity.y -= playerStatManager.instance.gravity * Time.deltaTime;
            StopGrapple();
        }

        // if use the jump key it will stop grappling 
        else if (testJumpKeyPressed())
        {
            playerMomentum = grappleSpeed * grappleDir;
            playerMomentum += Vector3.up * playerStatManager.instance.grappleLift;
            grappleState = movementState.grappleNormal;
            playerVelocity.y -= playerStatManager.instance.gravity * Time.deltaTime;
            StopGrapple();
        }

    }

    // tests if the grapple key is pressed and returns a bool
    bool testGrappleKeyPressed()
    {
        if (Input.GetButton("Fire2") && grappleCooldownTimer >= playerStatManager.instance.grappleCooldown)
            return true;

        else
            return false;

    }

    // tests if the jump key is pressed and returns a bool
    bool testJumpKeyPressed()
    {
        if (Input.GetButton("Jump"))
            return true;
        else
            return false;
    }

    public void StopGrapple()
    {
        pc.isGrappling = false;
        playerStatManager.instance.grappleRope.enabled = false;
    }
}
