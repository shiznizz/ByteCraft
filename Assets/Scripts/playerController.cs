using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    

    [SerializeField] int speed;
    [SerializeField] int speedModifer;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDistance;

    [SerializeField] float shootRate;

    int jumpCount;

    float shootTimer;

    private Vector3 moveDir;
    private Vector3 playerVelocity;


    private void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);
        movement();
        sprint();
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
}
