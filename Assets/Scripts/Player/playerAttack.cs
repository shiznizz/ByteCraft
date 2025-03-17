using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Linq;

public class playerAttack : MonoBehaviour
{
    public playerAttack instance;
    private playerController pc;

    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] AudioClip gunEmptyClip;

    private bool isMeleeAttacking = false;
    private bool isReloading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GetComponent<playerController>();
        instance = this;
    }

    public void weaponHandler()
    {
        if (isMeleeAttacking) return;

        playerStatManager.instance.attackTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && playerStatManager.instance.attackTimer >= playerStatManager.instance.attackCooldown)
        {
            if (inventoryManager.instance.weaponList[inventoryManager.instance.weaponListPos].ammoCur > 0)
                shoot();
        }

        selectWeapon();
        gunReload();
    }

    void shoot()
    {
        if (isReloading) return;
        if (inventoryManager.instance.returnCurrentWeapon().ammoCur == 0)
        {
            audioSource.PlayOneShot(gunEmptyClip);
        }
        playerStatManager.instance.attackTimer = 0;
        StartCoroutine(flashMuzzle());
        inventoryManager.instance.returnCurrentWeapon().ammoCur--;
        if (inventoryManager.instance.returnCurrentWeapon().shootSounds.Length != 0)
            playShootSound();

        if (inventoryManager.instance.returnCurrentWeapon().attackType == weaponStats.bulletType.RayCast)
        {
            //Debug.Log("Ray");
            shootRayCast();
        }
        else if (inventoryManager.instance.returnCurrentWeapon().attackType == weaponStats.bulletType.Projectile)
        {
            shootProjectile();
        }
        else if (inventoryManager.instance.returnCurrentWeapon().attackType == weaponStats.bulletType.Continuous)
        {
            shootContinuous();
        }
    }


    void shootRayCast()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, playerStatManager.instance.attackDistance, ~ignoreLayer))
        {
            //Debug.Log(hit.collider.name);
            if (inventoryManager.instance.returnCurrentWeapon().hitEffect != null)
                Instantiate(inventoryManager.instance.returnCurrentWeapon().hitEffect, hit.point, Quaternion.identity);

            //Check if the hit object is a trap
            traps hitTrap = hit.collider.GetComponent<traps>();
            if (hitTrap != null)
            {
                //Call a method in the trap script to trigger its effect
                hitTrap.TriggerTrapEffect();
            }

            IDamage damage = hit.collider.GetComponent<IDamage>();
            damage?.takeDamage(playerStatManager.instance.attackDamage);
        }


    }

    void shootProjectile()
    {
        Instantiate(inventoryManager.instance.returnCurrentWeapon().bulletObj, playerStatManager.instance.muzzleFlash.position, Camera.main.transform.rotation);
    }

    void shootContinuous()
    {

    }

    public void removeWeaponUI()
    {

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
        Debug.Log("if gun 1");

    }

    public void changeWeapon()
    {
        switch (inventoryManager.instance.weaponList[pc.weaponListPos].wepType)
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
        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();
        playerStatManager.instance.attackDamage = gun.shootDamage;
        playerStatManager.instance.attackDistance = gun.shootRange;
        playerStatManager.instance.attackCooldown = gun.shootRate;
        playerStatManager.instance.muzzleFlash.SetLocalPositionAndRotation(new Vector3(gun.moveFlashX, gun.moveFlashY, gun.moveFlashZ), playerStatManager.instance.muzzleFlash.rotation);

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = gun.model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.model.GetComponent<MeshRenderer>().sharedMaterial;

        //turnOffWeaponModels();
    }

    void gunReload()
    {
        
        if (Input.GetButtonDown("Reload") && inventoryManager.instance.weaponList.Count > 0)
        {
            isReloading = true;
            weaponStats gun = inventoryManager.instance.returnCurrentWeapon();

            if (gun.ammoReserve > gun.ammoMax)          //Check if the player can reload a full clip
            {
                gun.ammoReserve -= (gun.ammoMax - gun.ammoCur);
                gun.ammoCur = gun.ammoMax;
                if (gun.reloadSounds.Length != 0)
                    audioSource.PlayOneShot(gun.reloadSounds[Random.Range(0, gun.reloadSounds.Length)], gun.reloadVolume);
            }
            else if (gun.ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                gun.ammoCur = gun.ammoReserve;
                gun.ammoReserve = 0;
                audioSource.PlayOneShot(gun.reloadSounds[Random.Range(0, gun.reloadSounds.Length)], gun.reloadVolume);
            }
            
            //updatePlayerUI();
            StartCoroutine(ResetIsReloading());
        }
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
            if (inventoryManager.instance.weaponListPos == inventoryManager.instance.weaponList.Count - 1)
            {
                inventoryManager.instance.weaponListPos = 0;
            }
            else
            {
                inventoryManager.instance.weaponListPos++;
            }

            inventoryManager.instance.currentEquippedWeapon();
            changeWeapon();
        }
        else if (scrollInput < 0 && inventoryManager.instance.weaponList.Count > 0)
        {
            if (inventoryManager.instance.weaponListPos == 0)
            {
                inventoryManager.instance.weaponListPos = inventoryManager.instance.weaponList.Count - 1;
            }
            else
            {
                inventoryManager.instance.weaponListPos--;
            }

            changeWeapon();
            inventoryManager.instance.currentEquippedWeapon();
        }
    }

    void playShootSound()
    {
        audioSource.PlayOneShot(inventoryManager.instance.returnCurrentWeapon().shootSounds[Random.Range(0, inventoryManager.instance.returnCurrentWeapon().shootSounds.Length)], inventoryManager.instance.returnCurrentWeapon().shootVolume);
    }

    // Call this method to temporarily disable the player's weapons
    public void DisableWeapons()
    {
        isMeleeAttacking = true;
    }

    // Call this method to re-enable the player's weapons
    public void EnableWeapons()
    {
        isMeleeAttacking = false;
    }

    public IEnumerator ResetIsReloading()
    {
        yield return new WaitForSeconds(0.5f);
        isReloading = false;
    }
}
