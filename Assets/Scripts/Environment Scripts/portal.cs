using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class portal : MonoBehaviour
{
    public enum PortalType { teleport, endLevel};

    [Header("Portal Settings")]
    //Set this field in the Inspector for each portal you want to assign
    public int sceneBuildIndex;
    public PortalType type;
    

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (type == PortalType.teleport)
            {
                //Loads the scene specified in the inspector
                SceneManager.LoadScene(sceneBuildIndex);
            }
            else if (type == PortalType.endLevel)
            {
                gameManager.instance.youWin();
            }
        }
    }
}
