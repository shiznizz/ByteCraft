using UnityEngine;

public class KillEnemyQuestStep : QuestStep
{
    [Header("Config")]
    [SerializeField] GameObject enemy;
    [SerializeField] private string enemyName;
    private bool enemyKilled = false;
    private bool isCompleted = false;
    private enemyAI enemyScript;
    public void Start()
    {
        this.enemyScript = enemy.GetComponent<enemyAI>();
        string status = "Kill " + enemyName;
        ChangeState("", status);
        Debug.Log($"Quest step status: {status}");
    }

    public void Update()
    {
        if (!enemyKilled)
        {
            enemyKilled = enemyScript.isDead;
        } else
        {
            if (!isCompleted)
            {
                FinishQuestStep();
                isCompleted = true;
            }
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
