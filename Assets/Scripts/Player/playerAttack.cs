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
    private inventoryManager inv;

    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;

    private bool isMeleeAttacking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GetComponent<playerController>();
        inv = GetComponent<inventoryManager>();
        instance = this;
    }

    public void weaponHandler()
    {
        if (isMeleeAttacking) return;

        playerStatManager.instance.attackTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && playerStatManager.instance.attackTimer >= playerStatManager.instance.attackCooldown)
        {
            shoot();
        }

        selectWeapon();
        gunReload();
    }

    void shoot()
    {
        playerStatManager.instance.attackTimer = 0;
        StartCoroutine(flashMuzzle());
        if (inv.returnCurrentWeapon().shootSounds.Length != 0)
            playShootSound();

        if (inv.returnCurrentWeapon().attackType == weaponStats.bulletType.RayCast)
        {
            Debug.Log("Ray");
            shootRayCast();
        }
        else if (inv.returnCurrentWeapon().attackType == weaponStats.bulletType.Projectile)
        {
            shootProjectile();
        }
        else if (inv.returnCurrentWeapon().attackType == weaponStats.bulletType.Continuous)
        {
            shootContinuous();
        }
    }


    void shootRayCast()
    {
        inv.returnCurrentWeapon().ammoCur--;


        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, playerStatManager.instance.attackDistance, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            if (inv.returnCurrentWeapon().hitEffect != null)
                Instantiate(inv.returnCurrentWeapon().hitEffect, hit.point, Quaternion.identity);

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
        Instantiate(inv.returnCurrentWeapon().bulletObj, playerStatManager.instance.muzzleFlash.position, transform.rotation);
    }

    void shootContinuous()
    {

    }

    public void getWeaponStats()
    {
        inv.changeWeaponPOS(); // Selects the newly added weapon
        changeWeapon();
    }

    public void removeWeaponUI()
    {

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
        Debug.Log("if gun 1");

    }

    void changeWeapon()
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
        playerStatManager.instance.attackDamage = inv.returnCurrentWeapon().shootDamage;
        playerStatManager.instance.attackDistance = inv.returnCurrentWeapon().shootRange;
        playerStatManager.instance.attackCooldown = inv.returnCurrentWeapon().shootRate;
        playerStatManager.instance.muzzleFlash.SetLocalPositionAndRotation(new Vector3(inv.returnCurrentWeapon().moveFlashX, inv.returnCurrentWeapon().moveFlashY, inv.returnCurrentWeapon().moveFlashZ), playerStatManager.instance.muzzleFlash.rotation);

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = inv.returnCurrentWeapon().model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.gunModel.GetComponent<MeshRenderer>().sharedMaterial = inv.returnCurrentWeapon().model.GetComponent<MeshRenderer>().sharedMaterial;

        //turnOffWeaponModels();
    }

    void turnOffWeaponModels()
    {
        if (playerStatManager.instance.gunModel != null && inv.returnCurrentWeapon().wepType != weaponStats.weaponType.primary)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;

        else if (playerStatManager.instance.gunModel != null && inv.returnCurrentWeapon().wepType != weaponStats.weaponType.secondary)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;

        else if (playerStatManager.instance.gunModel != null && inv.returnCurrentWeapon().wepType != weaponStats.weaponType.special)
            playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
    }

    void gunReload()
    {
        if (Input.GetButtonDown("Reload") && inventoryManager.instance.weaponList.Count > 0)
        {
            if (inv.returnCurrentWeapon().ammoReserve > inv.returnCurrentWeapon().ammoMax)          //Check if the player can reload a full clip
            {
                inv.returnCurrentWeapon().ammoReserve -= (inv.returnCurrentWeapon().ammoMax - inv.returnCurrentWeapon().ammoCur);
                inv.returnCurrentWeapon().ammoCur = inv.returnCurrentWeapon().ammoMax;
                audioSource.PlayOneShot(inv.returnCurrentWeapon().reloadSounds[Random.Range(0, inv.returnCurrentWeapon().reloadSounds.Length)], inv.returnCurrentWeapon().reloadVolume);
            }
            else if (inv.returnCurrentWeapon().ammoReserve > 0)                               //If there is ammo in reserve but not a full clip reload remaining ammo
            {
                inv.returnCurrentWeapon().ammoCur = inv.returnCurrentWeapon().ammoReserve;
                inv.returnCurrentWeapon().ammoReserve = 0;
                audioSource.PlayOneShot(inv.returnCurrentWeapon().reloadSounds[Random.Range(0, inv.returnCurrentWeapon().reloadSounds.Length)], inv.returnCurrentWeapon().reloadVolume);
            }

            //updatePlayerUI();
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
        if (scrollInput > 0 && inv.weaponList.Count > 0)
        {
            //weaponListPos++;
            if (inv.weaponListPos == inv.weaponList.Count - 1)
            {
                inv.weaponListPos = 0;
            }
            else
            {
                inv.weaponListPos++;
            }

            inv.currentEquippedWeapon();
            changeWeapon();
        }
        else if (scrollInput < 0 && inv.weaponList.Count > 0)
        {
            if (inv.weaponListPos == 0)
            {
                inv.weaponListPos = inv.weaponList.Count - 1;
            }
            else
            {
                inv.weaponListPos--;
            }

            changeWeapon();
            inv.currentEquippedWeapon();
        }
    }

    void playShootSound()
    {
        audioSource.PlayOneShot(inv.returnCurrentWeapon().shootSounds[Random.Range(0, inv.returnCurrentWeapon().shootSounds.Length)], inv.returnCurrentWeapon().shootVolume);
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
}
