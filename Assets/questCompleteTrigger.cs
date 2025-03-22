using Unity.VisualScripting;
using UnityEngine;

public class questCompleteTrigger : MonoBehaviour
{
    GameObject quest;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        quest = GameObject.FindWithTag("Event");

        if (quest == null)
        {
            this.enabled = true;
        }
    }
}
