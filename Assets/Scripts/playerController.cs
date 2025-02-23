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

    [Header("Range Options")]
    [SerializeField] GameObject gunModel;
    [SerializeField] gunStats startGun;
    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] Transform muzzleFlash;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDistance;
    [SerializeField] float shootRate;

    [Header("Melee Options")]
    [SerializeField] GameObject meleeWeaponModel;
    [SerializeField] meleeWepStats startMelee;
    [SerializeField] List<meleeWepStats> meleeList = new List<meleeWepStats>();
    [SerializeField] int meleeDamage;
    [SerializeField] float meleeRange;
    [SerializeField] float meleeCooldown;
    private float meleeCooldownTimer = 0f;
    [SerializeField] Animator playerAnimator;

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
    int gunListPos;
    int meleeListPos;


    float grappleCooldownTimer;
    float shootTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

    bool isSprinting;
    bool isGrappling;

    private float jetpackFuelRegenTimer;

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
            getGunStats(startGun);

        if(startMelee != null)
            getMeleeWeaponStats(startMelee);
    }

    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);
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

        //Handles melee attack
        meleeCooldownTimer -= Time.deltaTime; // Decreases cooldown timer each frame

        if (Input.GetButtonDown("MeleeAttack") && meleeCooldownTimer <= 0)
        {
            meleeAttack();
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

        shootTimer += Time.deltaTime;
        grappleCooldownTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)   //Want button input to be the first condition for performance - other evaluations wont occur unless button is pressed
        {
            shoot();
        }
        // checks if clicking mouse 2 (right click)
        if (testGrappleKeyPressed())
        {
            shootGrapple();
        }

        selectGun();
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
        shootTimer = 0;
        gunList[gunListPos].ammoCur--;
        updatePlayerUI();

        StartCoroutine(flashMuzzle());

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance, ~ignoreLayer))
        {
            // Debug.Log(hit.collider.name);

            Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.identity);

            IDamage damage = hit.collider.GetComponent<IDamage>();

            damage?.takeDamage(shootDamage);
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

        if (gunList.Count > 0)
            gameManager.instance.updateAmmo(gunList[gunListPos]);
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

    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;

        changeGun();
    }

    public void getMeleeWeaponStats(meleeWepStats melee)
    {
        meleeList.Add(melee);
        meleeListPos = meleeList.Count - 1;

        changeMeleeWep();

        if (meleeWeaponModel != null)
        {
            meleeWeaponModel.SetActive(true);
        }
    }

    void selectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeGun();
        }
    }

    void selectMelee()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && meleeListPos < meleeList.Count - 1)
        {
            meleeListPos++;
            changeMeleeWep();
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0 && meleeListPos > 0)
        {
            meleeListPos--;
            changeMeleeWep();
        }
    }

    void changeGun()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDistance = gunList[gunListPos].shootRange;
        shootRate = gunList[gunListPos].shootRate;
        muzzleFlash.SetLocalPositionAndRotation(new Vector3(gunList[gunListPos].moveFlashX,gunList[gunListPos].moveFlashY,gunList[gunListPos].moveFlashZ),muzzleFlash.rotation);

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void changeMeleeWep()
    {
        meleeDamage = meleeList[meleeListPos].meleeDamage;
        meleeRange = meleeList[meleeListPos].meleeDitance;

        meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = meleeList[meleeListPos].model.GetComponent<MeshFilter>().sharedMesh;
        meleeWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = meleeList[meleeListPos].model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void gunReload()
    {
        if (Input.GetButtonDown("Reload") && gunList.Count > 0)
        {
            if (gunList[gunListPos].ammoReserve > gunList[gunListPos].ammoMax)          //Check if the player can reload a full clip
            {
                gunList[gunListPos].ammoReserve -= (gunList[gunListPos].ammoMax - gunList[gunListPos].ammoCur);
                gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
            }
            else if (gunList[gunListPos].ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                gunList[gunListPos].ammoCur = gunList[gunListPos].ammoReserve;
                gunList[gunListPos].ammoReserve = 0;
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
        meleeCooldownTimer = meleeCooldown; // Will Reset the cooldown timer

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
        if(Physics.Raycast(transform.position, transform.forward, out hit, meleeRange, ~ignoreLayer)) 
        {
            Debug.Log("Melee hit; " + hit.collider.name);

            //Apply damage if the object hit implements IDamage
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            damageable.takeDamage(meleeDamage);
        }

    }

    IEnumerator flashMuzzle()
    {
        muzzleFlash.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        muzzleFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.gameObject.SetActive(false);
    }
}
