using System.Collections;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public string hintMessage; // The message to display when this trigger is activated
    public TutorialManager tutorialManager; // Reference to the TutorialManager script
    public bool hintShown = false; // Tracks if the hint was shown already

    private void OnTriggerEnter(Collider other)
    {
        //Only shows the hint if the player enters the trigger
        if (other.CompareTag("Player"))
        {
            //If the hint hasn't been shown yet
            if (!hintShown)
            {
                tutorialManager.ShowHint(hintMessage);
                StartCoroutine(HideTutorialAfterDelay());
            }
        }
    }

    private IEnumerator HideTutorialAfterDelay()
    {
        yield return new WaitForSeconds(3f);  // Wait for 3 seconds
        tutorialManager.HideHint();
        hintShown = true;
    }
}
