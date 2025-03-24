=== level3Start ===
{ level3QuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
The enemy has triggered the lockdown protocol for the central hub.
That means all doors are sealed tight, and you're stuck until it's overriden.
There are four security offices in the corners of the hub.
Each one has a manual override. Get to them, hit the buttons, and lift the lockdown.
And expect heavy resistance. They know what you're up to.
~ StartQuest("PressButtonsQuest")
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