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
    public int DOT = 2;
    public float duration = 3;
    public float interval = 0.5f;
    public StatusEffectType effectType = 0;

    [Header("Lingering Status Effect Settings")]
    public StatusEffects statusEffectBlueprint;

    // track how much time has passed for each target in the hazard.
    private Dictionary<Collider, float> targetTimers = new Dictionary<Collider, float>();

    //// called when a collider enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // check if the collider belongs to a valid target (using tags "Player" or "Enemy").
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (!other.GetComponent<StatusEffects>())
                ApplyStatusEffect(other.gameObject);
        }
    }

    // call every frame while a collider remains inside the trigger.
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {

            if (!other.GetComponent<StatusEffects>())
                ApplyStatusEffect(other.gameObject);
        }
    }

    //// collider exits the trigger.
    //private void OnTriggerExit(Collider other)
    //{
    //    // remove the target from our timer dictionary so it stops receiving DOT damage.
    //    if (targetTimers.ContainsKey(other))
    //    {
    //        targetTimers.Remove(other);
    //    }
    //}

    public void ApplyStatusEffect(GameObject target)
    {
        StatusEffects effect = target.AddComponent<StatusEffects>();
        effect.InitializeStatus(DOT, interval, duration, effectType);
    }
}
