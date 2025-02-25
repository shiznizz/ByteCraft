using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MiniGameUIManager : MonoBehaviour
{
    public GameObject menuPause;  // Reference to the pause menu UI
    public Button returnButton;   // Reference to the return button on the pause menu

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause menu is hidden at the start
        menuPause.SetActive(false);

        // Set up the return button listener to return to the main scene
        returnButton.onClick.AddListener(ReturnToMainScene);
    }

    private void Update()
    {
        // Debugging to ensure the ESC key is being detected
        if (Input.GetKeyDown(KeyCode.Escape)) // ESC key to show/hide the menu
        {
            Debug.Log("ESC key pressed!");  // Debug log to check key detection

            if (isPaused)
            {
                ResumeGame(); // Resume the game if the menu is currently visible
            }
            else
            {
                PauseGame(); // Show the pause menu if the game is not paused
            }
        }
    }

    // Pause the game and show the pause menu
    private void PauseGame()
    {
        Debug.Log("Pausing game...");
        isPaused = true;
        menuPause.SetActive(true);   // Show the pause menu
        Time.timeScale = 0;          // Pause the game (stop time)
        Cursor.visible = true;      // Make the cursor visible
        Cursor.lockState = CursorLockMode.Confined; // Allow the cursor to move freely
    }

    // Resume the game and hide the pause menu
    private void ResumeGame()
    {
        Debug.Log("Resuming game...");
        isPaused = false;
        menuPause.SetActive(false);  // Hide the pause menu
        Time.timeScale = 1;          // Resume the game (restore time)
        Cursor.visible = false;     // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center
    }

    // Function called when the "Return to Main Scene" button is clicked
    private void ReturnToMainScene()
    {
        Debug.Log("Returning to main scene...");
        // Load the main scene (replace 0 with your main scene index if needed)
        Time.timeScale = 1; // Ensure time resumes before switching scenes
        SceneManager.LoadScene(0); // Load the main scene (Scene 0 in this case)
    }
}
