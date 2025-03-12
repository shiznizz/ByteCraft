using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class QuestLogScrollingList : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;

    [Header("Quest Log Button")]
    [SerializeField] private GameObject questLogButtonPrefab;

    private Dictionary<string, QuestLogButton> idToButtonMap = new Dictionary<string, QuestLogButton>();

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            QuestInfoSO questInfoTest = ScriptableObject.CreateInstance<QuestInfoSO>();
            questInfoTest.id = "test";
            questInfoTest.displayName = "Test " + i;
            questInfoTest.questStepPrefabs = new GameObject[0];
            Quest quest = new Quest(questInfoTest);
            QuestLogButton questLogButton = CreateButtonIfNotExists(quest, () =>
            {
                Debug.Log("SELECTED: " + questInfoTest.displayName);
            });

            if (i == 0)
            {
                questLogButton.button.Select();
            }
        }
    }

    public QuestLogButton CreateButtonIfNotExists(Quest quest, UnityAction selectAction)
    {
        QuestLogButton questLogButton = null;
        if (!idToButtonMap.ContainsKey(quest.info.id))
        {
            questLogButton = InstantiateQuestLogButton(quest, selectAction);
        }
        else
        {
            questLogButton = idToButtonMap[quest.info.id];
        }
        return questLogButton;
    }

    private QuestLogButton InstantiateQuestLogButton(Quest quest, UnityAction selectAction)
    {
        QuestLogButton questLogButton = Instantiate(questLogButtonPrefab, contentParent.transform).GetComponent<QuestLogButton>();
        questLogButton.gameObject.name = quest.info.id + "_button";
        questLogButton.Initialize(quest.info.displayName, selectAction);
        idToButtonMap[quest.info.id] = questLogButton;
        return questLogButton;
    }
}
