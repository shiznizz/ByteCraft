using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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

        if (!isCompleted)
        {
            if (BossQuestManager.instance.isComplete)
            {
                HandlePostKillDelay();

                isCompleted = true;
                FinishQuestStep();
                GoalManager.instance.triggerWin();
            }
        }
    }

    private IEnumerator HandlePostKillDelay()
    {
        // Wait for 5 seconds before triggering the win condition
        yield return new WaitForSeconds(5f);

        //// Mark the quest as complete after the delay
        //isCompleted = true;
        //FinishQuestStep();

        //Trigger the win condition
        //GoalManager.instance.triggerWin();
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
