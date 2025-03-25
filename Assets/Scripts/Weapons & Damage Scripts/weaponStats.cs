using UnityEngine;

[CreateAssetMenu]

public class weaponStats : itemSO
{
    public enum weaponType { primary, secondary, special }
    public enum bulletType { RayCast, Projectile, Continuous}
    public weaponType wepType;
    public bulletType attackType;

    public GameObject model;

    [Header("Adjustment for muzzle flash placement")]
    public Transform flashPOS;
    //public float moveFlashX;
    //public float moveFlashY;
    //public float moveFlashZ;

    [Header("Weapon Stats")]
    public int shootDamage;
    public int shootRange;
    public float shootRate;
    public int ammoCur, ammoMax, ammoReserve, ammoReserveMax;
    public GameObject bulletObj;
    public bool isChargeWeapon;
    public float chargeTime;

    [Header("Continous Specific Stats")]
    public float continuousRadius;
    public float maxRange;
    public float extensionSpeed;
    public float currLength;
    public float tickRate;
    public float heatGenPerShot;
    public bool canOverheat;

    [Header("Visuals and Sounds")]
    public ParticleSystem hitEffect;
    public AudioClip[] shootSounds;
    [Range(0, 1)] public float shootVolume;
    public AudioClip[] reloadSounds;
    [Range(0, 1)] public float reloadVolume;
    public AudioClip[] noAmmoSounds;
    [Range(0, 1)] public float noAmmoVolume;

    public void RefreshAmmo()
    {
        ammoCur = ammoMax;
        ammoReserve = ammoReserveMax;
    }

    public override itemSO GetItem()
    {
        return this;
    }

    public override ArmorSO GetArmor()
    {
        return null;
    }

    public override weaponStats GetWeapon()
    {
        return this;
    }

    public void setFlashPosition()
    {
        flashPOS = model.transform.GetChild(0);
    }
}
