using UnityEngine;
using System.Collections;

public enum StatusEffectType
{
    OnFire,
    Acid,
    Stun
}


public class StatusEffects : MonoBehaviour
{
    [Header("General Settings")]
    public StatusEffectType effectType = StatusEffectType.OnFire;

    public float duration = 3f;

    public float tickInterval = 0.2f;

    // keep track of how much time has passed
    public float timer = 0f;

    #region On Fire Settings

    [Header("On Fire Settings")]
    public int onFireDamagePerTick = 5;

    public ParticleSystem fireEffectPrefab;

    private ParticleSystem fireEffectInstance;

    #endregion On Fire Settings

    #region Acid Settings
    [Header("Acid Settings")]
    public int acidDamagePerTick = 2;

    public float acidDamageMultiplier = 2f;

    // original dmg multiplier is stored for after acid effect ends
    private float originalDamageMultiplier = 1f;

    // flag to ensure only storing multiplier once
    private bool multiplierApplied = false;

    #endregion Acid Settings

    #region Stun Settings
    [Header("Stun Settings")]
    public ParticleSystem stunEffectPrefab;

    // instantiated stun effect on target
    private ParticleSystem stunEffectInstance;

    // referencing enemyAI for isStunned
    private enemyAI stunnedEnemyAI;

    #endregion Stun Settings

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize effects based on type
        switch (effectType)
        {
            case StatusEffectType.OnFire:
                StartOnFire();
                break;

            case StatusEffectType.Acid:
                StartAcidEffect();
                break;

            case StatusEffectType.Stun:
                StartStunEffect();
                break;
        }
        // begin effect logic, will run until effect expires
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        // continue applying effect logic until total duration is reached
        while (timer < duration)
        {
            switch (effectType)
            {
                case StatusEffectType.OnFire:
                    OnFireTick();
                    break;

                case StatusEffectType.Acid:
                    AcidTick();
                    break;

                case StatusEffectType.Stun:
                    StunTick();
                    break;
            }
            // wait for specified tick intervals before repeating
            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }
        // end and clean up after status effect duration times out
        EndEffect();
    }

    #region On Fire Effect Methods

    private void StartOnFire()
    {
        if (fireEffectPrefab != null)
        {
            // instantiate fire particle effects as child of game obj
            fireEffectInstance = Instantiate(fireEffectPrefab, transform);
            fireEffectInstance.Play();
        }
    }

    private void OnFireTick()
    {
        // if the obj implements IDamage, apply damage
        IDamage damageable = GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.takeDamage(onFireDamagePerTick);
        }
    }

    #endregion On Fire Effect Methods

    #region Acid Methods

    // initializes acid effect by applying a dmg multiplier to target
    private void StartAcidEffect()
    {
        var ps = GetComponent<playerStatManager>();
        if (ps != null)
        {
            // store original dmg multiplier 
            originalDamageMultiplier = ps.damageMultiplier;
            // apply acid multiplier
            ps.damageMultiplier = acidDamageMultiplier;
            multiplierApplied = true;
        }
    }

    // applies DOT dmg for acid effect
    private void AcidTick()
    {
        IDamage damageable = GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.takeDamage(acidDamagePerTick);
        }
    }

    private void EndAcidEffect()
    {
        // if multiplier was successful, restore original val
        if (multiplierApplied)
        {
            var ps = GetComponent<playerStatManager>();
            if (ps != null)
            {
                ps.damageMultiplier = originalDamageMultiplier;
            }
            multiplierApplied = false;
        }
    }

    #endregion Acid Methods

    #region Stun Methods
    private void StartStunEffect()
    {
        // find enemyAI component to flag as stunned
        stunnedEnemyAI = GetComponent<enemyAI>();
        if (stunnedEnemyAI != null)
        {

            stunnedEnemyAI.isStunned = true;
        }

        if (stunEffectPrefab != null)
        {
            stunEffectInstance = Instantiate(stunEffectPrefab, transform);
            stunEffectInstance.Play();
        }
    }

    private void StunTick()
    {

    }

    private void EndStunEffect()
    {
        // reenable normal enemy behavior
        if (stunnedEnemyAI != null)
        {
            stunnedEnemyAI.isStunned = false;
        }

        // remove stun particle effect
        if (stunEffectInstance != null)
        {
            stunEffectInstance.Stop();
            Destroy(stunEffectInstance.gameObject, 2f);
        }
    }

    #endregion Stun Methods

    private void EndEffect()
    {
        switch (effectType)
        {
            case StatusEffectType.OnFire:
                if (fireEffectInstance != null)
                {
                    fireEffectInstance.Stop();
                    // allow the particles to fade out before destroying
                    Destroy(fireEffectInstance.gameObject, 2f);
                }
                break;

            case StatusEffectType.Acid:
                EndAcidEffect();
                break;

            case StatusEffectType.Stun:
                EndStunEffect();
                break;
        }
        // remove relative effect component from game obj
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
