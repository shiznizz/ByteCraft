using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
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
    public float shootVolume;
}
