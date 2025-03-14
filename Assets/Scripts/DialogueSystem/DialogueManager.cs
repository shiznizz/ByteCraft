using UnityEngine;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    private Story story;

    private bool dialoguePlaying = false;

    private void Awake()
    {
        story = new Story(inkJson.text);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Accept"))
        {
            SubmitPressed();
        }
    }

    private void SubmitPressed()
    {
        if (!dialoguePlaying) return;
        ContinueOrExitStory();
    }

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
        Debug.Log("Entering dialogue.");
        if (dialoguePlaying) return; // don't enter dialogue if we've already entered

        dialoguePlaying = true;

        // inform other parts of system that we've started dialogue
        GameEventsManager.instance.dialogueEvents.DialogueStarted();

        // jump to knot
        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
            Debug.Log("Knot name: " + knotName);
        }
        else
        {
            Debug.LogWarning("Knot name was the empty string when entering dialogue.");
        }

        // kick off the story
        ContinueOrExitStory();
    }

    private void ContinueOrExitStory()
    {
        if (story.canContinue)
        {
            string dialogueLine = story.Continue();
            GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine);
        }
        else
        {
            StartCoroutine(ExitDialogue());
        }
    }

    private IEnumerator ExitDialogue()
    {

        yield return null;

        Debug.Log("Exiting Dialogue");

        dialoguePlaying = false;

        // inform other parts of system that we've finished dialogue
        GameEventsManager.instance.dialogueEvents.DialogueFinished();

        story.ResetState(); // reset story state
    }
}
