using UnityEngine;

[CreateAssetMenu]

public class meleeWepStats : weaponStats
{
    public GameObject model;
    public int meleeDamage;
    public int meleeDistance;
    public int meleeCooldown;
    //public float meleeRange;

    public ParticleSystem hitEffect;
    public AudioClip[] meleeSounds;
    public float meleeVolume;
}
