using UnityEngine;

[CreateAssetMenu]

public class gunStats : weaponStats
{
    public GameObject model;

    [Header("Adjustment for muzzle flash placement")]
    public float moveFlashX;
    public float moveFlashY;
    public float moveFlashZ;

    [Header("Weapon Stats")]
    public int shootDamage;
    public int shootRange;
    public float shootRate;
    public int ammoCur, ammoMax, ammoReserve, ammoReserveMax;

    [Header("Visuals and Sounds")]
    public ParticleSystem hitEffect;
    public AudioClip[] shootSounds;
    [Range(0, 1)] public float shootVolume;
    public AudioClip[] reloadSounds;
    [Range(0,1)] public float reloadVolume;

    public void RefreshAmmo()
    {
       // if (type == weaponType.Gun)
        //{
            gun.ammoCur = gun.ammoMax;
            gun.ammoReserve = gun.ammoReserveMax;
        //}
    }
}
