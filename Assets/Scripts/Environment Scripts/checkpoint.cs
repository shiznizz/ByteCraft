using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(flashColor());
        }
    }

    IEnumerator flashColor()
    {
        model.material.color = Color.red;
        gameManager.instance.checkpointPopup.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        model.material.color = Color.white;
        gameManager.instance.checkpointPopup.SetActive(false);
    }
}
