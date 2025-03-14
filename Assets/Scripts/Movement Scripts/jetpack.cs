using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jetpackScript : MonoBehaviour
{
    private playerController pc;
    private Rigidbody rb;

    Coroutine jetpackCoroutine;

    private float jetpackFuelRegenTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<playerController>();

        jetpackFuelRegenTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStatManager.instance.hasJetpack)
            rigidJump();

        handleJetpackFuelRegen();
    }

    void FixedUpdate()
    {
        if (pc.isJetpacking)
            jetpack();
    }

    void rigidJump()
    {
        if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(transform.up * playerStatManager.instance.jumpForce, ForceMode.Impulse);

            playerStatManager.instance.jumpCount++;
        }
        else if (Input.GetButtonDown("Jump") && !pc.isJetpacking && !pc.isGrounded && playerStatManager.instance.hasJetpack)
        {
            // if existing jetpackCoroutine stop routine
            if (jetpackCoroutine != null)
                StopCoroutine(jetpackCoroutine);
            // start jetpack wait timer and enable jetpack
            jetpackCoroutine = StartCoroutine(jetpackWait());
        }

        if (Input.GetButtonUp("Jump"))
        {
            // stop coroutine and disable jetpack
            if (jetpackCoroutine != null)
            {
                StopCoroutine(jetpackCoroutine);
                jetpackCoroutine = null;
            }

            pc.isJetpacking = false;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && playerStatManager.instance.jumpCount < playerStatManager.instance.jumpMax)
        {
            //    playerStatManager.instance.jumpCount++;
            //    playerVelocity.y = playerStatManager.instance.jumpForce;

            //if (pc.isCrouching || pc.isSliding)
                ////exitCrouch();
            //if (pc.isWallRunning)
                ////wallJump();
        }
        else if (Input.GetButtonDown("Jump") && !pc.isJetpacking && !pc.isGrounded && playerStatManager.instance.hasJetpack)
        {
            // if existing jetpackCoroutine stop routine
            if (jetpackCoroutine != null)
                StopCoroutine(jetpackCoroutine);
            // start jetpack wait timer and enable jetpack
            jetpackCoroutine = StartCoroutine(jetpackWait());
        }

        if (pc.isJetpacking)
            jetpack();

        // stop jetpack and jetpack coroutine
        if (Input.GetButtonUp("Jump"))
        {
            // stop coroutine and disable jetpack
            if (jetpackCoroutine != null)
            {
                StopCoroutine(jetpackCoroutine);
                jetpackCoroutine = null;
            }

            pc.isJetpacking = false;
        }
    }

    void jetpack()
    {
        if (playerStatManager.instance.jetpackFuel > 0)
        {
            playerStatManager.instance.jetpackFuel -= playerStatManager.instance.jetpackFuelUse * Time.deltaTime;
            
            rb.AddForce(Vector3.up * playerStatManager.instance.jetpackSpeed);

            jetpackFuelRegenTimer = playerStatManager.instance.jetpackFuelRegenDelay;
        }
    }

    void handleJetpackFuelRegen()
    {
        if (playerStatManager.instance.jetpackFuel < playerStatManager.instance.jetpackFuelMax)
        {
            // Decrease the regen timer over time
            jetpackFuelRegenTimer -= Time.deltaTime;

            // Regenerate fuel only after the delay has passed
            if (jetpackFuelRegenTimer <= 0)
            {
                playerStatManager.instance.jetpackFuel += playerStatManager.instance.jetpackFuelRegen * Time.deltaTime;
                playerStatManager.instance.jetpackFuel = Mathf.Clamp(playerStatManager.instance.jetpackFuel, 0, playerStatManager.instance.jetpackFuelMax); // Clamp fuel between 0 and max
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
        yield return new WaitForSeconds(playerStatManager.instance.jetpackHoldTimer);

        pc.isJetpacking = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        jetpackCoroutine = null;
    }
}
