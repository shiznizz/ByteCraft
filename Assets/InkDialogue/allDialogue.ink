EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)

VAR CollectKeysQuestId = "CollectKeysQuest"
VAR CollectKeysQuestState = "REQUIREMENTS_NOT_MET"

INCLUDE collectKeysStart.ink
INCLUDE npcTest.ink