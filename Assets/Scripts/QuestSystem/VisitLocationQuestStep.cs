using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class VisitLocationQuestStep : QuestStep
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FinishQuestStep();
        }
    }
    protected override void SetQuestStepState(string newState)
    {
        // no state is needed for this quest step
    }
}
