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
    public bool wallRan;

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

    // leaving available until justin wants to move it
    [Header("Grapple Gun")]
    [SerializeField] Transform grappleShootPos;
    [SerializeField] LineRenderer grappleRope;

    float grappleCooldownTimer;

    Rigidbody rb;

    private float desiredSpeed;
    private float prevDesiredSpeed;
    private float slideSpeedIncrease;
    private float slideSpeedDecrease;

    private Vector3 moveDir;

    private float horizontalInput;
    private float verticalInput;

    private playerAttack playAtk;

    // variable for player input action map
    #endregion Variables

    //private void Awake()
    //{
    //    // sets state of the grapple
    //    grappleState = movementState.grappleNormal;
    //}

    void Start()
    {
        playAtk = GetComponent<playerAttack>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        HPOrig = playerStatManager.instance.HPMax;

        spawnPlayer();
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

            if (Input.GetButtonDown("Open")) // for opening loot chests
                openChest();
            #region stale
            ////switches states of grapple
            //switch (grappleState)
            //{
            //    // not grappling 
            //    case movementState.grappleNormal:
            //        if (!gameManager.instance.isPaused)
            //            //movement();
            //        break;
            //    // is grappling
            //    case movementState.grappleMoving:
            //        grappleMovement();
            //        break;
            //}
            #endregion stale
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
        if (isWallRunning) return;

        if (!isGrounded)
            applyGravity();

        // no more penquin mode. Stops player when they stop pressing keys unless airborn
        if (moveDir == Vector3.zero)
        {
            if (isGrounded) rb.linearVelocity = rb.linearVelocity * 0.6f;
            return;
        }

        if (isGrounded)
            rb.AddForce(moveDir.normalized * playerStatManager.instance.currSpeed * 10f, ForceMode.Force);
        else if (isJetpacking)
            rb.AddForce(moveDir.normalized * playerStatManager.instance.currSpeed * playerStatManager.instance.jetpackAirMod * 10f, ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(moveDir.normalized * playerStatManager.instance.currSpeed * playerStatManager.instance.airSpeedMod * 10f, ForceMode.Force);
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

    public void applyGravity()
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
            if (moveDir.magnitude > 0 && !isPlayingSteps && !isSliding)
            {
                StartCoroutine(PlaySteps());
            }
        }
        else
            rb.linearDamping = playerStatManager.instance.airDrag;
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
        if (grappleCooldownTimer <= playerStatManager.instance.grappleCooldown)
        {
            gameManager.instance.grappleGauge.enabled = true;
            gameManager.instance.grappleGauge.fillAmount = (float)grappleCooldownTimer / playerStatManager.instance.grappleCooldown;
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