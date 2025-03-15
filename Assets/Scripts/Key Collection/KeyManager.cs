using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public class KeyManager : MonoBehaviour
{
    [Header("Configuration")]
    private int startingKeys = 0;

    public int currentKeys { get; private set; }

    private void Awake()
    {
        currentKeys = startingKeys;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.keyEvents.onKeyGained += KeyGained;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.keyEvents.onKeyGained -= KeyGained;
    }

    private void Start()
    {
        GameEventsManager.instance.keyEvents.KeyChange(currentKeys);
    }

    private void KeyGained(int key)
    {
        currentKeys++;
        GameEventsManager.instance.keyEvents.KeyChange(currentKeys);
    }
}
