using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] LayerMask groundLayer;

    [Header("Player Options")]
    public int HP;
    [SerializeField] int jumpMax;
    int jumpCount;
    int HPOrig;

    [SerializeField] List<weaponStats> weaponList = new List<weaponStats>();

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

    [Header("Magic Options")]
    [SerializeField] GameObject magicWeaponModel;
    [SerializeField] weaponStats startMagic;
    [SerializeField] GameObject magicProjectile; // Projectile Prefab
    [SerializeField] float magicProjectileSpeed; // Speed of projectile
    [SerializeField] Transform magicPosition;

    //Weapons inventory (gun, melee)
    int weaponListPos;

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
    float grappleCooldownTimer;

    [Header("Player Movement")]
    [SerializeField] int speedModifer;
    [SerializeField] float momentumDrag;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    public float speed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float slideSpeed;
    public float slideSpeedIncrease;
    public float slideSpeedDecrease;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;
    private Vector3 forwardDir;

    float playerHeight;
    float standingHeight = 2f;
    float crouchHeight = 1f;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    public bool isGrounded;
    public bool isSprinting;
    public bool isGrappling;
    public bool isSliding;
    public bool isCrouching;
    public bool isWallRunning;

    float slideTimer;
    float maxSlideTime;

    public Transform groundCheck;

    [Header("JetPack Options")]
    [SerializeField] bool hasJetpack;
    [SerializeField] int jetpackFuelMax;
    [SerializeField] float jetpackFuel;
    [SerializeField] float jetpackFuelUse;
    [SerializeField] float jetpackFuelRegen;
    [SerializeField] float jetpackFuelRegenDelay;
    [SerializeField] int jetpackSpeed;

    private float jetpackFuelRegenTimer;

    // state of the grapple 
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

    private void Awake()
    {
        // sets state of the grapple
        grappleState = movementState.grappleNormal;
    }

    void Start()
    {
        HPOrig = HP;
        jetpackFuel = jetpackFuelMax;
        playerHeight = standingHeight;
        spawnPlayer();

        jetpackFuelRegenTimer = 0f;

        if (startGun != null)
        {
            weaponList.Add(startGun);
        }
        if (startMelee != null)
        {
            weaponList.Add(startMelee);
        }
        if (startMagic != null)
        {
            weaponList.Add(startMagic);
        }

        if (weaponList.Count > 0)
        {
            changeWeapon();
        }
    }

    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * attackDistance, Color.red);
        // switches states of grapple
        switch (grappleState)
        {
            // not grappling 
            case movementState.grappleNormal:
                if (!gameManager.instance.isPaused)
                movement();
                sprint();
                crouch();
                handleJetpackFuelRegen();
                if (Input.GetButtonDown("Open")) // for opening loot chests
                    openChest();
                break;
            // is grappling
            case movementState.grappleMoving:
                grappleMovement();
                sprint();
                crouch();
                handleJetpackFuelRegen();
                break;
        }
    }

    private void LateUpdate()
    {
        if (isGrappling)
            grappleRope.SetPosition(0, grappleShootPos.position);
    }

    void increaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void decreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease;
    }
    void movement()
    {
        checkGround();

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);

        speed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed;

        controller.Move(moveDir * speed * Time.deltaTime);

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

        attackTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && weaponList.Count > 0 && attackTimer >= attackCooldown)   //Want button input to be the first condition for performance - other evaluations wont occur unless button is pressed
        {
            if (weaponList[weaponListPos].type == weaponStats.weaponType.Gun && weaponList[weaponListPos].gun.ammoCur > 0)
            {
                shoot();
            }
            else if (weaponList[weaponListPos].type == weaponStats.weaponType.Melee)
            {
                meleeAttack();
            }
            else if (weaponList[weaponListPos].type == weaponStats.weaponType.Magic)
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
        if (isGrounded)
        {
            jumpCount = 0;
            playerVelocity = Vector3.zero;
            playerMomentum = Vector3.zero;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            //speed *= speedModifer;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            //speed /= speedModifer;
            isSprinting = false;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVelocity.y = jumpSpeed;
        }
        else if ((Input.GetButton("Jump") && !isGrounded) && hasJetpack)
        {
            jetpack();
        }
    }

    void crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = !isCrouching;

            if(isCrouching)
            {
                controller.height = crouchHeight;
                controller.center = crouchingCenter;
                playerHeight = crouchHeight;

                if (speed > walkSpeed)
                {
                    isSliding = true;
                    slideTimer = maxSlideTime;
                    forwardDir = transform.forward;
                    if(isGrounded)
                        increaseSpeed(slideSpeedIncrease);
                }
            }
            else
            {
                controller.height = standingHeight;
                controller.center = standingCenter;
                playerHeight = standingHeight;
                isSliding = false;
            }
        }
    }

    void slideMovement()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            controller.Move(forwardDir * slideSpeed * Time.deltaTime);
            if (slideTimer <= 0)
            {
                isSliding = false;
                decreaseSpeed(slideSpeedDecrease);
            }
        }
    }

        void jetpack()
    {
        if (jetpackFuel > 0)
        {
            jetpackFuel -= jetpackFuelUse * Time.deltaTime;

            playerVelocity.y = jetpackSpeed;

            jetpackFuelRegenTimer = jetpackFuelRegenDelay;

            updatePlayerUI();
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
                updatePlayerUI();
            }
        }
        else
        {
            // Reset the regen timer if fuel is full
            jetpackFuelRegenTimer = 0f;
            updatePlayerUI();
        }
    }

    void shoot()
    {
        attackTimer = 0;
        weaponList[weaponListPos].gun.ammoCur--;
        updatePlayerUI();

        StartCoroutine(flashMuzzle());

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, attackDistance, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            Instantiate(weaponList[weaponListPos].gun.hitEffect, hit.point, Quaternion.identity);

            IDamage damage = hit.collider.GetComponent<IDamage>();

            damage?.takeDamage(attackDamage);
        }
    }

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

    IEnumerator flashDamageScreen()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);
    }

    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        gameManager.instance.JPFuelGauge.fillAmount = (float)jetpackFuel / jetpackFuelMax;

        //Grapple recharge UI
        if (grappleCooldownTimer <= grappleCooldown)
        {
            gameManager.instance.grappleGauge.enabled = true;
            gameManager.instance.grappleGauge.fillAmount = (float)grappleCooldownTimer / grappleCooldown;
        }
        else if (gameManager.instance.grappleGauge.enabled)
            gameManager.instance.grappleGauge.enabled = false;

        // Toggle ammo counter based on weapon type
        if (weaponList.Count > 0)
        {
            if (weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
            {
                gameManager.instance.updateAmmo(weaponList[weaponListPos].gun);
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

    public void heal(int amount)
    {
        HP = Mathf.Min(HP + amount, HPOrig); // prevent exceeding max HP
        updatePlayerUI(); // refresh UI after pickup
    }

    public void getWeaponStats(weaponStats weapon)
    {
        weaponList.Add(weapon);
        weaponListPos = weaponList.Count - 1; // Selects the newly added weapon

        changeWeapon();
    }

    void changeWeapon()
    {
        switch (weaponList[weaponListPos].type)
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
        attackDamage = weaponList[weaponListPos].gun.shootDamage;
        attackDistance = weaponList[weaponListPos].gun.shootRange;
        attackCooldown = weaponList[weaponListPos].gun.shootRate;
        muzzleFlash.SetLocalPositionAndRotation(new Vector3(weaponList[weaponListPos].gun.moveFlashX,weaponList[weaponListPos].gun.moveFlashY,weaponList[weaponListPos].gun.moveFlashZ),muzzleFlash.rotation);

        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].gun.model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].gun.model.GetComponent<MeshRenderer>().sharedMaterial;

        turnOffWeaponModels();
    }

    void changeMeleeWep()
    {
        attackDamage = weaponList[weaponListPos].meleeWep.meleeDamage;
        attackRange = weaponList[weaponListPos].meleeWep.meleeDistance;

        attackCooldown = weaponList[weaponListPos].meleeWep.meleeCooldown;

        meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].meleeWep.model.GetComponent<MeshFilter>().sharedMesh;
        meleeWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].meleeWep.model.GetComponent<MeshRenderer>().sharedMaterial;

        turnOffWeaponModels();
    }

    void changeMagicWep()
    {
        attackDamage = weaponList[weaponListPos].magicWep.magicDamage;
        attackRange = weaponList[weaponListPos].magicWep.magicDitance;

        attackCooldown = weaponList[weaponListPos].magicWep.magicCooldown;

        magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].magicWep.model.GetComponent<MeshFilter>().sharedMesh;
        magicWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].magicWep.model.GetComponent<MeshRenderer>().sharedMaterial;

        turnOffWeaponModels();
    }

    void turnOffWeaponModels()
    {
        if (meleeWeaponModel != null && weaponList[weaponListPos].type != weaponStats.weaponType.Melee)
            meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;

        if (magicWeaponModel != null && weaponList[weaponListPos].type != weaponStats.weaponType.Magic)
            magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;

        if (gunModel != null && weaponList[weaponListPos].type != weaponStats.weaponType.Gun)
            gunModel.GetComponent<MeshFilter>().sharedMesh = null;
    }

    void gunReload()
    {
        if (Input.GetButtonDown("Reload") && weaponList.Count > 0 && weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
        {
            if (weaponList[weaponListPos].gun.ammoReserve > weaponList[weaponListPos].gun.ammoMax)          //Check if the player can reload a full clip
            {
                weaponList[weaponListPos].gun.ammoReserve -= (weaponList[weaponListPos].gun.ammoMax - weaponList[weaponListPos].gun.ammoCur);
                weaponList[weaponListPos].gun.ammoCur = weaponList[weaponListPos].gun.ammoMax;
            }
            else if (weaponList[weaponListPos].gun.ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                weaponList[weaponListPos].gun.ammoCur = weaponList[weaponListPos].gun.ammoReserve;
                weaponList[weaponListPos].gun.ammoReserve = 0;
            }

            updatePlayerUI();
        }
    }

    public void spawnPlayer()
    {
        controller.enabled = false;
        controller.transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true;

        HP = HPOrig;
        updatePlayerUI();
    }

    void meleeAttack()
    {
        attackTimer = 0; // Will Reset the cooldown timer

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("MeleeAttack");
        }

        //Activate melee weapon
        if (meleeWeaponModel != null)
        {
            meleeWeaponModel.SetActive(true); // Shows the melee weapon during the attack
        }

        //Raycast to detect enemies in melee range
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, attackRange, ~ignoreLayer)) 
        {
            Debug.Log("Melee hit; " + hit.collider.name);

            //Apply damage if the object hit implements IDamage
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            damageable.takeDamage(attackDamage);
        }

    }

    void shootMagicProjectile()
    {
        attackTimer = 0;

        Instantiate(magicProjectile, magicPosition.position, transform.rotation);
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
        if(scrollInput > 0 && weaponListPos < weaponList.Count - 1)
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
}
