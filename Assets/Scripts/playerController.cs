using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int speedModifer;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;

    [SerializeField] int jetpackFuelMax;
    [SerializeField] float jetpackFuelUse;
    [SerializeField] float jetpackFuelRegen;
    [SerializeField] float jetpackFuelRegenDelay;
    [SerializeField] int jetpackSpeed;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDistance;
    [SerializeField] float shootRate;

    int jumpCount;

    float shootTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;

    bool isSprinting;

    float jetpackFuel;
    private float jetpackFuelRegenTimer;

    void Start()
    {
        jetpackFuel = jetpackFuelMax;
        jetpackFuelRegenTimer = 0f;
    }

    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);
        movement();
        sprint();
        handleJetpackFuelRegen();
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

        controller.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity * Time.deltaTime;

        shootTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
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
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVelocity.y = jumpSpeed;
        }
        else if (Input.GetButton("Jump") && !controller.isGrounded)
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

        if (Physics.Raycast(Camera.main.transform.position,Camera.main.transform.forward, out hit, shootDistance, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage damage = hit.collider.GetComponent<IDamage>();

            damage?.takeDamage(shootDamage);  
        }
    }

    public void takeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            // Destroy(gameObject);
        }
    }
}
