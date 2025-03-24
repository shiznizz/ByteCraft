using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    public static GoalManager instance;
    public int goalCount;
    [SerializeField] public TMP_Text goalCountText;
    public string currentObjective;
    [SerializeField] TextMeshProUGUI objectiveText;
    [SerializeField] GameObject menuObjectiveFail;

    private void Awake()
    {
        instance = this;
        currentObjective = "";
        goalCount = 0;
    }

    private void Start()
    {
        
    }

    public void updateGameGoal(int amount)
    {
        //goalCount += amount;
        //goalCountText.text = goalCount.ToString("F0");

        //if (goalCount <= 0)
        //{
        //    gameManager.instance.youWin();
        //}
    }

    public void triggerWin()
    {
        gameManager.instance.youWin();
    }

    public void SetObjectiveText(string objective)
    {
        currentObjective = objective;
        objectiveText.SetText(currentObjective);
    }

    public void objectiveFailed(string failedObj)
    {
/*        switch (lvlIdx)
        {
            case 0: // will likely need to change this assignment later, just temp
                break;
            case 1: // level 2
                break;
            case 2: // level 3
                break;
            case 3: // level 4
                break;
        }*/
        gameManager.instance.switchMenu(menuObjectiveFail);
        objectiveText.SetText(currentObjective);
    }
}
