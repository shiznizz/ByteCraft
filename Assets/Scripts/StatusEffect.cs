using UnityEngine;
using System.Collections;

public enum StatusEffectType
{
    OnFire,
}


public class StatusEffects : MonoBehaviour
{
    [Header("General Settings")]
    public StatusEffectType effectType = StatusEffectType.OnFire;

    public float duration = 3f;

    public float tickInterval = 0.2f;

    private float timer = 0f;

    [Header("On Fire Settings")]
    public int onFireDamagePerTick = 5;

    public ParticleSystem fireEffectPrefab;

    private ParticleSystem fireEffectInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (effectType)
        {
            case StatusEffectType.OnFire:
                StartOnFire();
                break;
        }
        StartCoroutine(EffectRoutine());
    }

    private IEnumerator EffectRoutine()
    {
        while (timer < duration)
        {
            switch (effectType)
            {
                case StatusEffectType.OnFire:
                    OnFireTick();
                    break;
            }
            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }
        EndEffect();
    }

    #region On Fire Effect Methods

    private void StartOnFire()
    {
        if (fireEffectPrefab != null)
        {
            fireEffectInstance = Instantiate(fireEffectPrefab, transform);
            fireEffectInstance.Play();
        }
    }

    private void OnFireTick()
    {
        IDamage damageable = GetComponent<IDamage>();
        if (damageable != null)
        {
            damageable.takeDamage(onFireDamagePerTick);
        }
    }

    #endregion On Fire Effect Methods

    private void EndEffect()
    {
        switch (effectType)
        {
            case StatusEffectType.OnFire:
                if (fireEffectInstance != null)
                {
                    fireEffectInstance.Stop();
                    Destroy(fireEffectInstance.gameObject, 2f);
                }
                break;
        }
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
