using UnityEngine;
using System.Collections.Generic;


public class HazardDamage : MonoBehaviour
{
    [Header("Immediate Damage Settings")]
    public int immediateDamage = 5;

    [Header("DOT Settings")]
    public int damagePerTick = 2;

    public float damageInterval = 0.5f;

    // track how much time has passed for each target in the hazard.
    private Dictionary<Collider, float> targetTimers = new Dictionary<Collider, float>();

    // called when a collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // check if the collider belongs to a valid target (using tags "Player" or "Enemy").
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // apply immediate damage if the target has an IDamage component.
            IDamage damageable = other.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.takeDamage(immediateDamage);
            }

            // initialize a damage timer for this target.
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
            // update the timer for this target.
            if (targetTimers.ContainsKey(other))
            {
                targetTimers[other] += Time.deltaTime;

                // once the timer reaches the defined damage interval, apply DOT damage.
                if (targetTimers[other] >= damageInterval)
                {
                    IDamage damageable = other.GetComponent<IDamage>();
                    if (damageable != null)
                    {
                        damageable.takeDamage(damagePerTick);
                    }
                    // reset the timer for this target.
                    targetTimers[other] = 0f;
                }
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
