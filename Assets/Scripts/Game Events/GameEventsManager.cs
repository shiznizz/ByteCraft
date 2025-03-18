using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public KeyEvents keyEvents;
    public MiscEvents miscEvents;
    public QuestEvents questEvents;
    public DialogueEvents dialogueEvents;
    public EnemyEvents enemyEvents;
    public ButtonPressEvents buttonPressEvents;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        

        // instantiate events
        keyEvents = new KeyEvents();
        miscEvents = new MiscEvents();
        questEvents = new QuestEvents();
        dialogueEvents = new DialogueEvents();
        enemyEvents = new EnemyEvents();
        buttonPressEvents = new ButtonPressEvents();
    }

}
