EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)

VAR CollectKeysQuestId = "CollectKeysQuest"
VAR VisitLocationQuestId = "VisitLocationQuest"
VAR KillEnemyQuestId = "KillEnemyQuest"
VAR VisitLocationQuestTwoId = "VisitLocationQuest2"

VAR CollectKeysQuestState = "REQUIREMENTS_NOT_MET"
VAR KillEnemyQuestState = "REQUIREMENTS_NOT_MET"
VAR VisitLocationQuestState = "REQUIREMENTS_NOT_MET"
VAR TutorialStartQuestState = "REQUIREMENTS_NOT_MET"
VAR level2QuestState = "REQUIREMENTS_NOT_MET"
VAR level3QuestState = "REQUIREMENTS_NOT_MET"
VAR level4QuestState = "REQUIREMENTS_NOT_MET"

INCLUDE collectKeysStart.ink
INCLUDE npcTest.ink
INCLUDE killEnemyStart.ink
INCLUDE tutorialStart.ink
INCLUDE bossDialogue.ink
INCLUDE level2Start.ink
INCLUDE level3Start.ink
INCLUDE level4Start.ink



