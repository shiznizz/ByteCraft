=== level4Start ===
{ level4QuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
This is it. They know they're losing, so they're going all in.
They're going to try to overload and destroy the reactor. 
If they succeed, the entire station will go up in flames.
You need to reach the reactor room, hit the shutdown button, and hold your position until the system stabilizes.
They won't make it easy for you. Stand your ground and don't let them near that console.
~ StartQuest("KillEnemiesQuest - 20")
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