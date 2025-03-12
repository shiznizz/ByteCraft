using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestLogUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private QuestLogScrollingList scrollingList;
    [SerializeField] private TextMeshProUGUI questDisplayNameText;
    [SerializeField] private TextMeshProUGUI questStatusText;
    [SerializeField] private TextMeshProUGUI experienceRewardsText;
    [SerializeField] private TextMeshProUGUI questRequirementsText;
    [SerializeField] private TextMeshProUGUI questStepsText;

    public bool isShowing;

    private Button firstSelectedButton;
    private Quest currentQuest;

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    private void Update()
    {
        if (gameManager.instance.isPaused)
        {
            contentParent.SetActive(true);
        }
        else
        {
            contentParent.SetActive(false);
        }
    }

    /*    public void UpdateQuestLogUI(Quest quest)
        {
            currentQuest = quest;  // Track the currently selected quest
            SetQuestDetails(quest);
        }*/

    private void QuestStateChange(Quest quest)
    {
        Debug.Log("Running QuestStateChange");
        // add the button to the scrolling list if not already added
        QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () => {
            SetQuestLogInfo(quest);
        });

        // initialize the first selected button if not already so that it's
        // always the top button
        if (firstSelectedButton == null)
        {
            firstSelectedButton = questLogButton.button;
        }

        // set the button color based on quest state
        questLogButton.SetState(quest.state);
    }

    public void SetQuestLogInfo(Quest quest)
    {
        Debug.Log("Setting quest log info");
        // quest name
        questDisplayNameText.text = quest.info.displayName;

        // status
        //questStatusText.text = quest.GetFullStatusText();

        // steps
        questStepsText.text = "";
        foreach (GameObject questStep in quest.info.questStepPrefabs)
        {
            questStepsText.text += GetQuestStepName(questStep.name) + "\n";
        }

        // requirements
        questRequirementsText.text = "";
        foreach (QuestInfoSO prerequisiteQuestInfo in quest.info.questPrerequisites)
        {
            questRequirementsText.text += prerequisiteQuestInfo.displayName + "\n";
        }

        // rewards
        experienceRewardsText.text = quest.info.experienceReward + " XP";
    }

    private string GetQuestStepName(string name)
    {
        string stepName = "";

        switch (name)
        {
            case "VisitFirstLocationQuestStep":
                stepName = "Visit Location 1";
                break;
            case "VisitSecondLocationQuestStep":
                stepName = "Visit Location 2";
                break;
            case "CollectKeysQuestStep":
                stepName = "Collect 5 keys";
                break;
            default:
                break;
        }

        return stepName;
    }
}