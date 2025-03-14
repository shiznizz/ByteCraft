using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Ink.Runtime;

public class DialogueEvents
{
    public event Action<string> onEnterDialogue;
    public void EnterDialogue(string knotName)
    {
        onEnterDialogue?.Invoke(knotName);
    }

    public event Action onDialogueStarted;
    public void DialogueStarted()
    {
        onDialogueStarted?.Invoke();
    }

    public event Action onDialogueFinished;
    public void DialogueFinished()
    {
        onDialogueFinished?.Invoke();
    }

    public event Action<string, List<Choice>> onDisplayDialogue;
    public void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices)
    {
        onDisplayDialogue?.Invoke(dialogueLine, dialogueChoices);
    }

    public event Action<int> onUpdateChoiceIndex;
    public void UpdateChoiceIndex(int choiceIndex)
    {
        onUpdateChoiceIndex?.Invoke(choiceIndex);
    }
}
