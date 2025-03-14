using UnityEngine;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    private Story story;
    private int currentChoiceIndex = -1;

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
        GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
        GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;
    }

    private void UpdateChoiceIndex(int choiceInex)
    {
        this.currentChoiceIndex = choiceInex;
    }

    private void EnterDialogue(string knotName)
    {
        if (dialoguePlaying)
        {
            return; // don't enter dialogue if we've already entered
        }

        dialoguePlaying = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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
        // make a choice, if applicable
        if (story.currentChoices.Count > 0 && currentChoiceIndex != -1)
        {
            story.ChooseChoiceIndex(currentChoiceIndex);
            // reset choice index for next time
            currentChoiceIndex = -1;
        }

        if (story.canContinue)
        {
            string dialogueLine = story.Continue();

            // handle the case where there's an empty line of dialogue
            // by continuing until we get a line with content
            while (IsLineBlank(dialogueLine) && story.canContinue)
            {
                dialogueLine = story.Continue();
            }
            // handle the case where the last line of dialogue is blank
            // (empty choice, external function, etc...)
            if (IsLineBlank(dialogueLine) && !story.canContinue)
            {
                ExitDialogue();
            }
            else
            {
                GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices);
            }
        }
        else if (story.currentChoices.Count == 0)
        {
            StartCoroutine(ExitDialogue());
        }
    }

    private IEnumerator ExitDialogue()
    {

        yield return null;

        Debug.Log("Exiting Dialogue");

        dialoguePlaying = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // inform other parts of system that we've finished dialogue
        GameEventsManager.instance.dialogueEvents.DialogueFinished();

        story.ResetState(); // reset story state
    }

    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
    }
}
