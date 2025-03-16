=== collectKeysStart ===
{ CollectKeysQuestState :
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
Will you collect 5 keys?
* [Yes]
    Great!
    ~ StartQuest("CollectKeysQuest")
    -> END
* [No]
    Oh, ok then. Come back if you change your mind.
    -> END
-> END

= inProgress
How is collecting those keys going?
-> END

= canFinish
Oh? You collected the keys? Good job!
~ FinishQuest("CollectKeysQuest")
-> END

= finished
Thanks for collecting those keys!
-> END