using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ArcadeMachine : MonoBehaviour
{
    [Header("Arcade Machine Settings")]
    public int miniGameSceneIndex;
    private bool isPlayerInRange = false;
    private int sceneToLoad;

    [Header("UI Interaction Text Settings")]
    [SerializeField] TextMeshProUGUI interactionText;

    private void Start()
    {
        //Initially hide the interaction text
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

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

            //Shows the interaction text
            if (interactionText != null)
            {
                interactionText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Player leaves the trigger zone
        {
            isPlayerInRange = false; // No longer in range

            // Hide the interaction text when player leaves the range
            if (interactionText != null)
            {
                interactionText.gameObject.SetActive(false); // Hide the interaction text
            }
        }
    }
}