=== level2Start ===
{ level2QuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
Bad news.
The enemy higher-ups caught wind of your progress.
They've locked down the elevator to the central hub and stolen the access keys.
We've identified multiple enemy generals carrying those keys.
Your job is simple: find them, take them down, and recover those keys.
No keys, no progress. Make it count.
~ StartQuest("CollectKeysQuest")
-> END

= canStart
...
-> END

= inProgress
...
-> END

= canFinish
...
-> END

= finished
...
-> END