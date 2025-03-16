using UnityEngine;

public class KillEnemyQuestStep : QuestStep
{
    [Header("Config")]
    [SerializeField] GameObject enemy;
    [SerializeField] private string enemyName;
    private bool enemyKilled = false;
    private enemyAI enemyScript;
    public void Start()
    {
        this.enemyScript = enemy.GetComponent<enemyAI>();
        string status = "Kill " + enemyName;
        ChangeState("", status);
        Debug.Log($"Quest step status: {status}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FinishQuestStep();
        }
    }

    private void UpdateState()
    {
        string state = "In Progress";
        string status = "Kill " + enemyName;
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string newState)
    {
        UpdateState();
    }


}
