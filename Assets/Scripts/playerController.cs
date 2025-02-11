using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class playerController : MonoBehaviour
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Player Options")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int speedModifer;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] float momentumDrag;

    [Header("JetPack Options")]
    [SerializeField] int jetpackFuelMax;
    [SerializeField] float jetpackFuelUse;
    [SerializeField] float jetpackFuelRegen;
    [SerializeField] float jetpackFuelRegenDelay;
    [SerializeField] int jetpackSpeed;

    [Header("Weapon Options")]
    [SerializeField] int shootDamage;
    [SerializeField] int shootDistance;
    [SerializeField] float shootRate;

    [Header("Grapple Options")]
    [SerializeField] int grappleDistance;
    [SerializeField] int grappleLaunchSpeed;
    [SerializeField] int grappleLift;
    [SerializeField] float grappleSpeedMultiplier;
    [SerializeField] float grappleSpeedMin;
    [SerializeField] float grappleSpeedMax;
    [SerializeField] float grappleCooldown;



    // holds state of the grapple 
    private State grappleState;

    int jumpCount;

    float grappleCooldownTimer;
    float shootTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;
    private Vector3 playerMomentum;
    private Vector3 grapplePostion;

    bool isSprinting;

    float jetpackFuel;
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
        jetpackFuel = jetpackFuelMax;
        jetpackFuelRegenTimer = 0f;
    }

    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);
        // switches states of grapple
        switch (grappleState)
        {
            // not grappling 
            case State.grappleNormal:

                movement();
                sprint();
                handleJetpackFuelRegen();
                break;
            // is grappling
            case State.grappleMoving:
                grappleMovement();
                break;
        }
    }


    void movement()
    {
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVelocity = Vector3.zero;
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
        // dampen momentum
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

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
        // checks if clicking mouse 2 (right click)
        if (testGrappleKeyPressed())
        {
            shootGrapple();
        }
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
        if (testJumpKeyPressed() && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVelocity.y = jumpSpeed;
        }
        else if (testJumpKeyPressed() && !controller.isGrounded)
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

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

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
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, grappleDistance))
        {

            Debug.Log(hit.collider.name);

            grapplePostion = hit.point;
            grappleState = State.grappleMoving;

        }

    }
    // handles the grapple moving the character
    void grappleMovement()
    {

        float grappleSpeed = Mathf.Clamp(Vector3.Distance(transform.position, grapplePostion), grappleSpeedMin, grappleSpeedMax);
        // direction the player will move
        Vector3 grappleDir = (grapplePostion - transform.position).normalized;
        // moving the player
        controller.Move(grappleDir * grappleSpeed * grappleSpeedMultiplier * Time.deltaTime);

        // checks if reached end of grapple
        float grapleDistanceMove = 1f;
        if (Vector3.Distance(transform.position, grapplePostion) < grapleDistanceMove)
        {
            grappleState = State.grappleNormal;
            playerVelocity.y -= gravity * Time.deltaTime;
        }

        // if use the jump key it will stop grappling where you are
        else if (testJumpKeyPressed())
        {
            playerMomentum = grappleLaunchSpeed * grappleSpeed * grappleDir;
            playerMomentum += Vector3.up * grappleLift;
            grappleState = State.grappleNormal;
            playerVelocity.y -= gravity * Time.deltaTime;
        }

    }

    public void takeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            // Destroy(gameObject);
            //
        }
    }

    // tests if the grapple key is pressed and returns a bool
    bool testGrappleKeyPressed()
    {
        if (Input.GetButton("Fire2") && grappleCooldownTimer >= grappleCooldown)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // tests if the jump key is pressed and returns a bool
    bool testJumpKeyPressed()
    {
        if (Input.GetButton("Jump"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
