using UnityEngine;

public class QuestMarker : MonoBehaviour
{
    [SerializeField] Renderer marker;
    [SerializeField] GameObject particleEffect;

    private QuestState currentQuestState;
    private QuestPoint questPointScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        marker.enabled = false;
        particleEffect.SetActive(false);

        questPointScript = GetComponentInParent<QuestPoint>();
        currentQuestState = questPointScript.GetCurrentQuestState();
    }

    // Update is called once per frame
    void Update()
    {
        currentQuestState = questPointScript.GetCurrentQuestState();
        if (Input.GetButtonDown("Marker") && currentQuestState.Equals(QuestState.IN_PROGRESS) && !questPointScript.startPoint)
        {
            ShowMarker();
        } else if (Input.GetButtonUp("Marker"))
        {
            HideMarker();
        }
    }

    void ShowMarker()
    {
        marker.enabled = true;
        particleEffect.SetActive(true);
    }

    void HideMarker()
    {
        marker.enabled = false;
        particleEffect.SetActive(false);
    }
}
