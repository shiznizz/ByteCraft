using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectKeysQuestStep : QuestStep
{
    private int keysCollected = 0;
    private int keysToCollect = 5;

    private void OnEnable()
    {
        // TODO - Set up misc events to detect that a key has been collected
    }

    private void OnDisable()
    {
        // TODO - Set up misc events to detect that a key has been collected
    }

    private void KeyCollected()
    {
        if (keysCollected < keysToCollect)
        {
            keysCollected++;
            UpdateState();
        }

        if (keysCollected >= keysToCollect)
        {
            FinishQuestStep();
        }
    }

    private void UpdateState()
    {
        string state = keysCollected.ToString();
        ChangeState(state);
    }

    protected override void SetQuestStepState(string state)
    {
        this.keysCollected = System.Int32.Parse(state);
        UpdateState();
    }
}
