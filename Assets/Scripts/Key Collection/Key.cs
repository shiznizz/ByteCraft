using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]

public class Key : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float respawnTimeSeconds = 0;
    [SerializeField] private int keyGained = 1;

    [Header("Audio")]
    [SerializeField] AudioSource audSource;
    [SerializeField] private float audVol;
    [SerializeField] AudioClip audClip;

    private SphereCollider sphereCollider;
    private Renderer visual;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        visual = GetComponent<Renderer>();
    }

    private void CollectKey()
    {
        audSource.PlayOneShot(audClip, audVol);
        sphereCollider.enabled = false;
        visual.gameObject.SetActive(false);
        GameEventsManager.instance.keyEvents.KeyGained(keyGained);
        GameEventsManager.instance.miscEvents.KeyCollected();
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectKey();
        }
    }
}
