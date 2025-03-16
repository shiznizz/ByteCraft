using UnityEngine;
using TMPro;

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
        goalCount += amount;
        goalCountText.text = goalCount.ToString("F0");

        if (goalCount <= 0)
        {
            gameManager.instance.youWin();
        }
    }

    public void SetObjectiveText(string objective)
    {
        currentObjective = objective;
        objectiveText.SetText(currentObjective);
    }

    public void objectiveFailed(string failedObj)
    {

        gameManager.instance.switchMenu(menuObjectiveFail);
        objectiveText.SetText(currentObjective);
    }
}
