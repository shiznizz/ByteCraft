using UnityEngine;
[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueKnotName;
    [SerializeField] DialoguePanelUI dialoguePanel;

    [Header("Quest")]
    [SerializeField] private QuestInfoSO questInfoForPoint;

    [Header("Config")]
    [SerializeField] private bool startPoint = true;
    [SerializeField] private bool endPoint = true;

    [Header("Marker")]
    [SerializeField] Renderer model;


    private bool playerIsNear = false;
    private string questId;
    private QuestState currentQuestState;

    private void Awake()
    {
        // model.enabled = false;
        questId = questInfoForPoint.id;

    }

    private void Update()
    {
        SubmitPressed();
        if (Input.GetButtonDown("Marker") && currentQuestState.Equals(QuestState.IN_PROGRESS))
            model.enabled = true;
        else if (Input.GetButtonUp("Marker"))
            model.enabled = false;

        if ((questId == "CollectKeysQuest" || questId == "KillEnemiesQuest" || questId == "PressButtonsQuest") && currentQuestState.Equals(QuestState.CAN_FINISH))
        {
            GameEventsManager.instance.questEvents.FinishQuest(questId);
        }

        //if (Input.GetButtonDown("Accept")) SubmitPressed();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
    }

    private void SubmitPressed()
    {
        if (!playerIsNear)
        {
            return;
        }

        // if we have a knot name defined, try to start dialogue with it
        if (!dialogueKnotName.Equals(""))
        {
            GameEventsManager.instance.dialogueEvents.EnterDialogue(dialogueKnotName);
        }
        // otherwise, start or finish the quest immediately without dialogue
        else
        {

            if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
            {
                GameEventsManager.instance.questEvents.StartQuest(questId);
            }
            else if (currentQuestState.Equals(QuestState.CAN_FINISH)  && endPoint)
            {
                GameEventsManager.instance.questEvents.FinishQuest(questId);
                playerStatManager.instance.upgradeCurrency += 2;
                // GoalManager.instance.triggerWin();
            }

        }

    }

    private void QuestStateChange(Quest quest)
    {
        // only update the quest state if this point has the corresponding quest
        if (quest.info.id.Equals(questId))
        {
            currentQuestState = quest.state;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
            if (questId == "VisitLocationQuest2" && currentQuestState.Equals(QuestState.IN_PROGRESS) && endPoint)
            {
                GameEventsManager.instance.questEvents.AdvanceQuest(questId);
                if (currentQuestState.Equals(QuestState.CAN_FINISH))
                {
                    GameEventsManager.instance.questEvents.FinishQuest(questId);
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            if (dialoguePanel != null)
                dialoguePanel.contentParent.SetActive(false);
        }
    }
}
