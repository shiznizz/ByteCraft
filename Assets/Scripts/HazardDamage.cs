using UnityEngine;
using System.Collections.Generic;


public enum HazardType
{
    Fire,
    Acid,
    Stun
}

public class HazardDamage : MonoBehaviour
{
    [Header("Immediate Damage Settings")]
    public int immediateDamage = 5;

    [Header("DOT Settings")]
    public int dotDamagePerTick = 2;

    public float dotInterval = 0.5f;

    [Header("Lingering Status Effect Settings")]
    public StatusEffects statusEffectBlueprint;

    // track how much time has passed for each target in the hazard.
    private Dictionary<Collider, float> targetTimers = new Dictionary<Collider, float>();

    // called when a collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // check if the collider belongs to a valid target (using tags "Player" or "Enemy").
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // apply immediate damage if configured.
            if (immediateDamage > 0)
            {
                IDamage damageable = other.GetComponent<IDamage>();
                if (damageable != null)
                {
                    damageable.takeDamage(immediateDamage);
                }
            }

            // apply a lingering status effect using the blueprint if one is provided.
            if (statusEffectBlueprint != null)
            {
                StatusEffects.ApplyStatusEffect(other.gameObject, statusEffectBlueprint);
            }

            // initialize a DOT timer for this target.
            if (!targetTimers.ContainsKey(other))
            {
                targetTimers.Add(other, 0f);
            }
        }
    }

    // call every frame while a collider remains inside the trigger.
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (targetTimers.ContainsKey(other))
            {
                // Increment the DOT timer for this target.
                float timer = targetTimers[other] + Time.deltaTime;

                // Once the timer exceeds the interval, apply DOT damage and reset the timer.
                if (timer >= dotInterval)
                {
                    IDamage damageable = other.GetComponent<IDamage>();
                    if (damageable != null && dotDamagePerTick > 0)
                    {
                        damageable.takeDamage(dotDamagePerTick);
                    }
                    timer = 0f;
                }
                targetTimers[other] = timer;
            }
        }
    }

    // collider exits the trigger.
    private void OnTriggerExit(Collider other)
    {
        // remove the target from our timer dictionary so it stops receiving DOT damage.
        if (targetTimers.ContainsKey(other))
        {
            targetTimers.Remove(other);
        }
    }
}
