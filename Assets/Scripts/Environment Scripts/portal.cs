using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class portal : MonoBehaviour
{
    public enum PortalType { teleport, endLevel};

    [Header("Portal Settings")]
    //Set this field in the Inspector for each portal you want to assign
    public string sceneName;
    public PortalType type;
    private GameObject quest;
    public bool questCompleteRequired = true;
    

    private void OnTriggerEnter(Collider other)
    {
        quest = GameObject.FindWithTag("Event");

        if (other.CompareTag("Player") && (!questCompleteRequired || quest == null))
        {
            if (type == PortalType.teleport)
            {
                //Loads the scene specified in the inspector
                SceneManager.LoadScene(sceneName);
            }
            else if (type == PortalType.endLevel)
            {
                gameManager.instance.youWin();
            }
        }
    }
}
