using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    #region Variables
    [SerializeField] CharacterController controller;
    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Camera Options")]
    [SerializeField] Transform cameraTransform;
    Vector3 normalCamPos;
    Vector3 crouchCamPos;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    [Header("Audio Options")]
    [SerializeField] AudioClip[] stepSounds;
    [Range(0, 1)][SerializeField] float stepVolume;
    [SerializeField] float walkSoundInterval;
    [SerializeField] float runSoundInterval;

    [Header("Player Stat Options")]
    public int HP;
    [SerializeField] int jumpMax;
    int jumpCount;
    int HPOrig;
    bool isPlayingSteps;

    //[SerializeField] List<weaponStats> weaponList = new List<weaponStats>();

    [Header("Common Weapon Options")]
    [SerializeField] float attackCooldown;
    [SerializeField] int attackDamage;
    [SerializeField] int attackDistance;
    [SerializeField] int attackRange;

    [Header("Range Options")]
    [SerializeField] GameObject gunModel;
    [SerializeField] weaponStats startGun;
    [SerializeField] Transform muzzleFlash;

    [Header("Melee Options")]
    [SerializeField] GameObject meleeWeaponModel;
    [SerializeField] weaponStats startMelee;
    [SerializeField] Animator playerAnimator;
    [SerializeField] Collider meleeCol;

    [Header("Magic Options")]
    [SerializeField] GameObject magicWeaponModel;
    [SerializeField] weaponStats startMagic;
    [SerializeField] GameObject magicProjectile; // Projectile Prefab
    [SerializeField] float magicProjectileSpeed; // Speed of projectile
    [SerializeField] Transform magicPosition;

    //Weapons inventory (gun, melee)
    int weaponListPos;

    bool isGunPOSSet;

    float shootTimer;
    float attackTimer;

    //Tracks which weapon is active
    public enum WeaponType { Gun, Melee, Magic }
    public WeaponType currentWeapon = WeaponType.Gun;

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
    [SerializeField] bool hasJetpack;
    [SerializeField] int jetpackFuelMax;
    [SerializeField] float jetpackFuel;
    [SerializeField] float jetpackFuelUse;
    [SerializeField] float jetpackFuelRegen;
    [SerializeField] float jetpackFuelRegenDelay;
    [SerializeField] int jetpackSpeed;
    [SerializeField] float jetpackHoldTimer = 0.01f;

    private float jetpackFuelRegenTimer;

    [Header("Player Movement")]
    //[SerializeField] int speedModifer;
    [SerializeField] float momentumDrag;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    private float speed;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float crouchSpeed;
    [SerializeField] float slideSpeed;
    private float desiredSpeed;
    private float prevDesiredSpeed;
    private float slideSpeedIncrease;
    private float slideSpeedDecrease;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;
    private Vector3 forwardDir;

    float playerHeight;
    float standingHeight = 2f;
    float crouchHeight = 0.5f;

    Vector3 crouchingCenter = new Vector3(0, -0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;
    public bool isJetpacking;


    [SerializeField] float maxSlideTime;
    float slideTimer;


    [SerializeField] bool isPlayerInStartingLevel;

    // variable for player input action map
    #endregion Variables

    private void Awake()
    {
        // sets state of the grapple
        grappleState = movementState.grappleNormal;
    }

    void Start()
    {
        normalCamPos = cameraTransform.localPosition;
        crouchCamPos = new Vector3(0, crouchHeight, 0);

        HPOrig = HP;
        jetpackFuel = jetpackFuelMax;

        if(!isPlayerInStartingLevel)
            playerHeight = standingHeight;

        spawnPlayer();

        jetpackFuelRegenTimer = 0f;
    }

    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * attackDistance, Color.red);
        // switches states of grapple
        checkGround();
        updatePlayerUI();

        switch (grappleState)
        {
            // not grappling 
            case movementState.grappleNormal:
                if (!gameManager.instance.isPaused)
                    movement();

                if (Input.GetButtonDown("Open")) // for opening loot chests
                    openChest();
                break;
            // is grappling
            case movementState.grappleMoving:
                grappleMovement();
                break;
        }
        handleJetpackFuelRegen();
        //cameraChange();
    }

    #region Movement
    void movement()
    {
        if (!isGrounded)
        {
            checkWall();
            wallRun();
        }
        else
        {
            if (moveDir.magnitude > 0 && !isPlayingSteps)
            {
                StartCoroutine(PlaySteps());
            }
        }

        sprint();
        crouch();
        playerMoveHandler();
        jump();

        // apply momentum
        playerVelocity += playerMomentum;

        applyGravity();

        if (playerMomentum.magnitude >= 0f)
        {
            playerMomentum -= playerMomentum * momentumDrag * Time.deltaTime;
            if (playerMomentum.magnitude <= .0f)
            {
                playerMomentum = Vector3.zero;
            }
        }

        // checks if clicking mouse 2 (right click)
        if (testGrappleKeyPressed())
            shootGrapple();

        moveDir = Vector3.ClampMagnitude(moveDir, speed);

        // ==========================
        // player attack settings below
        // ==========================

        if (!isGunPOSSet && inventoryManager.instance.weaponList.Count > 0)
            getWeaponStats(); 

        attackTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && attackTimer >= attackCooldown)   //Want button input to be the first condition for performance - other evaluations wont occur unless button is pressed
        {
            if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun && inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur > 0)
            {
                shoot();
            }
            else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Melee)
            {
                meleeAttack();
            }
            else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Magic)
            {
                shootMagicProjectile();
            }
        }
        selectWeapon();
        gunReload();
    }

    void applyGravity()
    {
        controller.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    void checkGround()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f/*,~ignoreLayer*/);

        if (isGrounded)
        {
            jumpCount = 0;
            playerVelocity = Vector3.zero;
            playerMomentum = Vector3.zero;
        }
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
        if (isWallRunning) return;

        speed = isSprinting ? sprintSpeed : isSliding ? slideSpeed : isCrouching ? crouchSpeed : walkSpeed;

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
                controller.Move(moveDir * speed * Time.deltaTime);
            }
        }
    }

    void sprint()
    {
        // toggle sprint
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = !isSprinting;
            if (isSprinting)
            {
                if (isCrouching)
                    exitCrouch();
            }
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVelocity.y = jumpSpeed;

            if(isCrouching || isSliding)
                exitCrouch();
        }
        else if (Input.GetButtonDown("Jump") && !isJetpacking && !isGrounded && hasJetpack)
        {
            // if existing jetpackCoroutine stop routine
            if(jetpackCoroutine != null)
                 StopCoroutine(jetpackCoroutine);
            // start jetpack wait timer and enable jetpack
            jetpackCoroutine = StartCoroutine(jetpackWait());
        }
        
        if (isJetpacking)
            jetpack();

        // stop jetpack and jetpack coroutine
        if(Input.GetButtonUp("Jump"))
        {
            // stop coroutine and disable jetpack
            if (jetpackCoroutine != null)
            {
                StopCoroutine(jetpackCoroutine);
                jetpackCoroutine = null;
            }

            isJetpacking = false;
        }
    }

    #region Jetpack
    Coroutine jetpackCoroutine;

    void jetpack()
    {
        if (jetpackFuel > 0)
        {
            jetpackFuel -= jetpackFuelUse * Time.deltaTime;

            playerVelocity.y = jetpackSpeed;

            jetpackFuelRegenTimer = jetpackFuelRegenDelay;
        }
    }

    void handleJetpackFuelRegen()
    {
        if (jetpackFuel < jetpackFuelMax)
        {
            // Decrease the regen timer over time
            jetpackFuelRegenTimer -= Time.deltaTime;

            // Regenerate fuel only after the delay has passed
            if (jetpackFuelRegenTimer <= 0)
            {
                jetpackFuel += jetpackFuelRegen * Time.deltaTime;
                jetpackFuel = Mathf.Clamp(jetpackFuel, 0, jetpackFuelMax); // Clamp fuel between 0 and max
            }
        }
        else
        {
            // Reset the regen timer if fuel is full
            jetpackFuelRegenTimer = 0f;
        }
    }

    IEnumerator jetpackWait()
    {
        // make player wait hold timer before jetpacking
        yield return new WaitForSeconds(jetpackHoldTimer);

        isJetpacking = true;
        jetpackCoroutine = null;
    }

    #endregion Jetpack

    #region Crouch and Slide
    void crouch()
    {
        if (Input.GetButtonDown("Crouch") && !isPlayerInStartingLevel)
        {
            if(isGrounded)
                isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height = crouchHeight;
                controller.center = crouchingCenter;
                playerHeight = crouchHeight;
                //cameraTransform.localPosition = crouchCamPos;

                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, crouchCamPos, cameraChangeTime);               

                if (speed > walkSpeed)
                {
                    isSliding = true;
                    isSprinting = false;
                    slideTimer = maxSlideTime;
                    forwardDir = transform.forward;
                }
            }
            else
            {
                exitCrouch();
            }
        }
    }

    void exitCrouch()
    {

        controller.height = standingHeight;
        controller.center = standingCenter;
        playerHeight = standingHeight;
        isCrouching = false;
        isSliding = false;
        //cameraTransform.localPosition = normalCamPos;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, normalCamPos, cameraChangeTime);
        Debug.Log("exit crouch");
    }

    void slideMovement()
    {    
        // costs jp fuel to slide while not moving
        if (moveDir == Vector3.zero)
            jetpackFuel += -(jetpackFuelMax * .25f);
        
        // slide countdown and force player to move one direction
        slideTimer -= Time.deltaTime;
        controller.Move(forwardDir * slideSpeed * Time.deltaTime);
        if (slideTimer <= 0)
        {
            exitCrouch();
        }
        
    }
    #endregion Crouch and Slide

    IEnumerator smoothSpeedLerp()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredSpeed - speed);
        float startValue = speed;

        while (time < difference)
        {
            speed = Mathf.Lerp(startValue, desiredSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        speed = desiredSpeed;
    }

    #region WallRunning

    [Header("Wallrunning")]
    public LayerMask wallLayer;
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

    private float currentWallRunSpeed = 0f;
    public float wallRunAcceleration = 10f;
    public float maxWallRunSpeed = 10f;
    public float wallRunTime = 1.5f;
    public float wallJumpForce = 12f;

    private void checkWall()
    {
        RaycastHit hit;
        wallRight = Physics.Raycast(transform.position, orientation.right, out hit, wallCheckDistance, wallLayer);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out hit, wallCheckDistance, wallLayer);
        if (wallRight || wallLeft)
            wallNormal = hit.normal;
    }

    private void wallRun()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && !isGrounded && verticalInput > 0)
        {
            if (!isWallRunning)
                StartWallRun();

            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
                WallRunMovement();
            }

            if (wallRunTimer <= 0 && isWallRunning)
            {
                isExitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(KeyCode.Space))
                wallJump();
        }
        else if (isExitingWall)
        {
            if (isWallRunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                isExitingWall = false;
        }
        else
        {
            if (isWallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = maxWallRunTime;
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        currentWallRunSpeed = 0f;
    }

    private void WallRunMovement()
    {
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
        //apply jump vector to move controller
        //pc.moveDir(jumpDirection * wallJumpForce);
        StopWallRun();
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
            playerVelocity.y -= gravity * Time.deltaTime;
            StopGrapple();
        }

        // if use the jump key it will stop grappling 
        else if (testJumpKeyPressed())
        {
            playerMomentum = grappleSpeed * grappleDir;
            playerMomentum += Vector3.up * grappleLift;
            grappleState = movementState.grappleNormal;
            playerVelocity.y -= gravity * Time.deltaTime;
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

    #region Weapons
    void shoot()
    {
        attackTimer = 0;
        inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur--;
        updatePlayerUI();

        StartCoroutine(flashMuzzle());
        audioSource.PlayOneShot(inventoryManager.instance.weaponList[weaponListPos].gun.shootSounds[Random.Range(0, inventoryManager.instance.weaponList[weaponListPos].gun.shootSounds.Length)], inventoryManager.instance.weaponList[weaponListPos].gun.shootVolume);


        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, attackDistance, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            Instantiate(inventoryManager.instance.weaponList[weaponListPos].gun.hitEffect, hit.point, Quaternion.identity);

        }

        IDamage damage = hit.collider.GetComponent<IDamage>();

        damage?.takeDamage(attackDamage);
    }


    public void getWeaponStats()
    {
        
        weaponListPos = inventoryManager.instance.weaponList.Count - 1; // Selects the newly added weapon
        changeWeapon();

        isGunPOSSet = true;
        
    }
    
    void changeWeapon()
    {
        switch (inventoryManager.instance.weaponList[weaponListPos].type)
        {
            case weaponStats.weaponType.Gun:
                changeGun();
                break;
            case weaponStats.weaponType.Melee:
                changeMeleeWep();
                break;
            case weaponStats.weaponType.Magic:
                changeMagicWep();
                break;
        }
    }
    
    void changeGun()
    {
        attackDamage = inventoryManager.instance.weaponList[weaponListPos].gun.shootDamage;
        attackDistance = inventoryManager.instance.weaponList[weaponListPos].gun.shootRange;
        attackCooldown = inventoryManager.instance.weaponList[weaponListPos].gun.shootRate;
        muzzleFlash.SetLocalPositionAndRotation(new Vector3(inventoryManager.instance.weaponList[weaponListPos].gun.moveFlashX, inventoryManager.instance.weaponList[weaponListPos].gun.moveFlashY, inventoryManager.instance.weaponList[weaponListPos].gun.moveFlashZ), muzzleFlash.rotation);
    
        gunModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[weaponListPos].gun.model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[weaponListPos].gun.model.GetComponent<MeshRenderer>().sharedMaterial;
    
        turnOffWeaponModels();
    }
    
    void changeMeleeWep()
    {
        attackDamage = inventoryManager.instance.weaponList[weaponListPos].meleeWep.meleeDamage;
        attackRange = inventoryManager.instance.weaponList[weaponListPos].meleeWep.meleeDistance;
    
        attackCooldown = inventoryManager.instance.weaponList[weaponListPos].meleeWep.meleeCooldown;
    
        meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[weaponListPos].meleeWep.model.GetComponent<MeshFilter>().sharedMesh;
        meleeWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[weaponListPos].meleeWep.model.GetComponent<MeshRenderer>().sharedMaterial;
    
        turnOffWeaponModels();
    }
    
    void changeMagicWep()
    {
        attackDamage = inventoryManager.instance.weaponList[weaponListPos].magicWep.magicDamage;
        attackRange = inventoryManager.instance.weaponList[weaponListPos].magicWep.magicDitance;
    
        attackCooldown = inventoryManager.instance.weaponList[weaponListPos].magicWep.magicCooldown;
    
        magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[weaponListPos].magicWep.model.GetComponent<MeshFilter>().sharedMesh;
        magicWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[weaponListPos].magicWep.model.GetComponent<MeshRenderer>().sharedMaterial;
    
        turnOffWeaponModels();
    }
    
    void turnOffWeaponModels()
    {
        if (meleeWeaponModel != null && inventoryManager.instance.weaponList[weaponListPos].type != weaponStats.weaponType.Melee)
            meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;
    
        if (magicWeaponModel != null && inventoryManager.instance.weaponList[weaponListPos].type != weaponStats.weaponType.Magic)
            magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;
    
        if (gunModel != null && inventoryManager.instance.weaponList[weaponListPos].type != weaponStats.weaponType.Gun)
            gunModel.GetComponent<MeshFilter>().sharedMesh = null;
    }
    
    void gunReload()
    {
        if (Input.GetButtonDown("Reload") && inventoryManager.instance.weaponList.Count > 0 && inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
        {
            if (inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve > inventoryManager.instance.weaponList[weaponListPos].gun.ammoMax)          //Check if the player can reload a full clip
            {
                inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve -= (inventoryManager.instance.weaponList[weaponListPos].gun.ammoMax - inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur);
                inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur = inventoryManager.instance.weaponList[weaponListPos].gun.ammoMax;
                audioSource.PlayOneShot(inventoryManager.instance.weaponList[weaponListPos].gun.reloadSounds[Random.Range(0, inventoryManager.instance.weaponList[weaponListPos].gun.reloadSounds.Length)], inventoryManager.instance.weaponList[weaponListPos].gun.reloadVolume);
            }
            else if (inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur = inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve;
                inventoryManager.instance.weaponList[weaponListPos].gun.ammoReserve = 0;
                audioSource.PlayOneShot(inventoryManager.instance.weaponList[weaponListPos].gun.reloadSounds[Random.Range(0, inventoryManager.instance.weaponList[weaponListPos].gun.reloadSounds.Length)], inventoryManager.instance.weaponList[weaponListPos].gun.reloadVolume);
            }
    
            updatePlayerUI();
        }
    }
    
    
    void meleeAttack()
    {
        attackTimer = 0; // Will Reset the cooldown timer
    
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("MeleeAttack");
        }

        //StartCoroutine(toggleWepCol());


        //Activate melee weapon
        if (meleeWeaponModel != null)
        {
            meleeWeaponModel.SetActive(true); // Shows the melee weapon during the attack
        }

        //Raycast to detect enemies in melee range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, attackRange, ~ignoreLayer))
        {
            Debug.Log("Melee hit; " + hit.collider.name);

            //Apply damage if the object hit implements IDamage
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            damageable.takeDamage(attackDamage);
        }

    }

    //IEnumerator toggleWepCol()
    //{
    //    meleeCol.enabled = true;
    //    yield return new WaitForSeconds(0.1f);
    //    meleeCol.enabled = false;

    //}

    void shootMagicProjectile()
    {
        attackTimer = 0;
    
        Instantiate(magicProjectile, magicPosition.position, transform.rotation);
        audioSource.PlayOneShot(inventoryManager.instance.weaponList[weaponListPos].magicWep.magicSounds[Random.Range(0, inventoryManager.instance.weaponList[weaponListPos].magicWep.magicSounds.Length)], inventoryManager.instance.weaponList[weaponListPos].magicWep.magicVolume);
    }
    
    IEnumerator flashMuzzle()
    {
        muzzleFlash.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        muzzleFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.gameObject.SetActive(false);
    }
    
    //Switches between weapon types using a mouse scroll wheel
    void selectWeapon()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0 && weaponListPos < inventoryManager.instance.weaponList.Count - 1)
        {
            weaponListPos++;
            changeWeapon();
        }
        else if (scrollInput < 0 && weaponListPos > 0)
        {
            weaponListPos--;
            changeWeapon();
        }
    }
    #endregion Weapons
    
    #region Everything Else
    public void spawnPlayer()
    {
        //controller.enabled = false;
        controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;
        //controller.enabled = true;
    
        HP = HPOrig;
        updatePlayerUI();
    }
    
    public void takeDamage(int damage)
    {
        HP -= damage;
        StartCoroutine(flashDamageScreen());
        updatePlayerUI();
    
        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }
    
    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        gameManager.instance.JPFuelGauge.fillAmount = (float)jetpackFuel / jetpackFuelMax;

        //Toggle jetpack recharge UI
        if (hasJetpack)
            gameManager.instance.showJetpack();
        else if (!hasJetpack)
            gameManager.instance.hideJetpack();

        //Grapple recharge UI
        if (grappleCooldownTimer <= grappleCooldown)
        {
            gameManager.instance.grappleGauge.enabled = true;
            gameManager.instance.grappleGauge.fillAmount = (float)grappleCooldownTimer / grappleCooldown;
        }
        else if (gameManager.instance.grappleGauge.enabled)
            gameManager.instance.grappleGauge.enabled = false;
    
        // Toggle ammo counter based on weapon type
        if (inventoryManager.instance.weaponList.Count > 0)
        {
            if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
            {
                gameManager.instance.updateAmmo(inventoryManager.instance.weaponList[weaponListPos].gun);
                gameManager.instance.showAmmo();
            }
            else
                gameManager.instance.hideAmmo();
        }
    }
    
    private pickup.LootType lastLootType;
    
    public void PickupLoot(pickup.LootType type, int amount)
    {
        lastLootType = type;
    
        switch (type)
        {
            case pickup.LootType.Health:
                HP = Mathf.Min(HP + amount, HPOrig); // prevent exceeding max HP
                break;
        }
        updatePlayerUI(); // refresh UI after pickup
    }

    void openChest()
    {
        Debug.Log("Starting the openChest function...");
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 5f, ~ignoreLayer))
        {
            lootDrop dropsLoot = hit.collider.GetComponent<lootDrop>();

            if (dropsLoot != null)
            {
                Debug.Log("dropsLoot was not null!");
                dropsLoot.dropLoot();
            }
        }

    }

    public void heal(int amount)
    {
        HP = Mathf.Min(HP + amount, HPOrig); // prevent exceeding max HP
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
    #endregion Everything Else
}