using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class KeyUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI keyText;

    private void OnEnable()
    {
        GameEventsManager.instance.keyEvents.onKeyChange += KeyChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.keyEvents.onKeyChange -= KeyChange;
    }

    private void KeyChange(int key)
    {
        keyText.text = key.ToString();
    }
}
