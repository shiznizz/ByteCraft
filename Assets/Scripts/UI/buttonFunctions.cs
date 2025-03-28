using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    
    public void resume()
    {
        gameManager.instance.stateUnpause();
    }

    public void restart()
    {
        dataManager.instance.isRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.stateUnpause();
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene("1 Main Menu");
        gameManager.instance.mainMenu();
    }

    public void debugLevel()
    {
        SceneManager.LoadScene(7);
    }

    public void quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void respawnPlayer()
    {
        
        gameManager.instance.playerScript.spawnPlayer();
        gameManager.instance.stateUnpause();
    }
    
    public void switchMenu(GameObject menuToOpen)
    {
        gameManager.instance.switchMenu(menuToOpen,false);
    }

   
}

