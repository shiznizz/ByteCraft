using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage, IPickup
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Player Options")]
    public int HP;
    [SerializeField] int speed;
    [SerializeField] int speedModifer;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] float momentumDrag;

    [Header("JetPack Options")]
    [SerializeField] bool hasJetpack;
    [SerializeField] int jetpackFuelMax;
    [SerializeField] float jetpackFuel;
    [SerializeField] float jetpackFuelUse;
    [SerializeField] float jetpackFuelRegen;
    [SerializeField] float jetpackFuelRegenDelay;
    [SerializeField] int jetpackSpeed;

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
    private State grappleState;

    int jumpCount;
    int HPOrig;

    //Weapons inventory (gun, melee)
    int weaponListPos;

    float grappleCooldownTimer;
    float shootTimer;
    float attackTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

    bool isSprinting;
    bool isGrappling;

    private float jetpackFuelRegenTimer;

    //Tracks which weapon is active
    public enum WeaponType { Gun, Melee, Magic }
    public WeaponType currentWeapon = WeaponType.Gun;

    // state of the grapple 
    private enum State
    {
        grappleNormal, // did not shoot grapple
        grappleMoving, // grapple succesful now moving player
    }

    private void Awake()
    {
        // sets state of the grapple
        grappleState = State.grappleNormal;
    }

    void Start()
    {
        HPOrig = HP;
        jetpackFuel = jetpackFuelMax;
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
            case State.grappleNormal:
                if (!gameManager.instance.isPaused)
                movement();
                sprint();
                handleJetpackFuelRegen();
                break;
            // is grappling
            case State.grappleMoving:
                grappleMovement();
                sprint();
                handleJetpackFuelRegen();
                break;
        }
    }

    private void LateUpdate()
    {
        if (isGrappling)
            grappleRope.SetPosition(0, grappleShootPos.position);
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVelocity = Vector3.zero;
            playerMomentum = Vector3.zero;

        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);

        controller.Move(moveDir * speed * Time.deltaTime);

        jump();

        // apply momentum
        playerVelocity += playerMomentum;
        // move player
        controller.Move(playerVelocity * Time.deltaTime);
        // make player fall
        playerVelocity.y -= gravity * Time.deltaTime;
        //dampen momentum
        if (playerMomentum.magnitude >= 0f)
        {
            playerMomentum -= playerMomentum * momentumDrag * Time.deltaTime;
            if (playerMomentum.magnitude <= .0f)
            {
                playerMomentum = Vector3.zero;
            }
        }

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
            else if (weaponList[weaponListPos].type == weaponStats.weaponType.Magic )
            {
                shootMagicProjectile();
            }

        }
        // checks if clicking mouse 2 (right click)
        if (testGrappleKeyPressed())
        {
            shootGrapple();
        }

        selectWeapon();
        gunReload();
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= speedModifer;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= speedModifer;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVelocity.y = jumpSpeed;
        }
        else if ((Input.GetButton("Jump") && !controller.isGrounded) && hasJetpack)
        {
            jetpack();
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
            // Debug.Log(hit.collider.name);

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

            grappleState = State.grappleMoving;

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
            grappleState = State.grappleNormal;
            playerVelocity.y -= gravity * Time.deltaTime;
            StopGrapple();
        }

        // if use the jump key it will stop grappling 
        else if (testJumpKeyPressed())
        {
            playerMomentum = grappleSpeed * grappleDir;
            playerMomentum += Vector3.up * grappleLift;
            grappleState = State.grappleNormal;
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

        //if (weaponList.Count > 0 && weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
        //    gameManager.instance.updateAmmo(weaponList[weaponListPos].gun);
        //else
        //    gameManager.instance.hideAmmo();

        if (weaponList.Count > 0 && weaponListPos >= 0 && weaponListPos < weaponList.Count)
        {
            if (weaponList[weaponListPos].type == weaponStats.weaponType.Gun)
                gameManager.instance.updateAmmo(weaponList[weaponListPos].gun);
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
        attackRange = weaponList[weaponListPos].gun.shootRange;
        attackCooldown = weaponList[weaponListPos].gun.shootRate;

        gunModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].gun.model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].gun.model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void changeMeleeWep()
    {
        attackDamage = weaponList[weaponListPos].meleeWep.meleeDamage;
        attackRange = weaponList[weaponListPos].meleeWep.meleeDistance;

        meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].meleeWep.model.GetComponent<MeshFilter>().sharedMesh;
        meleeWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].meleeWep.model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void changeMagicWep()
    {
        attackDamage = weaponList[weaponListPos].magicWep.magicDamage;
        attackRange = weaponList[weaponListPos].magicWep.magicDitance;

        magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = weaponList[weaponListPos].magicWep.model.GetComponent<MeshFilter>().sharedMesh;
        magicWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = weaponList[weaponListPos].magicWep.model.GetComponent<MeshRenderer>().sharedMaterial;
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

        if (magicProjectile != null && magicWeaponModel != null)
        {
            //Creates the projectile at the player's position
            Instantiate(magicProjectile, magicWeaponModel.transform.position, Quaternion.LookRotation(Camera.main.transform.forward));

            ////Add a forward velocity to the projectile
            //Rigidbody rb = projectile.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.linearVelocity = Camera.main.transform.forward * magicProjectileSpeed;
            //}
        }
    }
      
    IEnumerator flashMuzzle()
    {
        muzzleFlash.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        muzzleFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.gameObject.SetActive(false);
    }

    //Updates which weapon model is active based on currentWeapon
    void updateWeaponModels()
    {
        if(gunModel != null)
        gunModel.SetActive(currentWeapon == WeaponType.Gun);
        if(meleeWeaponModel != null)
        meleeWeaponModel.SetActive(currentWeapon == WeaponType.Melee);
        if(magicWeaponModel != null)
        magicWeaponModel.SetActive(currentWeapon == WeaponType.Magic);
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
}
