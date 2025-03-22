using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using Ink.Runtime;

public class BossDialogueManager : MonoBehaviour
{
    public TextAsset inkJson;
    private Story inkStory;

    private void Start()
    {
        inkStory = new Story(inkJson.text);
    }

    public string GetNextDialogue()
    {
        if (inkStory.canContinue)
        {
            return inkStory.Continue();
        }
        return null;
    }

    public void ResetDialogue()
    {
        inkStory.ResetState();
    }
}
