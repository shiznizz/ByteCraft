EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)

=== npc ===
Hey there!
Are you looking for a quest?
-> END

=== collectKeysStart ===
Will you collect 5 keys and bring them to my friend over there?
* [Yes]
    Great!
    ~ StartQuest("CollectKeysQuest")
    -> END
* [No]
    Oh, ok then. Come back if you change your mind.
    -> END