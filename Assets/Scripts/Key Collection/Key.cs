using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]

public class Key : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float respawnTimeSeconds = 0;
    [SerializeField] private int keyGained = 1;

    private SphereCollider sphereCollider;
    private Renderer visual;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        visual = GetComponent<Renderer>();
    }

    private void CollectKey()
    {
        sphereCollider.enabled = false;
        visual.gameObject.SetActive(false);
        GameEventsManager.instance.keyEvents.KeyGained(keyGained);
        GameEventsManager.instance.miscEvents.KeyCollected();
        StopAllCoroutines();
        StartCoroutine(RespawnAfterTime());
    }

    private IEnumerator RespawnAfterTime()
    {
        yield return new WaitForSeconds(respawnTimeSeconds);
        sphereCollider.enabled = true;
        visual.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }
}
