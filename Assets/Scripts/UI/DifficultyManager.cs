using UnityEngine;
using TMPro;


public enum GameDifficulty { Easy, Normal, Hard }

public class DifficultyManager : MonoBehaviour
{

    [SerializeField] TMP_Dropdown DifficultyDropdown;
    public static DifficultyManager instance;

    // difficulty is Normal
    public GameDifficulty currentDifficulty = GameDifficulty.Normal;

    // multipliers for scaling enemy stats
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;

    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            UpdateDifficultyMultipliers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDifficulty(GameDifficulty newDifficulty)
    {
        currentDifficulty = newDifficulty;
        UpdateDifficultyMultipliers();
        Debug.Log("Difficulty set to: " + currentDifficulty.ToString());
    }

    public void SetDifficultyFromIndex(int index)
    {
        SetDifficulty((GameDifficulty)index);
    }


    private void UpdateDifficultyMultipliers()
    {
        // adjust multipliers based on selected difficulty
        switch (currentDifficulty)
        { 
            case GameDifficulty.Easy:
                enemyHealthMultiplier = 0.75f;
                enemyDamageMultiplier = 0.75f;
                DifficultyDropdown.value = 0;
                break;

            case GameDifficulty.Normal:
                enemyHealthMultiplier = 1f;
                enemyDamageMultiplier = 1f;
                DifficultyDropdown.value = 1;
                break;

            case GameDifficulty.Hard:
                enemyHealthMultiplier = 1.5f;
                enemyDamageMultiplier = 1.5f;
                DifficultyDropdown.value = 2;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
