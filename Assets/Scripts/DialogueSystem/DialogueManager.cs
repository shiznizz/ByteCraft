using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private bool dialoguePlaying = false;
    private void OnEnable()
    {
        GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
    }

    private void EnterDialogue(string knotName)
    {
        if (dialoguePlaying) return;
        dialoguePlaying = true;

        Debug.Log("Entering dialogue for knot name: " + knotName);
    }
}
