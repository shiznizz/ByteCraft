using System.Runtime.CompilerServices;
using UnityEngine;

public class CollectKeysQuestStep : QuestStep
{
    private int keysCollected = 0;
    private int keysToCollect = 5;

    private void Start()
    {
        UpdateState();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.miscEvents.onKeyCollected += KeyCollected;
        // TODO - Set up misc events to detect that a key has been collected
    }

    private void OnDisable()
    {
        GameEventsManager.instance.miscEvents.onKeyCollected -= KeyCollected;
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
        string status = "Collected " + keysCollected + " / " + keysToCollect + " keys.";
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.keysCollected = System.Int32.Parse(state);
        UpdateState();
    }
}
