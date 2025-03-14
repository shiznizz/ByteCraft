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
    [SerializeField] Transform orientation;
    [SerializeField] CharacterController controller;
    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask groundLayer;
    // is this variable going to be used here? 
    [SerializeField] traps trap;

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;
    public bool isJetpacking;

    [Header("Camera Options")]
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
    public int HPOrig; // will move after enemy AI is not in use

    public int weaponListPos;

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
        grappleNormal, // did not shoot grapple
        grappleMoving, // grapple succesful now moving player
    }
    float grappleCooldownTimer;

    Rigidbody rb;

    private float desiredSpeed;
    private float prevDesiredSpeed;
    private float slideSpeedIncrease;
    private float slideSpeedDecrease;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

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
        if (!gameManager.instance.isPaused)
        {
            playerInput();
            SpeedControl();
            checkGround();

            updatePlayerUI();
            playAtk.weaponHandler();

            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * playerStatManager.instance.attackDistance, Color.red);
            //switches states of grapple
            switch (grappleState)
            {
                // not grappling 
                case movementState.grappleNormal:
                    if (!gameManager.instance.isPaused)
                        //movement();

                        if (Input.GetButtonDown("Open")) // for opening loot chests
                            openChest();
                    break;
                // is grappling
                case movementState.grappleMoving:
                    grappleMovement();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!gameManager.instance.isPaused)
            movePlayer();
    }

    void playerInput()
    {
        setPlayerSpeed();

        if (isWallRunning) return;
        if (isSliding) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveDir = (horizontalInput * orientation.right) + (verticalInput * orientation.forward);

        jump();
        sprint();
    }

    #region Movement
    void movePlayer()
    {
        if (!isGrounded && !isWallRunning)
            applyGravity();

        // no more penquin mode. Stops player when they stop pressing keys unless airborn
        if (moveDir == Vector3.zero)
        {
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        rb.AddForce(moveDir.normalized * playerStatManager.instance.currSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if(horizontalVel.magnitude > playerStatManager.instance.currSpeed)
        {
            Vector3 limitVelocity = horizontalVel.normalized * playerStatManager.instance.currSpeed;
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
            //checkWall();
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

        playerStatManager.instance.attackTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;
    }

    void applyGravity()
    {
        // adds a continous downwards force to the rigidbody
        rb.AddForce(Vector3.down * playerStatManager.instance.gravity);
    }

    void checkGround()
    {
        // call in update.
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerStatManager.instance.playerHeight * 0.5f + 0.1f/*,~ignoreLayer*/);

        // applies drag and resets jump count
        if (isGrounded)
        {
            playerStatManager.instance.jumpCount = 0;
            rb.linearDamping = playerStatManager.instance.groundDrag;
            
            // if you are grounded and moving play step sounds.
            if (moveDir.magnitude > 0 && !isPlayingSteps)
            {
                StartCoroutine(PlaySteps());
            }
        }
        else
            rb.linearDamping = playerStatManager.instance.airDrag;
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
            //WallRunMovement();
        }
        // essentially an if else statment that checks movement state and applies the proper speed
        else
            playerStatManager.instance.currSpeed = isSprinting ? playerStatManager.instance.sprintSpeed : isSliding ? playerStatManager.instance.slideSpeed 
                    : isCrouching ? playerStatManager.instance.crouchSpeed : playerStatManager.instance.walkSpeed;

        if (isGrounded && isSliding)
        {
            //slideMovement();
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

    void sprint()
    {
        // toggle sprint on if moving forward and sprint button is pressed
        if (Input.GetButtonDown("Sprint") && Input.GetKey(KeyCode.W))
            isSprinting = !isSprinting;

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
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(transform.up * playerStatManager.instance.jumpForce, ForceMode.Impulse);

                playerStatManager.instance.jumpCount++;
            }
        }
    }

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

    IEnumerator PlaySteps()
    {
        isPlayingSteps = true;
        audioSource.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)], stepVolume);
        if (!isSprinting)
            yield return new WaitForSeconds(walkSoundInterval);
        else
            yield return new WaitForSeconds(runSoundInterval);
        isPlayingSteps = false;
    }

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

    public void refillFuel(int amount)
    {
        playerStatManager.instance.jetpackFuel = Mathf.Min(playerStatManager.instance.jetpackFuel + amount, playerStatManager.instance.jetpackFuelMax);
        updatePlayerUI();
    }

    #endregion Everything Else
}