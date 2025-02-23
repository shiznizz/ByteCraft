using UnityEngine;

[CreateAssetMenu]

public class meleeWepStats : ScriptableObject
{
    public GameObject model;
    public int meleeDamage;
    public int meleeDistance;
    //public float meleeRange;

    public ParticleSystem hitEffect;
    public AudioClip[] meleeSounds;
    public float meleeVolume;
}
