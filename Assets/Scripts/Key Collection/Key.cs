using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]

public class Key : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int keyGained = 1;

    [Header("Audio")]
    [SerializeField] AudioSource audSource;
    [SerializeField] private float audVol;
    [SerializeField] AudioClip audClip;
    [SerializeField] private ModulatedSoundBank keyPickupSoundBank;

    private SphereCollider sphereCollider;
    private Renderer visual;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        visual = GetComponent<Renderer>();
    }

    private void CollectKey()
    {
        /*keyPickupSoundBank.PlaySpecificExternal(audSource, audClip);*/
        //sphereCollider.enabled = false;
        visual.gameObject.SetActive(false);
        GameEventsManager.instance.keyEvents.KeyGained(keyGained);
        GameEventsManager.instance.miscEvents.KeyCollected();
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioSource playerAudSource = other.gameObject.GetComponent<AudioSource>();
            Debug.Log("Player entered key collider.");
            if (playerAudSource != null) playerAudSource.PlayOneShot(audClip);
            //keyPickupSoundBank.PlaySpecificExternal(audSource, audClip);
            CollectKey();
        }
    }
}
