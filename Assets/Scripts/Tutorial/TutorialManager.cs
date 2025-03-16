using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel; // The UI Panel
    public TextMeshProUGUI hintText;     // The text component in the panel
    public string[] tutorialHints;   // Array to hold different hint messages
    private int currentHintIndex = 0;// Index to track the current hint message

    private void Start()
    {
        tutorialPanel.SetActive(false); // Ensures panel is hidden at the start
    }

    //Method to show the tutorial hint
    public void ShowHint(string hint)
    {
        hintText.text = hint; // Updates the hint text
        tutorialPanel.SetActive(true); // Shos the tutorial panel
    }

    //Hides the tutorial panel
    public void HideHint()
    {
        tutorialPanel.SetActive(false);
    }

    // Optionally, you can complete the tutorial when the last hint is displayed
    public void CompleteTutorial()
    {
        // Check if the tutorial is completed (all hints have been shown)
        if (currentHintIndex >= tutorialHints.Length)
        {
            HideHint();  // Hide the tutorial panel after completion
        }
    }
}
