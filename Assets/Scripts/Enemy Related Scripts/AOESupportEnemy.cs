using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOESupportEnemy : MonoBehaviour
{
    [Header("Aura Settings")]
    [SerializeField] private float auraRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask supportEnemyLayer;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private GameObject auraEffectPrefab;
    private GameObject auraEffectInstance;

    [Header("Buff Settings")]
    [SerializeField] private float buffDuration = 2f; // How often to refresh buff
    [SerializeField] private float hpBuffMultiplier = 5f;
    [SerializeField] private float damageBuffMultiplier = 1.15f;
    [SerializeField] private float speedBuffMultiplier = 1.25f;

    private Dictionary<enemyAI, Coroutine> buffedEnemies = new Dictionary<enemyAI, Coroutine>();

    void Start()
    {
        // Spawn visible aura effect
        if (auraEffectPrefab != null)
        {
            auraEffectInstance = Instantiate(auraEffectPrefab, transform.position, Quaternion.identity, transform);
            auraEffectInstance.transform.localScale = new Vector3(auraRadius * 2, 0.1f, auraRadius * 2); // Visible radius
        }

        StartCoroutine(AuraEffectRoutine());
    }

    private IEnumerator AuraEffectRoutine()
    {
        while (true)
        {
            int excludedLayers = floorLayer | supportEnemyLayer;
            int includedLayers = ~excludedLayers;
            Collider[] enemiesInAura = Physics.OverlapSphere(transform.position, auraRadius, includedLayers);

            // Track and buff enemies within the aura
            foreach (Collider enemy in enemiesInAura)
            {
                //if (enemy.TryGetComponent(out enemyAI enemyScript)) Debug.Log("Grabbed enemy script");
                if (enemy.TryGetComponent(out enemyAI enemyScript) && !buffedEnemies.ContainsKey(enemyScript))
                {
                    Coroutine buffCoroutine = StartCoroutine(ApplyBuff(enemyScript));
                    buffedEnemies.Add(enemyScript, buffCoroutine);
                }
            }

            // Remove buffs from enemies that have left the aura
            List<enemyAI> enemiesToRemove = new List<enemyAI>();
            foreach (var entry in buffedEnemies)
            {
                if (!IsEnemyInAura(entry.Key))
                {
                    StopCoroutine(entry.Value);
                    ResetStats(entry.Key);
                    enemiesToRemove.Add(entry.Key);
                }
            }

            foreach (var enemy in enemiesToRemove)
            {
                buffedEnemies.Remove(enemy);
            }

            yield return new WaitForSeconds(0.5f); // Check aura every 0.5 seconds
        }
    }

    private IEnumerator ApplyBuff(enemyAI enemy)
    {
        // Store original stats to reset later
        int originalHP = enemy.HP;
        float originalSpeed = enemy.agent.speed;

        enemy.SetHP(Mathf.RoundToInt(enemy.HP * hpBuffMultiplier));
        enemy.agent.speed *= speedBuffMultiplier;

        while (IsEnemyInAura(enemy))
        {
            yield return new WaitForSeconds(buffDuration);
        }

        ResetStats(enemy);
    }

    private void ResetStats(enemyAI enemy)
    {
        if (enemy != null)
        {
            enemy.SetHP(Mathf.RoundToInt(enemy.HP / hpBuffMultiplier));
            enemy.agent.speed /= speedBuffMultiplier;
        }
    }

    private bool IsEnemyInAura(enemyAI enemy)
    {
        return Vector3.Distance(transform.position, enemy.transform.position) <= auraRadius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
