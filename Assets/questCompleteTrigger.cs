using Unity.VisualScripting;
using UnityEngine;

public class questCompleteTrigger : MonoBehaviour
{
    [SerializeField] GameObject enableObj;
    GameObject quest;


    // Update is called once per frame
    void Update()
    {
        quest = GameObject.FindWithTag("Event");

        if (quest == null)
        {
            enableObj.SetActive(true);
        }
        else
        {
            enableObj.SetActive(false);
        } 
            
    }
}
