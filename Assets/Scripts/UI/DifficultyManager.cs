using UnityEngine;


public enum GameDifficulty { Easy, Normal, Hard }

public class DifficultyManager : MonoBehaviour
{
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

    private void UpdateDifficultyMultipliers()
    {
        // adjust multipliers based on selected difficulty
        switch (currentDifficulty)
        { 
            case GameDifficulty.Easy:
                enemyHealthMultiplier = 0.75f;
                enemyDamageMultiplier = 0.75f;
                break;

            case GameDifficulty.Normal:
                enemyHealthMultiplier = 1f;
                enemyDamageMultiplier = 1f;
                break;

            case GameDifficulty.Hard:
                enemyHealthMultiplier = 1.5f;
                enemyDamageMultiplier = 1.5f;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
