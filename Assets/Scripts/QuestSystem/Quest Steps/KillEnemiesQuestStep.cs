using UnityEngine;

public class KillEnemiesQuestStep : QuestStep
{
    private int enemiesKilled = 0;
    [SerializeField] public int enemiesToKill = 5;

    private void Start()
    {
        UpdateState();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.miscEvents.onEnemyKilled += EnemyKilled;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.miscEvents.onEnemyKilled += EnemyKilled;
    }

    private void EnemyKilled()
    {
        if (enemiesKilled < enemiesToKill)
        {
            enemiesKilled++;
            UpdateState();
        }

        if (enemiesKilled >= enemiesToKill)
        {
            FinishQuestStep();
        }
    }

    private void UpdateState()
    {
        string state = enemiesKilled.ToString();
        string status = "Killed " + enemiesKilled + " / " + enemiesToKill + " enemies.";
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.enemiesKilled = System.Int32.Parse(state);
        UpdateState();
    }
}
