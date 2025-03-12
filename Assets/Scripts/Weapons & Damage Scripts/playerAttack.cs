using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class playerAttack : MonoBehaviour
{
    private playerController pc;

    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        oldMovement();
    }

    void oldMovement()
    {
        playerStatManager.instance.attackTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && playerStatManager.instance.attackTimer >= playerStatManager.instance.attackCooldown)   //Want button input to be the first condition for performance - other evaluations wont occur unless button is pressed
        {
            //if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun && inventoryManager.instance.weaponList[weaponListPos].gun.ammoCur > 0)
            //{
            shoot();
            //}
            //else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Melee)
            //{
            //    meleeAttack();
            //}
            //else if (inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Magic)
            //{
            //    shootMagicProjectile();
            //}
        }
        selectWeapon();
        gunReload();
    }

    void shoot()
    {
        playerStatManager.instance.attackTimer = 0;
        inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoCur--;
        
        //updatePlayerUI();

        StartCoroutine(flashMuzzle());
        audioSource.PlayOneShot(inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootSounds[Random.Range(0, inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootSounds.Length)], inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootVolume);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, playerStatManager.instance.attackDistance, ~ignoreLayer))
        {

            Instantiate(inventoryManager.instance.weaponList[pc.weaponListPos].gun.hitEffect, hit.point, Quaternion.identity);

            //Check if the hit object is a trap
            traps hitTrap = hit.collider.GetComponent<traps>();
            if (hitTrap != null)
            {
                //Call a method in the trap script to trigger its effect
                hitTrap.TriggerTrapEffect();
            }

        }

        IDamage damage = hit.collider.GetComponent<IDamage>();

        damage?.takeDamage(playerStatManager.instance.attackDamage);
    }


    public void getWeaponStats()
    {
        inventoryManager.instance.changeWeaponPOS(); // Selects the newly added weapon
        changeWeapon();

        // isGunPOSSet = true;
    }

    public void removeWeaponUI()
    {

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
        Debug.Log("if gun 1");

    }

    void changeWeapon()
    {
        switch (inventoryManager.instance.weaponList[pc.weaponListPos].type)
        {
            case weaponStats.weaponType.primary:
                changeGun();
                break;
            case weaponStats.weaponType.secondary:
                changeGun();
                break;
            case weaponStats.weaponType.special:
                changeGun();
                break;
        }
    }

    public void changeGun()
    {
        playerStatManager.instance.attackDamage = inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootDamage;
        playerStatManager.instance.attackDistance = inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootRange;
        playerStatManager.instance.attackCooldown = inventoryManager.instance.weaponList[pc.weaponListPos].gun.shootRate;
        playerStatManager.instance.muzzleFlash.SetLocalPositionAndRotation(new Vector3(inventoryManager.instance.weaponList[pc.weaponListPos].gun.moveFlashX, inventoryManager.instance.weaponList[pc.weaponListPos].gun.moveFlashY, inventoryManager.instance.weaponList[pc.weaponListPos].gun.moveFlashZ), playerStatManager.instance.muzzleFlash.rotation);

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[pc.weaponListPos].gun.model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.gunModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[pc.weaponListPos].gun.model.GetComponent<MeshRenderer>().sharedMaterial;

        //turnOffWeaponModels();
    }

    void changeMeleeWep()
    {
        playerStatManager.instance.attackDamage = inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeDamage;
        playerStatManager.instance.attackRange = inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeDistance;

        playerStatManager.instance.attackCooldown = inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeCooldown;

        playerStatManager.instance.meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.meleeWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.model.GetComponent<MeshRenderer>().sharedMaterial;

        turnOffWeaponModels();
    }

    void changeMagicWep()
    {
        playerStatManager.instance.attackDamage = inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicDamage;
        playerStatManager.instance.attackRange = inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicDitance;

        playerStatManager.instance.attackCooldown = inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicCooldown;

        playerStatManager.instance.magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.magicWeaponModel.GetComponent<MeshRenderer>().sharedMaterial = inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.model.GetComponent<MeshRenderer>().sharedMaterial;

        turnOffWeaponModels();
    }

    void turnOffWeaponModels()
    {
        //if (meleeWeaponModel != null && inventoryManager.instance.weaponList[weaponListPos].type != weaponStats.weaponType.Melee)
        //    meleeWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;

        //if (magicWeaponModel != null && inventoryManager.instance.weaponList[weaponListPos].type != weaponStats.weaponType.Magic)
        //    magicWeaponModel.GetComponent<MeshFilter>().sharedMesh = null;

        if (playerStatManager.instance.gunModel != null && inventoryManager.instance.weaponList[pc.weaponListPos].type != weaponStats.weaponType.primary)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;

        else if (playerStatManager.instance.gunModel != null && inventoryManager.instance.weaponList[pc.weaponListPos].type != weaponStats.weaponType.secondary)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;

        else if (playerStatManager.instance.gunModel != null && inventoryManager.instance.weaponList[pc.weaponListPos].type != weaponStats.weaponType.special)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
    }

    //&& inventoryManager.instance.weaponList[weaponListPos].type == weaponStats.weaponType.Gun
    void gunReload()
    {
        if (Input.GetButtonDown("Reload") && inventoryManager.instance.weaponList.Count > 0)
        {
            if (inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoReserve > inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoMax)          //Check if the player can reload a full clip
            {
                inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoReserve -= (inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoMax - inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoCur);
                inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoCur = inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoMax;
                audioSource.PlayOneShot(inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadSounds[Random.Range(0, inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadSounds.Length)], inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadVolume);
            }
            else if (inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoCur = inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoReserve;
                inventoryManager.instance.weaponList[pc.weaponListPos].gun.ammoReserve = 0;
                audioSource.PlayOneShot(inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadSounds[Random.Range(0, inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadSounds.Length)], inventoryManager.instance.weaponList[pc.weaponListPos].gun.reloadVolume);
            }

            //updatePlayerUI();
        }
    }


    void meleeAttack()
    {
        playerStatManager.instance.attackTimer = 0; // Will Reset the cooldown timer

        if (playerStatManager.instance.playerAnimator != null)
        {
            playerStatManager.instance.playerAnimator.SetTrigger("MeleeAttack");
        }

        //StartCoroutine(toggleWepCol());
        audioSource.PlayOneShot(inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeSounds[Random.Range(0, inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeSounds.Length)], inventoryManager.instance.weaponList[pc.weaponListPos].meleeWep.meleeVolume);


        //Activate melee weapon
        if (playerStatManager.instance.meleeWeaponModel != null)
        {
            playerStatManager.instance.meleeWeaponModel.SetActive(true); // Shows the melee weapon during the attack
        }

        //Raycast to detect enemies in melee range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, playerStatManager.instance.attackRange, ~ignoreLayer))
        {
            Debug.Log("Melee hit; " + hit.collider.name);

            //Apply damage if the object hit implements IDamage
            IDamage damageable = hit.collider.GetComponent<IDamage>();
            damageable.takeDamage(playerStatManager.instance.attackDamage);
        }

    }

    void shootMagicProjectile()
    {
        playerStatManager.instance.attackTimer = 0;

        Instantiate(playerStatManager.instance.magicProjectile, playerStatManager.instance.magicPosition.position, transform.rotation);
        audioSource.PlayOneShot(inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicSounds[Random.Range(0, inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicSounds.Length)], inventoryManager.instance.weaponList[pc.weaponListPos].magicWep.magicVolume);
    }

    IEnumerator flashMuzzle()
    {
        playerStatManager.instance.muzzleFlash.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        playerStatManager.instance.muzzleFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        playerStatManager.instance.muzzleFlash.gameObject.SetActive(false);
    }

    //Switches between weapon types using a mouse scroll wheel
    void selectWeapon()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0 && inventoryManager.instance.weaponList.Count > 0)
        {
            //weaponListPos++;
            if (pc.weaponListPos == inventoryManager.instance.weaponList.Count - 1)
            {
                pc.weaponListPos = 0;
            }
            else
            {
                pc.weaponListPos++;
            }

            inventoryManager.instance.currentEquippedWeapon();
            changeWeapon();
            //&& weaponListPos < inventoryManager.instance.weaponList.Count - 1
        }
        else if (scrollInput < 0 && inventoryManager.instance.weaponList.Count > 0)
        {
            if (pc.weaponListPos == 0)
            {
                pc.weaponListPos = inventoryManager.instance.weaponList.Count - 1;
            }
            else
            {
                pc.weaponListPos--;
            }


            changeWeapon();
            inventoryManager.instance.currentEquippedWeapon();
        }
    }
}
