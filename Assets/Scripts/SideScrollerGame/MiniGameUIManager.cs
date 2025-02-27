using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MiniGameUIManager : MonoBehaviour
{
    [Header("Start Menu")]
    [SerializeField] private GameObject startMenu;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;

    private ArcadeMachine arcadeMachine;

    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseScreen.activeInHierarchy)
                pauseGame(false);
            else
                pauseGame(true);
        }
    }

    #region Start Screen

    public void MiniGameStartMenu()
    {
        startMenu.SetActive(true);
    }

    public void startGame()
    {
        SceneManager.LoadScene(3);
    }

    public void quitStartMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }

    #endregion

    #region Game Over
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        //MiniGameSoundManager.instance.PlaySound(gameOverSound);
    }

    public void miniGameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void miniGameMainMenu()
    {
        SceneManager.LoadScene(4);
    }

    public void miniGameQuit()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }
    #endregion

    #region Pause
    public void pauseGame(bool status)
    {
        pauseScreen.SetActive(status);

        if (status)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    #endregion
}
