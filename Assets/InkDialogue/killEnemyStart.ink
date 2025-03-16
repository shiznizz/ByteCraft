=== killEnemyStart ===
{ KillEnemyQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
// not possible for this quest, but putting something here
Come back once you've leveled up a bit more.
-> END

= canStart
Will you kill that enemy over there?
* [Yes]
    Great!
    ~ StartQuest("KillEnemyQuest")
    -> END
* [No]
    Oh, ok then. Come back if you change your mind.
    -> END
-> END

= inProgress
Uhhh... you gonna kill that enemy?
-> END

= canFinish
Oh? You killed the enemy? Good job!
~ FinishQuest("KillEnemyQuest")
-> END

= finished
Thanks for killing that enemy!
-> END