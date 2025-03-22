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
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask groundLayer;
    // is this variable going to be used here? 
    [SerializeField] traps trap;
    [SerializeField] LayerMask groundMask;

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;
    public bool isJetpacking;
    public bool hasHeadSpace;

    [Header("Camera Options")]
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    [Header("Audio Options")]
    [SerializeField][Range(0, 1)] float stepVolume;
    [SerializeField] float walkSoundInterval;
    [SerializeField] float runSoundInterval;
    bool isPlayingSteps;
    [SerializeField] AudioSource audioSource;
    [SerializeField] private ModulatedSoundBank footStepSounds;
    [SerializeField] private ModulatedSoundBank jumpSounds;
    [SerializeField] private ModulatedSoundBank hurtSounds;
    [SerializeField] private ModulatedSoundBank landingSounds;
    [SerializeField] private ModulatedSoundBank deathSounds;

    [Header("Player Stat Options")]
    public int HPOrig; // will move after enemy AI is not in use
    float shieldGenTimer;

    public int weaponListPos;
    public bool isAirborne;

    // leaving available until justin wants to move it
    [Header("Grapple Gun")]
    [SerializeField] Transform grappleShootPos;
    [SerializeField] LineRenderer grappleRope;

    [Header("Button Options")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    float grappleCooldownTimer;

    Rigidbody rb;

    private float desiredSpeed;

    private Vector3 moveDir;

    private float horizontalInput;
    private float verticalInput;

    private playerAttack playAtk;

    bool shieldBreak;

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
            handleShieldRegen();
            SetIsAirborne(!isGrounded);

            if (isCrouching)
                checkSky();

            updatePlayerUI();
            playAtk.weaponHandler();

            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * playerStatManager.instance.attackDistance, Color.red);
        }
    }

    private void FixedUpdate()
    {
        if (!gameManager.instance.isPaused)
            movePlayer();
    }

    void playerInput()
    {
        openChest(); // for opening loot chests
        tryToInteract(); // for interacting with buttons and switches

        setPlayerSpeed();

        if (isWallRunning) return;
        if (isSliding) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveDir = (horizontalInput * orientation.right) + (verticalInput * orientation.forward);

        //if(!isCrouching && !hasHeadSpace)
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
        if (isGrappling)
        {
            playerStatManager.instance.currSpeed = playerStatManager.instance.grappleSpeedMax;
            return;
        }

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
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, playerStatManager.instance.playerHeight * 0.5f + 0.1f/*,~ignoreLayer*/);
        isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, playerStatManager.instance.playerHeight / 2, 0), 0.5f, groundMask);

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
    void checkSky()
    {
        // call in update.
        Debug.DrawRay(transform.position, Vector3.up, Color.red, playerStatManager.instance.playerHeight * 0.5f + 0.1f);
        hasHeadSpace = Physics.SphereCast(transform.position, 2f, Vector3.up, out RaycastHit hit, playerStatManager.instance.playerHeight + 0.1f/*,~ignoreLayer*/);
        //Debug.Log("ray" + hit);
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
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpSounds.PlayRandomSound();
        }
        if (!playerStatManager.instance.hasJetpack)
        {
            if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax /*&& isGrounded*/)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(transform.up * playerStatManager.instance.jumpForce, ForceMode.Impulse);
                playerStatManager.instance.jumpCount++;
            }

            //if(Input.GetButtonUp("Jump") && !isWallRunning)
            //    playerStatManager.instance.jumpCount++;
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
        footStepSounds.PlayRandomSound();
        if (!isSprinting)
            yield return new WaitForSeconds(walkSoundInterval);
        else
            yield return new WaitForSeconds(runSoundInterval);
        isPlayingSteps = false;
    }

    public void spawnPlayer()
    {
        controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;
        updatePlayerUI();
    }
    
    public void takeDamage(int damage)
    {
        if (shieldBreak)
            playerStatManager.instance.HP -= damage;

        playerStatManager.instance.shield -= damage;
        shieldGenTimer = playerStatManager.instance.shieldRegenDelay;

        if (playerStatManager.instance.shield <= 0)
            shieldBreak = true;

        if(damage > 0)
        {
            hurtSounds.PlayRandomSound();
            StartCoroutine(flashDamageScreen());
        }
        updatePlayerUI();
        
        if (playerStatManager.instance.HP <= 0)
        {
            deathSounds.PlayRandomSound();
            gameManager.instance.youLose();
        }
    }
    
    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)playerStatManager.instance.HP / HPOrig;
        gameManager.instance.JPFuelGauge.fillAmount = (float)playerStatManager.instance.jetpackFuel / playerStatManager.instance.jetpackFuelMax;
        gameManager.instance.shieldBar.fillAmount = (float)playerStatManager.instance.shield / playerStatManager.instance.shieldMax;

        if (playerStatManager.instance.shield > playerStatManager.instance.shieldMax)
        {
            gameManager.instance.showOverShield();
            gameManager.instance.overShieldBar.fillAmount = (float)playerStatManager.instance.shield / playerStatManager.instance.shieldOverChargeMax;
        }
        else
        {
            gameManager.instance.hideOverShield();
            gameManager.instance.overShieldBar.fillAmount = 0;
        }


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
        if (Input.GetKeyDown(interactKey)) 
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
    }
    
    void tryToInteract()
    {  
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, interactionDistance, ~ignoreLayer))
        {
            buttons button = hit.collider.GetComponent<buttons>();

            if (button != null)
            {
                if (hit.collider.CompareTag("Button"))
                {
                    if (Input.GetKeyDown(interactKey))
                        button.pressButton();
                 
                    if (Input.GetKeyUp(interactKey))
                        button.ReleaseButton();
                }

                if (hit.collider.CompareTag("Switch"))
                {
                    if (Input.GetKeyDown(interactKey))
                        button.pressButton();
                }
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
        playerStatManager.instance.shield = Mathf.Min(playerStatManager.instance.shield + amount, playerStatManager.instance.shieldOverChargeMax);
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

    void handleShieldRegen()
    {
        if (playerStatManager.instance.shield < playerStatManager.instance.shieldMax)
        {
            // Decrease the regen timer over time
            shieldGenTimer -= Time.deltaTime;
            // Debug.Log("Shield Regen Time: " +  shieldGenTimer);

            // Regenerate only after the delay has passed
            if (shieldGenTimer <= 0)
            {
                playerStatManager.instance.shield += playerStatManager.instance.shieldRegen * Time.deltaTime;
                playerStatManager.instance.shield = Mathf.Clamp(playerStatManager.instance.shield, 0, playerStatManager.instance.shieldMax); // Clamp fuel between 0 and max

                if (playerStatManager.instance.shield > 0)
                    shieldBreak = false;
            }
        }
        // Reset the regen timer if shield is full
        else
            shieldGenTimer = 0f;
    }

    #endregion Everything Else

    private void SetIsAirborne(bool airborne)
    {
        if (isAirborne)
        {
            if (!airborne)
            {
                landingSounds.PlaySpecificSound(0);
            }
        }
        isAirborne = airborne;
    }

    public void MoveController(Transform destination)
    {
        controller.transform.position = destination.position;
    }
}