using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    public MiscEvents miscEvents;
    public QuestEvents questEvents;
    public KeyEvents keyEvents;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Game Events Manager in the scene.");
        }
        instance = this;

        // instantiate events
        keyEvents = new KeyEvents();
        miscEvents = new MiscEvents();
        questEvents = new QuestEvents();
    }

}
