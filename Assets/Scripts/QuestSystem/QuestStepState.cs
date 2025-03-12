using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestStepState
{
    public string state;

    public QuestStepState(string state)
    {
        this.state = state;
    }

    public QuestStepState()
    {
        this.state = "";
    }
}
