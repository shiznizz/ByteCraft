using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public GameObject model;
    public int shootDamage;
    public int shootRange;
    public float shootRate;
    public int ammoCur, ammoMax, ammoReserve, ammoReserveMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSounds;
    public float shootVolume;
}
