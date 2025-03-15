EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)

VAR CollectKeysQuestId = "CollectKeysQuest"
VAR VisitLocationQuestId = "VisitLocationQuest"
VAR KillEnemyQuestId = "KillEnemyQuest"

VAR CollectKeysQuestState = "REQUIREMENTS_NOT_MET"
VAR KillEnemyQuestState = "REQUIREMENTS_NOT_MET"
VAR VisitLocationQuestState = "REQUIREMENTS_NOT_MET"

INCLUDE collectKeysStart.ink
INCLUDE npcTest.ink
INCLUDE killEnemyStart.ink


