using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadeMachine : MonoBehaviour
{

    public int miniGameSceneIndex;
    private bool isPlayerInRange = false;
    private int sceneToLoad;

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (SceneManager.GetActiveScene().buildIndex != miniGameSceneIndex)
            {
                //Transition to the mini game scene
                SceneManager.LoadScene(miniGameSceneIndex);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure this is player
        {
            isPlayerInRange = true; // Player is close enough to interact
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Player leaves the trigger zone
        {
            isPlayerInRange = false; // No longer in range
        }
    }
}