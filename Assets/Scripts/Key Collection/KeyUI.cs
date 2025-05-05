using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class KeyUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private GameObject keyContentParent;

    private void Start()
    {
        keyContentParent.SetActive(false);
        //Debug.Log("Key UI.");
    }

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
        if (key == 0) return;
        //Debug.Log("Inside of KeyChange");
        keyText.text = "Collected Keys: " + key.ToString() + " / 5";

        StartCoroutine(DisplayKeyCollectedText());
    }

    IEnumerator DisplayKeyCollectedText()
    {
        keyContentParent.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        keyContentParent.SetActive(false);
    }
}
