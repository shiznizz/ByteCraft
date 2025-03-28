using UnityEngine;
using System.Collections.Generic;

public class PressButtonQuestStep : QuestStep
{
    private int buttonsPressed = 0;
    private int buttonsToPress = 4;

    private void Start()
    {
        UpdateState();
    }

    private void Update()
    {
        if (buttonsPressed == buttonsToPress)
        {
            FinishQuestStep();
        }
    }

    private void OnEnable()
    {
        GameEventsManager.instance.miscEvents.onButtonPressed += ButtonPressed;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.miscEvents.onButtonPressed -= ButtonPressed;
    }

    private void ButtonPressed()
    {
        if (buttonsPressed < buttonsToPress)
        {
            buttonsPressed++;
            UpdateState();
        }
    }

    private void UpdateState()
    {
        string state = buttonsPressed.ToString();
        string status = "Found " + buttonsPressed + " / " + buttonsToPress + " buttons.";
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.buttonsPressed = System.Int32.Parse(state);
        UpdateState();
    }
}
