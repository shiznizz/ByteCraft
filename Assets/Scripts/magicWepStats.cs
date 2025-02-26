using UnityEngine;

[CreateAssetMenu]

public class magicWepStats : weaponStats
{
    public GameObject model;
    public int magicDamage;
    public int magicDitance;
    public int magicCooldown;
    //public float meleeRange;

    public ParticleSystem hitEffect;
    public AudioClip[] magicSounds;
    public float magicVolume;
}
