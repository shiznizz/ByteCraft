using UnityEngine;

[CreateAssetMenu]

public class meleeWepStats : ScriptableObject
{
    public GameObject model;
    public int meleeDamage;
    public int meleeDitance;
    //public float meleeRange;

    public ParticleSystem hitEffect;
    public AudioClip[] meleeSounds;
    public float meleeVolume;
}
