//using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    #region Variables
    // please dont move this variable :)
    [SerializeField] Transform orientation;

    [SerializeField] CharacterController controller;
    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] traps trap;

    [Header("Camera Options")]
    //[SerializeField] Transform cameraTransform;
    //Vector3 normalCamPos;
    //Vector3 crouchCamPos;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    [Header("Audio Options")]
    [SerializeField] AudioClip[] stepSounds;
    [Range(0, 1)][SerializeField] float stepVolume;
    [SerializeField] float walkSoundInterval;
    [SerializeField] float runSoundInterval;
    bool isPlayingSteps;

    [Header("Player Stat Options")]
    //[SerializeField] int jumpMax;
    //int jumpCount;
    public int HPOrig; // will move after enemy AI is not in use

    //[Header("Common Weapon Options")]
    //[SerializeField] float attackCooldown;
    //[SerializeField] int attackDamage;
    //[SerializeField] int attackDistance;
    //[SerializeField] int attackRange;

    //[Header("Range Options")]
    //[SerializeField] GameObject gunModel;
    //[SerializeField] weaponStats startGun;
    //[SerializeField] Transform muzzleFlash;

    //[Header("Melee Options")]
    //[SerializeField] GameObject meleeWeaponModel;
    //[SerializeField] weaponStats startMelee;
    //[SerializeField] Animator playerAnimator;
    //[SerializeField] Collider meleeCol;

    //[Header("Magic Options")]
    //[SerializeField] GameObject magicWeaponModel;
    //[SerializeField] weaponStats startMagic;
    //[SerializeField] GameObject magicProjectile; // Projectile Prefab
    //[SerializeField] float magicProjectileSpeed; // Speed of projectile
    //[SerializeField] Transform magicPosition;

    public int weaponListPos;

    ////bool isGunPOSSet;

    //float shootTimer;
    //float attackTimer;

    ////Tracks which weapon is active
    //public enum WeaponType { Gun, Melee, Magic }
    //public WeaponType currentWeapon = WeaponType.Gun;

    [Header("Grapple Options")]
    [SerializeField] int grappleDistance;
    [SerializeField] int grappleLift;
    [SerializeField] float grappleSpeedMultiplier;
    [SerializeField] float grappleSpeedMin;
    [SerializeField] float grappleSpeedMax;
    [SerializeField] float grappleCooldown;

    [Header("Grapple Gun")]
    [SerializeField] Transform grappleShootPos;
    [SerializeField] LineRenderer grappleRope;

    // holds state of the grapple 
    private movementState grappleState;
    public enum movementState
    {
        walking,
        sprinting,
        wallRunning,
        crouching,
        sliding,
        air,
        grappleNormal, // did not shoot grapple
        grappleMoving, // grapple succesful now moving player
    }
    float grappleCooldownTimer;

    [Header("JetPack Options")]
    //[SerializeField] bool hasJetpack;
    //[SerializeField] int jetpackFuelMax;
    //[SerializeField] float jetpackFuel;
    //[SerializeField] float jetpackFuelUse;
    //[SerializeField] float jetpackFuelRegen;
    //[SerializeField] float jetpackFuelRegenDelay;
    //[SerializeField] int jetpackSpeed;
    //[SerializeField] float jetpackHoldTimer = 0.01f;

        

    [Header("Player Movement")]
    //[SerializeField] int speedModifer;
    //[SerializeField] float momentumDrag;
    //[SerializeField] int jumpSpeed;
    //[SerializeField] int gravity;

    Rigidbody rb;

    private float desiredSpeed;
    private float prevDesiredSpeed;
    private float slideSpeedIncrease;
    private float slideSpeedDecrease;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;
    public bool isJetpacking;

    private float horizontalInput;
    private float verticalInput;

    private playerAttack playAtk;

    // variable for player input action map
    #endregion Variables

    private void Awake()
    {
        // sets state of the grapple
        grappleState = movementState.grappleNormal;
    }

    void Start()
    {
        playAtk = GetComponent<playerAttack>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        HPOrig = playerStatManager.instance.HPMax;
        playerStatManager.instance.jetpackFuel = playerStatManager.instance.jetpackFuelMax;
        
        playerStatManager.instance.playerHeight = playerStatManager.instance.standingHeight;

        spawnPlayer();

        //jetpackFuelRegenTimer = 0f;
    }

    private void Update()
    {
        playerInput();
        SpeedControl();
        checkGround();
        
        updatePlayerUI();
        playAtk.weaponHandler();

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * playerStatManager.instance.attackDistance, Color.red);
        // switches states of grapple
        //switch (grappleState)
        //{
        //    // not grappling 
        //    case movementState.grappleNormal:
        //        if (!gameManager.instance.isPaused)
        //            //movement();

        //        if (Input.GetButtonDown("Open")) // for opening loot chests
        //            openChest();
        //        break;
        //    // is grappling
        //    case movementState.grappleMoving:
        //        grappleMovement();
        //        break;
        //}
        //handleJetpackFuelRegen();
        //cameraChange();
    }

    private void FixedUpdate()
    {
        if (!gameManager.instance.isPaused)
            movePlayer();
        { 
        }
    }

    void playerInput()
    {
        setPlayerSpeed();

        if (isWallRunning) return;
        if (isSliding) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        jump();
        sprint();
    }

    #region Movement
    void movePlayer()
    {
        moveDir = (horizontalInput * orientation.right) + (verticalInput * orientation.forward);

        if(!isGrounded && !isWallRunning)
            applyGravity();

        if (moveDir == Vector3.zero)
        {
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        rb.AddForce(moveDir.normalized * playerStatManager.instance.currSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if(flatVelocity.magnitude > playerStatManager.instance.currSpeed)
        {
            Vector3 limitVelocity = flatVelocity.normalized * playerStatManager.instance.currSpeed;
            rb.linearVelocity = new Vector3(limitVelocity.x, rb.linearVelocity.y, limitVelocity.z);
        }
    }

    private void setPlayerSpeed()
    {
        playerStatManager.instance.currSpeed = isWallRunning ? playerStatManager.instance.wallRunSpeed : isSprinting ? playerStatManager.instance.sprintSpeed : isSliding ? playerStatManager.instance.slideSpeed
                    : isCrouching ? playerStatManager.instance.crouchSpeed : playerStatManager.instance.walkSpeed;
    }

    void move()
    {
        moveDir = (Input.GetAxis("Horizontal") * orientation.right) +
                      (Input.GetAxis("Vertical") * orientation.forward);

        if (isSliding) return;
        if(isWallRunning) return;

        if (moveDir == Vector3.zero)
        { 
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        float speedToApply = Mathf.Max(playerStatManager.instance.speedLimit, horizontalSpeed);

        if (!isGrounded)
            speedToApply = playerStatManager.instance.currSpeed;

        if (speedToApply >= playerStatManager.instance.speedLimit)
            speedToApply *= isGrounded ? 0.985f : 0.99f;

        Vector3 newVelocity = moveDir.normalized * speedToApply;
        newVelocity.y = rb.linearVelocity.y;

        if(isGrounded) 
            rb.linearVelocity = newVelocity;
        else
            rb.AddForce(moveDir.normalized * speedToApply, ForceMode.Force);

        if (!isGrounded && horizontalSpeed > playerStatManager.instance.speedLimit)
        {
            Vector3 newHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            newHorizontalVelocity *= 0.98f;
            rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
        }
    }

    void movement()
    {
        // check wall will now be placed in update in wall running script
        // should conciously add audio to new movement functions
        if (!isGrounded)
        {
            checkWall();
            //wallRun();
        }
        else
        {
            if (moveDir.magnitude > 0 && !isPlayingSteps)
            {
                StartCoroutine(PlaySteps());
            }
        }

        // create new movement functions for sprinting, and jump
        sprint();
        // crouch should be called in update in crouch script
        // crouch();
        // update playerMoveHandler() then call in update.
        playerMoveHandler();
        // move jump to update and create conditions for if hasJetpack
        jump();

        // wont touch grapple code.
        // apply momentum
        playerVelocity += playerMomentum;

        // apply gravity if not wall running or grounded
        if (!isWallRunning)
            applyGravity();

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

        // ==========================
        // player attack settings below
        // ==========================

        //if (!isGunPOSSet && inventoryManager.instance.weaponList.Count > 0)
        //    getWeaponStats(); 

        playerStatManager.instance.attackTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;

        //if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && playerStatManager.instance.attackTimer >= playerStatManager.instance.attackCooldown)   //Want button input to be the first condition for performance - other evaluations wont occur unless button is pressed
        //{
        //    //if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun && inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur > 0)
        //    //{
        //        shoot();
        //    //}
        //    //else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Melee)
        //    //{
        //    //    meleeAttack();
        //    //}
        //    //else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Magic)
        //    //{
        //    //    shootMagicProjectile();
        //    //}
        //}
        //selectWeapon();
        //gunReload();
    }

    void applyGravity()
    {
        // call if not grounded or wall running.
        // needs to be placed in fixed update
        rb.AddForce(Vector3.down * playerStatManager.instance.gravity);

        // old gravity code below
        //controller.Move(playerVelocity * Time.deltaTime);
        //playerVelocity.y -= playerStatManager.instance.gravity * Time.deltaTime;
    }

    void checkGround()
    {
        // call in update.
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerStatManager.instance.playerHeight * 0.5f + 0.1f/*,~ignoreLayer*/);

        if (isGrounded)
        {
            playerStatManager.instance.jumpCount = 0;
            rb.linearDamping = playerStatManager.instance.groundDrag;

            // antiquated code get rid of when no longer being used.
            //playerVelocity = Vector3.zero;
            //playerMomentum = Vector3.zero;
        }
        else
            rb.linearDamping = playerStatManager.instance.airDrag;
    }

    IEnumerator PlaySteps()
    {
        isPlayingSteps = true;
        audioSource.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)],stepVolume);
        if (!isSprinting)
            yield return new WaitForSeconds(walkSoundInterval);
        else
            yield return new WaitForSeconds(runSoundInterval);
        isPlayingSteps = false;
    }

    private void playerMoveHandler()
    {
        // gut this then take what you need. this is the old playerInput
        // wall running and sliding should just boot the player out.
        // then apply proper speed.
        // call in update.

        // in different method move the player based on input obtained here.
        if (isWallRunning)
        {
            // apply wallRunSpeed then start wall run
            playerStatManager.instance.currSpeed = playerStatManager.instance.wallRunSpeed;
            WallRunMovement();
        }
        // essentially an if else statment that checks movement state and applies the proper speed
        else
            playerStatManager.instance.currSpeed = isSprinting ? playerStatManager.instance.sprintSpeed : isSliding ? playerStatManager.instance.slideSpeed 
                    : isCrouching ? playerStatManager.instance.crouchSpeed : playerStatManager.instance.walkSpeed;

        if (isGrounded && isSliding)
        {
            slideMovement();
        }
        else
        {
            moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                      (Input.GetAxis("Vertical") * transform.forward);
            if(moveDir != Vector3.zero)
            {
                controller.Move(moveDir * playerStatManager.instance.currSpeed * Time.deltaTime);
            }
        }
    }

    bool secondSprintInput;
    void sprint()
    {
        // toggle sprint on if moving forward and sprint button is pressed
        if (Input.GetButtonDown("Sprint") && Input.GetKey(KeyCode.W))
        {
            if(!isSprinting)
            {
                isSprinting = true;

                if (isCrouching)
                    exitCrouch();
            }
            // turn sprint off if pressed again
            else
                isSprinting = false;
        }

        // toggle sprint off if not moving forward
        if (Input.GetKeyUp(KeyCode.W))
            isSprinting = false;
    }

    void jump()
    {
        if (!playerStatManager.instance.hasJetpack)
        {
            if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax)
            {
                playerStatManager.instance.jumpCount++;

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(transform.up * playerStatManager.instance.jumpForce, ForceMode.Impulse);
            }
        }
    }

    //void jump()
    //{
    //    if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax)
    //    {
    //        playerStatManager.instance.jumpCount++;
    //        playerVelocity.y = playerStatManager.instance.jumpForce;

    //        if(isCrouching || isSliding)
    //            exitCrouch();
    //        if (isWallRunning)
    //            wallJump();
    //    }
    //    else if (Input.GetButtonDown("Jump") && !isJetpacking && !isGrounded && playerStatManager.instance.hasJetpack)
    //    {
    //        // if existing jetpackCoroutine stop routine
    //        if(jetpackCoroutine != null)
    //             StopCoroutine(jetpackCoroutine);
    //        // start jetpack wait timer and enable jetpack
    //        jetpackCoroutine = StartCoroutine(jetpackWait());
    //    }
        
    //    if (isJetpacking)
    //        jetpack();

    //    // stop jetpack and jetpack coroutine
    //    if(Input.GetButtonUp("Jump"))
    //    {
    //        // stop coroutine and disable jetpack
    //        if (jetpackCoroutine != null)
    //        {
    //            StopCoroutine(jetpackCoroutine);
    //            jetpackCoroutine = null;
    //        }

    //        isJetpacking = false;
    //    }
    //}

    #region Jetpack
    //Coroutine jetpackCoroutine;

    //void jetpack()
    //{
    //    if (playerStatManager.instance.jetpackFuel > 0)
    //    {
    //        playerStatManager.instance.jetpackFuel -= playerStatManager.instance.jetpackFuelUse * Time.deltaTime;

    //        playerVelocity.y = playerStatManager.instance.jetpackSpeed;

    //        jetpackFuelRegenTimer = playerStatManager.instance.jetpackFuelRegenDelay;
    //    }
    //}

    //void handleJetpackFuelRegen()
    //{
    //    if (playerStatManager.instance.jetpackFuel < playerStatManager.instance.jetpackFuelMax)
    //    {
    //        // Decrease the regen timer over time
    //        jetpackFuelRegenTimer -= Time.deltaTime;

    //        // Regenerate fuel only after the delay has passed
    //        if (jetpackFuelRegenTimer <= 0)
    //        {
    //            playerStatManager.instance.jetpackFuel += playerStatManager.instance.jetpackFuelRegen * Time.deltaTime;
    //            playerStatManager.instance.jetpackFuel = Mathf.Clamp(playerStatManager.instance.jetpackFuel, 0, playerStatManager.instance.jetpackFuelMax); // Clamp fuel between 0 and max
    //        }
    //    }
    //    else
    //    {
    //        // Reset the regen timer if fuel is full
    //        jetpackFuelRegenTimer = 0f;
    //    }
    //}

    //IEnumerator jetpackWait()
    //{
    //    // make player wait hold timer before jetpacking
    //    yield return new WaitForSeconds(playerStatManager.instance.jetpackHoldTimer);

    //    isJetpacking = true;
    //    jetpackCoroutine = null;
    //}

    public void refillFuel(int amount)
    {
        playerStatManager.instance.jetpackFuel = Mathf.Min(playerStatManager.instance.jetpackFuel + amount, playerStatManager.instance.jetpackFuelMax);
        updatePlayerUI();
    }

    #endregion Jetpack

    #region Crouch and Slide
    void crouch()
    {
        //if (Input.GetButtonDown("Crouch") && !isPlayerInStartingLevel)
        //{
        //    // toggles crouch
        //    if(isGrounded)
        //        isCrouching = !isCrouching;

        //    // adjusts controller height and orients controller on ground
        //    if (isCrouching)
        //    {
        //        controller.height = crouchHeight;
        //        controller.center = crouchingCenter;
        //        playerHeight = crouchHeight;

        //        // changes camera position (lerp was breaking this)
        //        cameraTransform.localPosition = crouchCamPos;

        //        //cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, crouchCamPos, cameraChangeTime);               
        //        // if moving faster than walking - slide
        //        if (speed > playerStatManager.instance.playerWalkSpeed)
        //        {
        //            isSliding = true;
        //            isSprinting = false;
        //            // starts slide timer and sets vector to lock player movement
        //            slideTimer = maxSlideTime;
        //            forwardDir = transform.forward;
        //        }
        //    }
        //    else
        //    {
        //        exitCrouch();
        //    }
        //}
    }

    void exitCrouch()
    {
        //// readjusts controller and camera height
        //controller.height = standingHeight;
        //controller.center = standingCenter;
        //playerHeight = standingHeight;
        //isCrouching = false;
        //isSliding = false;
        //cameraTransform.localPosition = normalCamPos;
        ////cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, normalCamPos, cameraChangeTime);
        //Debug.Log("exit crouch");
    }

    void slideMovement()
    {    
        //// costs jp fuel to slide while not moving
        //if (moveDir == Vector3.zero)
        //    jetpackFuel += -(jetpackFuelMax * .25f);
        
        //// slide countdown and force player to move one direction
        //slideTimer -= Time.deltaTime;
        //controller.Move(forwardDir * playerStatManager.instance.playerSlideSpeed * Time.deltaTime);
        //if (slideTimer <= 0)
        //{
        //    exitCrouch();
        //}
        
    }
    #endregion Crouch and Slide

    // previously used to simulate momentum by lerping speed transitions
    IEnumerator smoothSpeedLerp()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredSpeed - playerStatManager.instance.currSpeed);
        float startValue = playerStatManager.instance.currSpeed;

        while (time < difference)
        {
            playerStatManager.instance.currSpeed = Mathf.Lerp(startValue, desiredSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        playerStatManager.instance.currSpeed = desiredSpeed;
    }

    #region WallRunning

    //[Header("Wallrunning")]
    //public LayerMask wallLayer;
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
    //public float wallCheckDistance = 1f;
    //private RaycastHit rightWallHit;
    //private RaycastHit leftWallHit;
    //private bool wallRight;
    //private bool wallLeft;
    //private Vector3 wallNormal;

    //[Header("Exiting")]
    //private bool isExitingWall;
    //public float exitWallTime;
    //private float exitWallTimer;

    //[Header("References")]
    //public Transform orientation;

    //public float wallRunAcceleration = 10f;
    ////public float maxWallRunSpeed = 10f;
    //public float wallRunTime = 1.5f;
    //public float wallJumpForce = 12f;

    private void checkWall()
    {
        //RaycastHit hit;

        //// checks if player is next to a left or right wall then enters or exits wall running state accordingly
        //wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        //wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);

        //if ((wallRight || wallLeft) && !isWallRunning)
        //    wallRun();
        //if ((!wallRight && !wallLeft) && isWallRunning)
        //    stopWallRun();
    }

    private void wallRun()
    {
        //// reset jumps, start wallrun, cancel gravity
        //playerStatManager.instance.playerJumpCount = 0;
        //StartWallRun();
        //playerVelocity = Vector3.zero;

        //// checks wall normal and sets wall normal to left or right wall normal then updates the forwareDir
        //wallNormal = wallLeft ? leftWallHit.normal : rightWallHit.normal;
        //forwardDir = Vector3.Cross(wallNormal, Vector3.up);

        //// if on left wall go backwards
        //if(Vector3.Dot(forwardDir, leftWallHit.normal) < 0)
        //    forwardDir = -forwardDir;
    }

    private void StartWallRun()
    {
        //isWallRunning = true;
        //wallRunTimer = maxWallRunTime;
    }

    private void stopWallRun()
    {
        //isWallRunning = false;
    }

    private void WallRunMovement()
    {
        //horizontalInput = Input.GetAxis("Horizontal");
        //// checks angle of normal vector to make sure you're going forward within 90 degree angle
        //if (moveDir.z > (forwardDir.z - wallRunAcceleration) && moveDir.z < (forwardDir.z + wallRunAcceleration))
        //    moveDir += forwardDir;
        //else if (moveDir.z < (forwardDir.z - wallRunAcceleration) && moveDir.z > (forwardDir.z + wallRunAcceleration))
        //{
        //    // if not cancel wall run and stop movement vector
        //    moveDir = Vector3.zero;
        //    stopWallRun();
        //}

        //// allows for seamless movement off the wall left or right during wallrunning
        //moveDir.x += horizontalInput * wallJumpForce;
        //// clamp movement vector to current speed (wall run) 
        //moveDir = Vector3.ClampMagnitude(moveDir, speed);
    }

    private void wallJump()
    {
        //// 
        //Vector3 jumpDirection = wallNormal + Vector3.up;
        //jumpDirection.Normalize(); // Normalize to keep a consistent jump force

        //// Apply jump force
        //playerVelocity = jumpDirection * wallJumpForce;
        //moveDir.x = wallNormal.x * wallJumpForce;

        //stopWallRun();
    }

    #endregion WallRunning

    #region GrappleHook
    // handles where the grapple is hitting
    void shootGrapple()
    {
        // resets cooldown
        grappleCooldownTimer = 0;
        // chcks if the grapple hits a collider or not
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, grappleDistance, ~ignoreLayer))
        {

            Debug.Log(hit.collider.name);

            isGrappling = true;
            grapplePostion = hit.point;

            grappleRope.enabled = true;
            grappleRope.SetPosition(1, grapplePostion);

            grappleState = movementState.grappleMoving;
        }
    }
    private void LateUpdate()
    {
        if (isGrappling)
            grappleRope.SetPosition(0, grappleShootPos.position);
    }
    // handles the grapple moving the character
    void grappleMovement()
    {
        // sets min and max speed for grapple movement
        float grappleSpeed = Mathf.Clamp(Vector3.Distance(transform.position, grapplePostion), grappleSpeedMin, grappleSpeedMax);
        // direction the player will move
        Vector3 grappleDir = (grapplePostion - transform.position).normalized;
        // moving the player
        controller.Move(grappleSpeed * grappleSpeedMultiplier * Time.deltaTime * grappleDir);

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
            playerMomentum += Vector3.up * grappleLift;
            grappleState = movementState.grappleNormal;
            playerVelocity.y -= playerStatManager.instance.gravity * Time.deltaTime;
            StopGrapple();
        }

    }

    // tests if the grapple key is pressed and returns a bool
    bool testGrappleKeyPressed()
    {
        if (Input.GetButton("Fire2") && grappleCooldownTimer >= grappleCooldown)
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
        isGrappling = false;
        grappleRope.enabled = false;
    }
    #endregion GrappleHook
    #endregion Movement
    
    #region Everything Else
    public void spawnPlayer()
    {
        controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;

        playerStatManager.instance.HP = HPOrig;
        updatePlayerUI();
    }
    
    public void takeDamage(int damage)
    {
        playerStatManager.instance.HP -= damage;
        StartCoroutine(flashDamageScreen());
        updatePlayerUI();
    
        if (playerStatManager.instance.HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }
    
    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)playerStatManager.instance.HP / HPOrig;
        gameManager.instance.JPFuelGauge.fillAmount = (float)playerStatManager.instance.jetpackFuel / playerStatManager.instance.jetpackFuelMax;

        //Toggle jetpack recharge UI
        if (playerStatManager.instance.hasJetpack)
            gameManager.instance.showJetpack();
        else if (!playerStatManager.instance.hasJetpack)
            gameManager.instance.hideJetpack();

        //Grapple recharge UI
        if (grappleCooldownTimer <= grappleCooldown)
        {
            gameManager.instance.grappleGauge.enabled = true;
            gameManager.instance.grappleGauge.fillAmount = (float)grappleCooldownTimer / grappleCooldown;
        }
        else if (gameManager.instance.grappleGauge.enabled)
            gameManager.instance.grappleGauge.enabled = false;
        
        //Handle Ammo UI
        if (inventoryManager.instance.weaponList.Count > 0)
        {
            gameManager.instance.showAmmo();
            gameManager.instance.updateAmmo();
        }    
    }
    
    private pickup.LootType lastLootType;
    
    public void PickupLoot(pickup.LootType type, int amount)
    {
        lastLootType = type;
    
        switch (type)
        {
            case pickup.LootType.Health:
                playerStatManager.instance.HP = Mathf.Min(playerStatManager.instance.HP + amount, HPOrig); // prevent exceeding max HP
                break;
        }
        updatePlayerUI(); // refresh UI after pickup
    }

    void openChest()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 5f, ~ignoreLayer))
        {
            lootDrop dropsLoot = hit.collider.GetComponent<lootDrop>();

            if (dropsLoot != null)
            {
                dropsLoot.dropLoot();
            }
        }

    }

    public void heal(int amount)
    {
        playerStatManager.instance.HP = Mathf.Min(playerStatManager.instance.HP + amount, HPOrig); // prevent exceeding max HP
        updatePlayerUI(); // refresh UI after pickup
    }
    
    IEnumerator flashDamageScreen()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);
    }

    public void addInventory(itemSO item)
    {
        inventoryManager.instance.addItem(item);
    }

    public void getArmor(int amount)
    {
        playerStatManager.instance.Armor = Mathf.Min(playerStatManager.instance.Armor + amount, playerStatManager.instance.ArmorMax);
        //gameManager.instance.updateArmorUI(armor);  will be implemented at a alatter time
    }

    public void getAmmo(int amount)
    {
        //if (inventoryManager.instance.weaponList.Count > 0 &&
        //    inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
        //{
        //    inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve =
        //        Mathf.Min(inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve + amount,
        //        inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserveMax);

        //    updatePlayerUI();
        //}
    }

    #endregion Everything Else
}