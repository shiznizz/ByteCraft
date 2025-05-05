=== tutorialStart ===
{ TutorialStartQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> END
}

= requirementsNotMet
Alright, listen up!
We sent you to this station because you're the best shot we've got.
An unknown hostile group has infiltrated the station, and they're making a move on the station's core systems.
If they succeed, this whole place is going up in flames - and it'll take a chunk of the planet's orbit with it.
Your mission is simple: stop them. That means securing key systems, defending critical points, and eliminating hostiles.
Stay sharp out there. The station - and everyone on the planet - is counting on you.

~ StartQuest("VisitLocationQuest2")
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
Great job, but this was just the beginning. Watch your back, and godspeed.
~ FinishQuest("VisitLocationQuest2")
-> END