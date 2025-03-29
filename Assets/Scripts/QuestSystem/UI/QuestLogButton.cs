using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class QuestLogButton : MonoBehaviour, ISelectHandler
{
    public Button button { get; private set; }
    private TextMeshProUGUI buttonText;
    private UnityAction onSelectAction;

    // because we're instantiating the button and it may be disabled when we
    // instantiate it, we need to manually initialize anything here.
    public void Initialize(string displayName, UnityAction selectAction)
    {
        this.button = this.GetComponent<Button>();
        this.buttonText = this.GetComponentInChildren<TextMeshProUGUI>();

        this.buttonText.text = displayName;
        this.onSelectAction = selectAction;
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelectAction();
    }

    void UpdateButtonColor(QuestState state)
    {
        switch (state)
        {
            case QuestState.REQUIREMENTS_NOT_MET:
                buttonText.color = Color.red;
                break;
            case QuestState.CAN_START:
            case QuestState.IN_PROGRESS:
                buttonText.color = Color.black;
                break;
            case QuestState.CAN_FINISH:
            case QuestState.FINISHED:
                string newButtonText = "<s>" + buttonText.text + "</s>";
                buttonText.text = newButtonText;
                buttonText.color = Color.green;
                break;
            default:
                Debug.LogWarning("Quest State not recognized by switch statement: " + state);
                break;
        }
    }

    public void SetState(QuestState state)
    {
        switch (state)
        {
            case QuestState.REQUIREMENTS_NOT_MET:
                buttonText.color = Color.red;
                break;
            case QuestState.CAN_START:
            case QuestState.IN_PROGRESS:
                buttonText.color = Color.black;
                break;
            case QuestState.CAN_FINISH:
            case QuestState.FINISHED:
                string newButtonText = "<s>" + buttonText.text + "</s>";
                buttonText.text = newButtonText;
                buttonText.color = Color.green;
                break;
            default:
                Debug.LogWarning("Quest State not recognized by switch statement: " + state);
                break;
        }
    }
}