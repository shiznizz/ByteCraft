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
    public static playerAttack instance;

    private playerController pc;

    [SerializeField] AudioSource audioSource;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] AudioClip gunEmptyClip;

    private bool isMeleeAttacking = false;
    private bool isReloading = false;

    private BoxCollider continuousCollider;
    private Transform continuousMesh;

    private void Awake()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pc = GetComponent<playerController>();
        
    }

    public void weaponHandler()
    {
        if (isMeleeAttacking) return;

        playerStatManager.instance.attackTimer += Time.deltaTime;

        if (Input.GetButton("Fire1") && inventoryManager.instance.weaponList.Count > 0 && playerStatManager.instance.attackTimer >= playerStatManager.instance.attackCooldown)
        {
            if (inventoryManager.instance.weaponList[inventoryManager.instance.weaponListPos].ammoCur > 0)
                shoot();
            else
                audioSource.PlayOneShot(gunEmptyClip, 0.1f);
        }
        hotKeyWeapon();
        selectWeapon();
        gunReload();
    }

    void shoot()
    {
        if (isReloading) return;

        playerStatManager.instance.attackTimer = 0;
        StartCoroutine(flashMuzzle());
        inventoryManager.instance.returnCurrentWeapon().ammoCur--;
        if (inventoryManager.instance.returnCurrentWeapon().shootSounds.Length != 0)
            playShootSound();

        if (inventoryManager.instance.returnCurrentWeapon().attackType == weaponStats.bulletType.RayCast)
        {
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
        else if (inventoryManager.instance.returnCurrentWeapon().attackType == weaponStats.bulletType.lobber)
        {
            shootLobber();
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
        //Instantiate(inventoryManager.instance.returnCurrentWeapon().bulletObj, playerStatManager.instance.muzzleFlash.position, Camera.main.transform.rotation);
        continuousCollider = inventoryManager.instance.returnCurrentWeapon().bulletObj.GetComponent<BoxCollider>();
        continuousMesh = inventoryManager.instance.returnCurrentWeapon().bulletObj.GetComponent<MeshFilter>().transform;

        continuousCollider.enabled = true;
        continuousCollider.size = new Vector3(continuousCollider.size.x, continuousCollider.size.y, 100);
        continuousCollider.center = new Vector3(0, 0, 100 / 2f);
        continuousMesh.localScale = new Vector3(1, 1, 100);

        if (Input.GetButtonUp("Fire1"))
        {
            continuousCollider.size = new Vector3(continuousCollider.size.x, continuousCollider.size.y, 0);
            continuousMesh.localScale = Vector3.one;
            //inventoryManager.instance.returnCurrentWeapon().bulletObj.SetActive(false);

        }
    }

    void shootLobber()
    {

    }

    public void removeWeaponUI()
    {
        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = null;
        playerStatManager.instance.gunModel.GetComponent<MeshRenderer>().sharedMaterial = null;
        gameManager.instance.hideAmmo();
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

        Vector3 gunPOS = playerStatManager.instance.gunModel.transform.localPosition;
        gun.setFlashPosition();
        Vector3 flashPOS = gun.flashPOS.localPosition;
        float scale = 0.5f;

        //playerStatManager.instance.muzzleFlash.SetLocalPositionAndRotation(new Vector3(gun.moveFlashX, gun.moveFlashY, gun.moveFlashZ), playerStatManager.instance.muzzleFlash.rotation);
        playerStatManager.instance.muzzleFlash.SetLocalPositionAndRotation(new Vector3(gunPOS.x-(flashPOS.z*scale), gunPOS.y + (flashPOS.y*scale), gunPOS.z+(flashPOS.x*scale)), playerStatManager.instance.muzzleFlash.rotation);
        

        playerStatManager.instance.gunModel.GetComponent<MeshFilter>().sharedMesh = gun.model.GetComponent<MeshFilter>().sharedMesh;
        playerStatManager.instance.gunModel.GetComponent<MeshRenderer>().sharedMaterial = gun.model.GetComponent<MeshRenderer>().sharedMaterial;
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
        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();

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
    void hotKeyWeapon()
    {
        int weaponIndex;
        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryManager.instance.weaponList[0] != null)
        {
            
            weaponIndex = inventoryManager.instance.weaponList.IndexOf(inventoryManager.instance.slotBossScript.primaryWeapon.weapon);

            inventoryManager.instance.weaponListPos = weaponIndex;
            changeWeapon();
            inventoryManager.instance.currentEquippedWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryManager.instance.weaponList[1] != null)
        {
            
            weaponIndex = inventoryManager.instance.weaponList.IndexOf(inventoryManager.instance.slotBossScript.secondaryWeapon.weapon);

            inventoryManager.instance.weaponListPos = weaponIndex;
            changeWeapon();
            inventoryManager.instance.currentEquippedWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryManager.instance.weaponList[2] != null)
        {
            
            weaponIndex = inventoryManager.instance.weaponList.IndexOf(inventoryManager.instance.slotBossScript.specialWeapon.weapon);

            inventoryManager.instance.weaponListPos = weaponIndex;
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
